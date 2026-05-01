using System.Collections.Generic;
using UnityEngine;

namespace Elyqara.Dungeon
{
    // 씬 안 모든 Room 수집 + Player spawn 위치 lookup.
    // Room 이 OnEnable 에서 자기 자신 register — Manager singleton 은 씬에 컴포넌트 하나 두면 됨.
    // 절차생성 X. 직접 맵핑 — 사용자가 씬에 방 인스턴스 직접 배치하는 워크플로우.
    public sealed class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance { get; private set; }

        private static readonly List<Room> _rooms = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Register(Room room)
        {
            if (room != null && !_rooms.Contains(room)) _rooms.Add(room);
        }

        public static void Unregister(Room room)
        {
            _rooms.Remove(room);
        }

        public Room GetStartRoom()
        {
            foreach (var room in _rooms)
            {
                if (room != null && room.Data != null && room.Data.IsStartRoom) return room;
            }
            return _rooms.Count > 0 ? _rooms[0] : null;
        }

        public Vector3 GetPlayerSpawnPosition(int playerIndex)
        {
            var startRoom = GetStartRoom();
            if (startRoom == null) return Vector3.zero;

            var points = startRoom.GetSpawnPoints();
            if (points == null || points.Length == 0) return startRoom.Origin.position;

            var slot = points[playerIndex % points.Length];
            return slot != null ? slot.position : startRoom.Origin.position;
        }
    }
}
