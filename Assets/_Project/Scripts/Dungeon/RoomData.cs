using UnityEngine;

namespace Elyqara.Dungeon
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "Elyqara/Room Data", order = 100)]
    public sealed class RoomData : ScriptableObject
    {
        [SerializeField] private string roomName;
        [TextArea, SerializeField] private string description;
        [SerializeField] private bool isStartRoom;

        public string RoomName => roomName;
        public string Description => description;
        public bool IsStartRoom => isStartRoom;
    }
}
