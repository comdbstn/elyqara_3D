using System.IO;
using Elyqara.Dungeon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Elyqara.EditorTools
{
    // 단계 6 prefab/씬/asset 통합. [Tools/Elyqara/Setup Phase 6] 한 번 누르면 끝.
    // - StartRoom RoomData SO 생성 (isStartRoom = true)
    // - StartRoom.prefab 에 Room component + 4 door socket child + SpawnPoint child 추가
    // - Player.prefab 에 PlayerSpawnPositioner component 추가
    // - 활성 씬에 [DungeonManager] GameObject 추가
    // 멱등 — 재실행 시 이미 있는 것 skip, Door 위치만 갱신.
    public static class Phase6Setup
    {
        private const string RoomDataPath = "Assets/_Project/Data/Rooms/StartRoom.asset";
        private const string StartRoomPrefabPath = "Assets/_Project/Prefabs/Dungeon/Rooms/StartRoom.prefab";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Networking/Player.prefab";

        [MenuItem("Tools/Elyqara/Setup Phase 6")]
        public static void Run()
        {
            var roomData = EnsureRoomData();
            EnsureStartRoomPrefab(roomData);
            EnsurePlayerPrefab();
            EnsureDungeonManagerInScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase6Setup] 완료. StartRoom.asset / StartRoom.prefab / Player.prefab / [DungeonManager] 처리됨.");
        }

        private static RoomData EnsureRoomData()
        {
            var existing = AssetDatabase.LoadAssetAtPath<RoomData>(RoomDataPath);
            if (existing != null)
            {
                Debug.Log($"[Phase6Setup] RoomData 이미 존재: {RoomDataPath}");
                return existing;
            }

            var dir = Path.GetDirectoryName(RoomDataPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var data = ScriptableObject.CreateInstance<RoomData>();
            var so = new SerializedObject(data);
            so.FindProperty("roomName").stringValue = "StartRoom";
            so.FindProperty("description").stringValue = "단계 6 시작 방 — 단계 5 ProBuilder 그레이박스 + 4방향 입구";
            so.FindProperty("isStartRoom").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(data, RoomDataPath);
            Debug.Log($"[Phase6Setup] RoomData 생성: {RoomDataPath}");
            return data;
        }

        private static void EnsureStartRoomPrefab(RoomData data)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StartRoomPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Phase6Setup] StartRoom.prefab not found at {StartRoomPrefabPath}");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(StartRoomPrefabPath);
            try
            {
                var room = contents.GetComponent<Room>();
                if (room == null) room = contents.AddComponent<Room>();

                // Ground 자식의 Renderer.bounds 기준으로 동서남북 끝 중앙 4개 좌표 계산.
                // Ground 못 찾으면 default ±5m fallback.
                var (n, e, s, w, c) = ComputeDoorPositions(contents);

                var doorN = EnsureChild(contents.transform, "Door_N", n);
                var doorE = EnsureChild(contents.transform, "Door_E", e);
                var doorS = EnsureChild(contents.transform, "Door_S", s);
                var doorW = EnsureChild(contents.transform, "Door_W", w);
                var spawn = EnsureChild(contents.transform, "SpawnPoint", c);

                var so = new SerializedObject(room);
                so.FindProperty("data").objectReferenceValue = data;
                so.FindProperty("doorNorth").objectReferenceValue = doorN;
                so.FindProperty("doorEast").objectReferenceValue = doorE;
                so.FindProperty("doorSouth").objectReferenceValue = doorS;
                so.FindProperty("doorWest").objectReferenceValue = doorW;

                var spawnPointsProp = so.FindProperty("spawnPoints");
                spawnPointsProp.arraySize = 1;
                spawnPointsProp.GetArrayElementAtIndex(0).objectReferenceValue = spawn;

                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(contents, StartRoomPrefabPath);
                Debug.Log("[Phase6Setup] StartRoom.prefab 갱신 (Room + 4 doors + SpawnPoint)");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void EnsurePlayerPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Phase6Setup] Player.prefab not found at {PlayerPrefabPath}");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var positioner = contents.GetComponent<PlayerSpawnPositioner>();
                if (positioner != null)
                {
                    Debug.Log("[Phase6Setup] PlayerSpawnPositioner 이미 존재");
                    return;
                }

                contents.AddComponent<PlayerSpawnPositioner>();
                PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
                Debug.Log("[Phase6Setup] Player.prefab 에 PlayerSpawnPositioner 추가");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void EnsureDungeonManagerInScene()
        {
            var existing = Object.FindFirstObjectByType<DungeonManager>();
            if (existing != null)
            {
                Debug.Log("[Phase6Setup] DungeonManager 이미 씬에 존재");
                return;
            }

            var go = new GameObject("[DungeonManager]");
            go.AddComponent<DungeonManager>();
            EditorSceneManager.MarkSceneDirty(go.scene);
            EditorSceneManager.SaveScene(go.scene);
            Debug.Log("[Phase6Setup] 씬에 [DungeonManager] 추가");
        }

        private static Transform EnsureChild(Transform parent, string name, Vector3 localPosition)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                return existing;
            }

            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            return child.transform;
        }

        // Ground (또는 가장 큰 Renderer) bounds 기준 동서남북 끝 중앙 + 중앙 좌표 반환 (root local space).
        private static (Vector3 n, Vector3 e, Vector3 s, Vector3 w, Vector3 center) ComputeDoorPositions(GameObject root)
        {
            var ground = root.transform.Find("Ground");
            Renderer renderer = null;
            if (ground != null) renderer = ground.GetComponent<Renderer>();

            if (renderer == null)
            {
                var renderers = root.GetComponentsInChildren<Renderer>();
                float biggestSize = 0f;
                foreach (var r in renderers)
                {
                    var size = r.bounds.size.x * r.bounds.size.z;
                    if (size > biggestSize) { biggestSize = size; renderer = r; }
                }
            }

            if (renderer == null)
            {
                Debug.LogWarning("[Phase6Setup] Ground/Renderer 못 찾음 — default ±5m fallback.");
                return (new Vector3(0, 0, 5), new Vector3(5, 0, 0), new Vector3(0, 0, -5), new Vector3(-5, 0, 0), new Vector3(0, 1, 0));
            }

            // world bounds → root.transform 의 local space 변환 (root pos = 0,0,0 보통이지만 안전)
            var worldBounds = renderer.bounds;
            var rootInv = root.transform.worldToLocalMatrix;
            var center = rootInv.MultiplyPoint3x4(worldBounds.center);
            var north = rootInv.MultiplyPoint3x4(new Vector3(worldBounds.center.x, worldBounds.center.y, worldBounds.max.z));
            var east = rootInv.MultiplyPoint3x4(new Vector3(worldBounds.max.x, worldBounds.center.y, worldBounds.center.z));
            var south = rootInv.MultiplyPoint3x4(new Vector3(worldBounds.center.x, worldBounds.center.y, worldBounds.min.z));
            var west = rootInv.MultiplyPoint3x4(new Vector3(worldBounds.min.x, worldBounds.center.y, worldBounds.center.z));

            // SpawnPoint = 중앙 + Y 1m (Player capsule pivot 가정)
            var spawn = center + new Vector3(0, 1f, 0);
            return (north, east, south, west, spawn);
        }
    }
}
