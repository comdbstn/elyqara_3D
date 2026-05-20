using System.Collections.Generic;
using Elyqara.Enemies;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Dungeon
{
    // 단계 13 — 1층 (Stage1+2+3) 자동 미로 생성. Room-and-Corridor 알고리즘.
    // 단계 6 의 직접 맵핑 인프라 (Editor DungeonGenerator / RoomData 7장 / StartRoom prefab) 보존 — 미래 2층/3층 ref.
    //
    // NGO 시드 동기화 = 호스트만 시드 생성 → NetworkVariable<int> → 클라이언트 같은 시드로 결정론적 생성.
    // Server-authoritative = 적/Boss spawn 은 IsServer 만.
    //
    // mesh = Unity primitive Cube + Material (단순 정공). Souls-like = point light 산발.
    // 외부 자료 ref: VAZGRIZ / Catlike Coding Maze / dvdmc unity_maze_generator (단순화 적용).
    public sealed class RuntimeDungeonGenerator : NetworkBehaviour
    {
        [Header("Generation")]
        [SerializeField] private DungeonGenerationData data;

        [Header("Room Data Refs")]
        [SerializeField] private RoomData spawnRoomData;
        [SerializeField] private RoomData genericRoomData;
        [SerializeField] private RoomData exitRoomData;

        [Header("Stage Config")]
        [SerializeField] private string nextSceneName = "";
        [SerializeField] private bool isBossStage = false;

        [Header("Enemy Refs (Host only)")]
        [SerializeField] private NetworkObject genericEnemyPrefab;
        [SerializeField] private NetworkObject bossEnemyPrefab;

        private readonly NetworkVariable<int> _seed = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private bool _generated;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                int seed = Random.Range(1, int.MaxValue);
                _seed.Value = seed;
                Generate(seed);
            }
            else
            {
                if (_seed.Value != 0) Generate(_seed.Value);
                else _seed.OnValueChanged += OnSeedSync;
            }
        }

        public override void OnNetworkDespawn()
        {
            _seed.OnValueChanged -= OnSeedSync;
        }

        private void OnSeedSync(int prev, int now)
        {
            if (_generated || now == 0) return;
            Generate(now);
        }

        private void Generate(int seed)
        {
            if (_generated) return;
            _generated = true;
            if (data == null)
            {
                Debug.LogError("[RuntimeDungeonGenerator] DungeonGenerationData missing");
                return;
            }

            var rng = new System.Random(seed);
            var rooms = PlaceRooms(rng);
            if (rooms.Count == 0)
            {
                Debug.LogError("[RuntimeDungeonGenerator] failed to place any room");
                return;
            }
            AssignRoomKinds(rooms);

            var corridors = ConnectRooms(rooms, rng);
            var roomCells = CollectRoomCells(rooms);
            var floorCells = new HashSet<Vector2Int>(roomCells);
            foreach (var c in corridors)
                foreach (var cell in c.Cells)
                    floorCells.Add(cell);

            var builtRooms = BuildRoomGameObjects(rooms, floorCells);
            BuildCorridorMesh(corridors, floorCells, roomCells);
            BuildNavMesh();

            if (IsServer) AttachServerComponents(rooms, builtRooms);
        }

        // -------- Layout --------

        private struct RoomBox { public int X, Y, W, H; public RoomKind Kind; }
        private enum RoomKind { Generic, Spawn, Exit }
        private struct Corridor { public List<Vector2Int> Cells; }

        private List<RoomBox> PlaceRooms(System.Random rng)
        {
            var rooms = new List<RoomBox>();
            int attempts = 0;
            while (rooms.Count < data.roomCount && attempts < data.maxPlacementAttempts)
            {
                attempts++;
                int w = rng.Next(data.minRoomSize, data.maxRoomSize + 1);
                int h = rng.Next(data.minRoomSize, data.maxRoomSize + 1);
                int x = rng.Next(1, data.gridWidth - w - 1);
                int y = rng.Next(1, data.gridHeight - h - 1);
                var box = new RoomBox { X = x, Y = y, W = w, H = h, Kind = RoomKind.Generic };
                if (!Overlaps(box, rooms)) rooms.Add(box);
            }
            return rooms;
        }

        private bool Overlaps(RoomBox box, List<RoomBox> existing)
        {
            int b = data.roomBuffer;
            foreach (var e in existing)
            {
                if (box.X - b < e.X + e.W + b &&
                    box.X + box.W + b > e.X - b &&
                    box.Y - b < e.Y + e.H + b &&
                    box.Y + box.H + b > e.Y - b)
                    return true;
            }
            return false;
        }

        private void AssignRoomKinds(List<RoomBox> rooms)
        {
            var first = rooms[0]; first.Kind = RoomKind.Spawn; rooms[0] = first;

            int farthestIdx = 0;
            float farthestDist = 0f;
            var c0 = new Vector2(rooms[0].X + rooms[0].W * 0.5f, rooms[0].Y + rooms[0].H * 0.5f);
            for (int i = 1; i < rooms.Count; i++)
            {
                var ci = new Vector2(rooms[i].X + rooms[i].W * 0.5f, rooms[i].Y + rooms[i].H * 0.5f);
                float d = Vector2.Distance(c0, ci);
                if (d > farthestDist) { farthestDist = d; farthestIdx = i; }
            }
            if (farthestIdx > 0)
            {
                var exit = rooms[farthestIdx]; exit.Kind = RoomKind.Exit; rooms[farthestIdx] = exit;
            }
        }

        private List<Corridor> ConnectRooms(List<RoomBox> rooms, System.Random rng)
        {
            // Nearest-neighbor + L-shape. TinyKeep (Delaunay+MST+A*) 단순화.
            var corridors = new List<Corridor>();
            for (int i = 1; i < rooms.Count; i++)
            {
                int closestIdx = 0;
                float closestDist = float.MaxValue;
                var ci = new Vector2(rooms[i].X + rooms[i].W * 0.5f, rooms[i].Y + rooms[i].H * 0.5f);
                for (int j = 0; j < i; j++)
                {
                    var cj = new Vector2(rooms[j].X + rooms[j].W * 0.5f, rooms[j].Y + rooms[j].H * 0.5f);
                    float d = Vector2.Distance(ci, cj);
                    if (d < closestDist) { closestDist = d; closestIdx = j; }
                }
                corridors.Add(MakeCorridor(rooms[closestIdx], rooms[i], rng));
            }
            return corridors;
        }

        private Corridor MakeCorridor(RoomBox a, RoomBox b, System.Random rng)
        {
            var path = new List<Vector2Int>();
            int ax = a.X + a.W / 2, ay = a.Y + a.H / 2;
            int bx = b.X + b.W / 2, by = b.Y + b.H / 2;
            int cx = ax, cy = ay;
            bool horizontalFirst = rng.Next(2) == 0;
            if (horizontalFirst)
            {
                while (cx != bx) { cx += cx < bx ? 1 : -1; path.Add(new Vector2Int(cx, cy)); }
                while (cy != by) { cy += cy < by ? 1 : -1; path.Add(new Vector2Int(cx, cy)); }
            }
            else
            {
                while (cy != by) { cy += cy < by ? 1 : -1; path.Add(new Vector2Int(cx, cy)); }
                while (cx != bx) { cx += cx < bx ? 1 : -1; path.Add(new Vector2Int(cx, cy)); }
            }
            return new Corridor { Cells = path };
        }

        private HashSet<Vector2Int> CollectRoomCells(List<RoomBox> rooms)
        {
            var cells = new HashSet<Vector2Int>();
            foreach (var r in rooms)
            {
                for (int x = r.X; x < r.X + r.W; x++)
                    for (int y = r.Y; y < r.Y + r.H; y++)
                        cells.Add(new Vector2Int(x, y));
            }
            return cells;
        }

        // -------- Build (mesh / GameObject) --------

        private List<Room> BuildRoomGameObjects(List<RoomBox> rooms, HashSet<Vector2Int> floorCells)
        {
            var built = new List<Room>();
            foreach (var box in rooms)
            {
                var roomGo = new GameObject($"Room_{box.Kind}_{box.X}_{box.Y}");
                roomGo.transform.SetParent(transform, false);
                roomGo.transform.localPosition = new Vector3(box.X * data.cellSize, 0f, box.Y * data.cellSize);

                float w = box.W * data.cellSize;
                float h = box.H * data.cellSize;
                float s = data.cellSize;

                BuildFloor(roomGo.transform, "Floor", new Vector3(w * 0.5f, -0.1f, h * 0.5f), new Vector3(w, 0.2f, h));
                BuildCeiling(roomGo.transform, "Ceiling", new Vector3(w * 0.5f, data.wallHeight + 0.1f, h * 0.5f), new Vector3(w, 0.2f, h));

                // 단계 13 fix — cell 단위 wall. 외부 인접 cell 이 floor (corridor) 면 wall 안 만듦 → corridor 진입로 자동 뚫림. 문 X.
                for (int x = box.X; x < box.X + box.W; x++)
                {
                    if (floorCells.Contains(new Vector2Int(x, box.Y + box.H))) continue;
                    float lx = (x - box.X) * s;
                    BuildWall(roomGo.transform, "Wall_N_" + x, new Vector3(lx + s * 0.5f, data.wallHeight * 0.5f, h), new Vector3(s, data.wallHeight, data.wallThickness));
                }
                for (int x = box.X; x < box.X + box.W; x++)
                {
                    if (floorCells.Contains(new Vector2Int(x, box.Y - 1))) continue;
                    float lx = (x - box.X) * s;
                    BuildWall(roomGo.transform, "Wall_S_" + x, new Vector3(lx + s * 0.5f, data.wallHeight * 0.5f, 0f), new Vector3(s, data.wallHeight, data.wallThickness));
                }
                for (int y = box.Y; y < box.Y + box.H; y++)
                {
                    if (floorCells.Contains(new Vector2Int(box.X + box.W, y))) continue;
                    float lz = (y - box.Y) * s;
                    BuildWall(roomGo.transform, "Wall_E_" + y, new Vector3(w, data.wallHeight * 0.5f, lz + s * 0.5f), new Vector3(data.wallThickness, data.wallHeight, s));
                }
                for (int y = box.Y; y < box.Y + box.H; y++)
                {
                    if (floorCells.Contains(new Vector2Int(box.X - 1, y))) continue;
                    float lz = (y - box.Y) * s;
                    BuildWall(roomGo.transform, "Wall_W_" + y, new Vector3(0f, data.wallHeight * 0.5f, lz + s * 0.5f), new Vector3(data.wallThickness, data.wallHeight, s));
                }

                var doorN = MakeMarker(roomGo.transform, "DoorN", new Vector3(w * 0.5f, 0f, h));
                var doorE = MakeMarker(roomGo.transform, "DoorE", new Vector3(w, 0f, h * 0.5f));
                var doorS = MakeMarker(roomGo.transform, "DoorS", new Vector3(w * 0.5f, 0f, 0f));
                var doorW = MakeMarker(roomGo.transform, "DoorW", new Vector3(0f, 0f, h * 0.5f));

                var spawnPoints = new Transform[4];
                for (int i = 0; i < 4; i++)
                {
                    float fx = (i % 2 == 0 ? 0.25f : 0.75f) * w;
                    float fz = (i / 2 == 0 ? 0.25f : 0.75f) * h;
                    spawnPoints[i] = MakeMarker(roomGo.transform, $"SpawnPoint_{i}", new Vector3(fx, 0.5f, fz));
                }

                var room = roomGo.AddComponent<Room>();
                var rd = box.Kind switch
                {
                    RoomKind.Spawn => spawnRoomData,
                    RoomKind.Exit => exitRoomData,
                    _ => genericRoomData
                };
                room.Setup(rd, doorN, doorE, doorS, doorW, spawnPoints);
                built.Add(room);

                if (data.placePointLights)
                {
                    var lightGo = new GameObject("PointLight");
                    lightGo.transform.SetParent(roomGo.transform, false);
                    lightGo.transform.localPosition = new Vector3(w * 0.5f, data.wallHeight * 0.7f, h * 0.5f);
                    var light = lightGo.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = data.lightColor;
                    light.range = data.lightRange;
                    light.intensity = data.lightIntensity;
                }
            }
            return built;
        }

        private void BuildCorridorMesh(List<Corridor> corridors, HashSet<Vector2Int> floorCells, HashSet<Vector2Int> roomCells)
        {
            var corridorRoot = new GameObject("Corridors");
            corridorRoot.transform.SetParent(transform, false);

            var processedCells = new HashSet<Vector2Int>();
            foreach (var c in corridors)
            {
                foreach (var cell in c.Cells)
                {
                    if (roomCells.Contains(cell)) continue;  // 방 cell 은 방이 처리. corridor mesh skip (Z-fight 회피).
                    if (!processedCells.Add(cell)) continue;

                    var cellGo = new GameObject($"Cell_{cell.x}_{cell.y}");
                    cellGo.transform.SetParent(corridorRoot.transform, false);
                    cellGo.transform.localPosition = new Vector3(cell.x * data.cellSize, 0f, cell.y * data.cellSize);

                    float s = data.cellSize;
                    BuildFloor(cellGo.transform, "Floor", new Vector3(s * 0.5f, -0.1f, s * 0.5f), new Vector3(s, 0.2f, s));
                    BuildCeiling(cellGo.transform, "Ceiling", new Vector3(s * 0.5f, data.wallHeight + 0.1f, s * 0.5f), new Vector3(s, 0.2f, s));

                    var north = new Vector2Int(cell.x, cell.y + 1);
                    var south = new Vector2Int(cell.x, cell.y - 1);
                    var east = new Vector2Int(cell.x + 1, cell.y);
                    var west = new Vector2Int(cell.x - 1, cell.y);

                    if (!floorCells.Contains(north))
                        BuildWall(cellGo.transform, "Wall_N", new Vector3(s * 0.5f, data.wallHeight * 0.5f, s), new Vector3(s, data.wallHeight, data.wallThickness));
                    if (!floorCells.Contains(south))
                        BuildWall(cellGo.transform, "Wall_S", new Vector3(s * 0.5f, data.wallHeight * 0.5f, 0f), new Vector3(s, data.wallHeight, data.wallThickness));
                    if (!floorCells.Contains(east))
                        BuildWall(cellGo.transform, "Wall_E", new Vector3(s, data.wallHeight * 0.5f, s * 0.5f), new Vector3(data.wallThickness, data.wallHeight, s));
                    if (!floorCells.Contains(west))
                        BuildWall(cellGo.transform, "Wall_W", new Vector3(0f, data.wallHeight * 0.5f, s * 0.5f), new Vector3(data.wallThickness, data.wallHeight, s));
                }
            }
        }

        // 단계 13 — NavMesh runtime bake. Unity.AI.Navigation 2.0.9.
        // collectObjects = Children — generator 의 자식 GameObject (모든 floor/wall) 만 baking 대상.
        // 적 NavMeshAgent 추적용 필수.
        private void BuildNavMesh()
        {
            var surface = gameObject.GetComponent<NavMeshSurface>();
            if (surface == null) surface = gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.BuildNavMesh();
        }

        private void BuildFloor(Transform parent, string name, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (data.floorMaterial != null) go.GetComponent<MeshRenderer>().sharedMaterial = data.floorMaterial;
        }

        // 단계 13 fix — 천장 추가. 하늘 차폐 (DanMachi 어두운 톤 정합). wall material 재사용.
        private void BuildCeiling(Transform parent, string name, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (data.wallMaterial != null) go.GetComponent<MeshRenderer>().sharedMaterial = data.wallMaterial;
        }

        private void BuildWall(Transform parent, string name, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (data.wallMaterial != null) go.GetComponent<MeshRenderer>().sharedMaterial = data.wallMaterial;
        }

        private Transform MakeMarker(Transform parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        // -------- Server-only (호스트 권위) --------

        private void AttachServerComponents(List<RoomBox> rooms, List<Room> builtRooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                var box = rooms[i];
                var room = builtRooms[i];
                if (room.Data == null) continue;
                if (room.Data.IsStartRoom) continue;

                float w = box.W * data.cellSize;
                float h = box.H * data.cellSize;
                Vector3 roomCenterLocal = new Vector3(w * 0.5f, 0f, h * 0.5f);

                if (room.Data == exitRoomData)
                {
                    if (isBossStage)
                    {
                        if (bossEnemyPrefab != null)
                        {
                            var bossSpawnPos = room.Origin.position + roomCenterLocal + Vector3.up * 0.5f;
                            var bossInstance = Instantiate(bossEnemyPrefab, bossSpawnPos, Quaternion.identity);
                            bossInstance.Spawn(true);
                        }
                    }
                    else if (!string.IsNullOrEmpty(nextSceneName))
                    {
                        var trigger = new GameObject("StageTrigger");
                        trigger.transform.SetParent(room.transform, false);
                        trigger.transform.localPosition = roomCenterLocal + Vector3.up;
                        var col = trigger.AddComponent<BoxCollider>();
                        col.isTrigger = true;
                        col.size = new Vector3(3f, 2f, 3f);
                        var st = trigger.AddComponent<Elyqara.Game.StageTrigger>();
                        st.Init(nextSceneName);
                    }
                }
                else
                {
                    if (genericEnemyPrefab != null)
                    {
                        var spawnerGo = new GameObject("EnemySpawner");
                        spawnerGo.transform.SetParent(room.transform, false);
                        spawnerGo.transform.localPosition = roomCenterLocal;
                        var spawner = spawnerGo.AddComponent<EnemySpawner>();
                        spawner.Init(genericEnemyPrefab);
                    }
                }
            }
        }
    }
}
