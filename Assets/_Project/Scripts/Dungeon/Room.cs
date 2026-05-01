using UnityEngine;

namespace Elyqara.Dungeon
{
    public sealed class Room : MonoBehaviour, IRoom
    {
        [SerializeField] private RoomData data;
        [SerializeField] private Transform doorNorth;
        [SerializeField] private Transform doorEast;
        [SerializeField] private Transform doorSouth;
        [SerializeField] private Transform doorWest;
        [SerializeField] private Transform[] spawnPoints;

        public RoomData Data => data;
        public Transform Origin => transform;

        public Transform GetDoorSocket(DoorDirection direction)
        {
            return direction switch
            {
                DoorDirection.North => doorNorth,
                DoorDirection.East => doorEast,
                DoorDirection.South => doorSouth,
                DoorDirection.West => doorWest,
                _ => null
            };
        }

        public Transform[] GetSpawnPoints() => spawnPoints;

        private void OnEnable()
        {
            DungeonManager.Register(this);
        }

        private void OnDisable()
        {
            DungeonManager.Unregister(this);
        }

#if UNITY_EDITOR
        // Scene view 에서 Door socket 위치 + 라벨 시각화. N=cyan / E=red / S=green / W=yellow.
        // 인접 방 align 작업 시 시각적 anchor 역할.
        private void OnDrawGizmos()
        {
            DrawDoor(doorNorth, "N", Color.cyan);
            DrawDoor(doorEast, "E", Color.red);
            DrawDoor(doorSouth, "S", Color.green);
            DrawDoor(doorWest, "W", Color.yellow);

            if (spawnPoints != null)
            {
                Gizmos.color = Color.white;
                foreach (var sp in spawnPoints)
                {
                    if (sp != null) Gizmos.DrawSphere(sp.position, 0.2f);
                }
            }
        }

        private static void DrawDoor(Transform door, string label, Color color)
        {
            if (door == null) return;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(door.position, 0.5f);
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(door.position + Vector3.up * 0.7f, label);
        }
#endif
    }
}
