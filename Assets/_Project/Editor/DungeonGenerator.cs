using Elyqara.Dungeon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Elyqara.EditorTools
{
    // 단계 6 검증용 7-room 자동 생성 + 정렬 도구.
    // Layout: 십자 (StartRoom 동서남북 1개씩) + East 연쇄 2개 = 7개.
    //
    //                    [Room_N]
    //                       |
    //   [Room_W]-[StartRoom]-[Room_E]-[Room_E2]-[Room_E3]
    //                       |
    //                    [Room_S]
    //
    // 각 방 = StartRoom prefab 복제 (같은 mesh). 모양 다양성은 단계 12+ 콘텐츠 확장.
    // 사용자가 메뉴 한 번 누르면 prefab 복제 / RoomData asset 생성 / 씬 인스턴스화 / Door 자동 정렬 일괄.
    public static class DungeonGenerator
    {
        private const string StartRoomPrefabPath = "Assets/_Project/Prefabs/Dungeon/Rooms/StartRoom.prefab";
        private const string RoomsFolder = "Assets/_Project/Prefabs/Dungeon/Rooms";
        private const string RoomDataFolder = "Assets/_Project/Data/Rooms";

        [MenuItem("Tools/Elyqara/Generate Test Dungeon (7 rooms)")]
        public static void GenerateTestDungeon()
        {
            var startRoom = FindStartRoomInScene();
            if (startRoom == null)
            {
                Debug.LogError("[DungeonGen] 씬에 StartRoom 인스턴스 없음. 먼저 Tools/Elyqara/Setup Phase 6 메뉴 실행.");
                return;
            }

            CreateAdjacentRoom(startRoom, DoorDirection.North, "Room_N");
            var roomE = CreateAdjacentRoom(startRoom, DoorDirection.East, "Room_E");
            CreateAdjacentRoom(startRoom, DoorDirection.South, "Room_S");
            CreateAdjacentRoom(startRoom, DoorDirection.West, "Room_W");

            if (roomE != null)
            {
                var roomE2 = CreateAdjacentRoom(roomE, DoorDirection.East, "Room_E2");
                if (roomE2 != null)
                {
                    CreateAdjacentRoom(roomE2, DoorDirection.East, "Room_E3");
                }
            }

            EditorSceneManager.MarkSceneDirty(startRoom.gameObject.scene);
            EditorSceneManager.SaveScene(startRoom.gameObject.scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DungeonGen] 7개 방 layout 생성 완료. 새 방 prefab 6장 + RoomData 6개 + 씬 인스턴스 + Door 자동 정렬.");
        }

        private static Room CreateAdjacentRoom(Room anchor, DoorDirection anchorDir, string newRoomName)
        {
            var newPrefabPath = $"{RoomsFolder}/{newRoomName}.prefab";
            var newDataPath = $"{RoomDataFolder}/{newRoomName}Data.asset";

            var data = EnsureRoomData(newDataPath, newRoomName);
            var newPrefab = EnsureRoomPrefab(newPrefabPath, data);
            if (newPrefab == null) return null;

            var instance = EnsureSceneInstance(newPrefab, newRoomName);
            var newRoom = instance.GetComponent<Room>();
            if (newRoom == null)
            {
                Debug.LogError($"[DungeonGen] {newRoomName} 인스턴스에 Room 컴포넌트 없음");
                return null;
            }

            SnapToAnchor(newRoom, anchor, anchorDir);
            return newRoom;
        }

        private static RoomData EnsureRoomData(string path, string roomName)
        {
            var existing = AssetDatabase.LoadAssetAtPath<RoomData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<RoomData>();
            var so = new SerializedObject(data);
            so.FindProperty("roomName").stringValue = roomName;
            so.FindProperty("description").stringValue = $"테스트 던전 — {roomName} (StartRoom 복제)";
            so.FindProperty("isStartRoom").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static GameObject EnsureRoomPrefab(string path, RoomData data)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            if (!AssetDatabase.CopyAsset(StartRoomPrefabPath, path))
            {
                Debug.LogError($"[DungeonGen] Prefab 복제 실패: {StartRoomPrefabPath} → {path}");
                return null;
            }
            AssetDatabase.Refresh();

            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var room = contents.GetComponent<Room>();
                if (room != null)
                {
                    var so = new SerializedObject(room);
                    so.FindProperty("data").objectReferenceValue = data;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject EnsureSceneInstance(GameObject prefab, string instanceName)
        {
            var existing = FindRoomInScene(instanceName);
            if (existing != null) return existing.gameObject;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = instanceName;
            return instance;
        }

        private static void SnapToAnchor(Room source, Room anchor, DoorDirection anchorDir)
        {
            var oppositeDir = Opposite(anchorDir);
            var anchorDoor = anchor.GetDoorSocket(anchorDir);
            var sourceDoor = source.GetDoorSocket(oppositeDir);

            if (anchorDoor == null || sourceDoor == null)
            {
                Debug.LogError($"[DungeonGen] Door socket 누락. anchor={anchorDoor}, source={sourceDoor}");
                return;
            }

            var delta = anchorDoor.position - sourceDoor.position;
            source.transform.position += delta;
        }

        private static DoorDirection Opposite(DoorDirection dir)
        {
            return dir switch
            {
                DoorDirection.North => DoorDirection.South,
                DoorDirection.East => DoorDirection.West,
                DoorDirection.South => DoorDirection.North,
                DoorDirection.West => DoorDirection.East,
                _ => DoorDirection.North,
            };
        }

        private static Room FindStartRoomInScene()
        {
            var rooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None);
            foreach (var r in rooms)
            {
                if (r.Data != null && r.Data.IsStartRoom) return r;
            }
            return rooms.Length > 0 ? rooms[0] : null;
        }

        private static Room FindRoomInScene(string name)
        {
            var rooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None);
            foreach (var r in rooms)
            {
                if (r.gameObject.name == name) return r;
            }
            return null;
        }
    }
}
