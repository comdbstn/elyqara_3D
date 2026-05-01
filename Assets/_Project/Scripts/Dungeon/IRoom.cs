using UnityEngine;

namespace Elyqara.Dungeon
{
    public enum DoorDirection
    {
        North,
        East,
        South,
        West
    }

    public interface IRoom
    {
        RoomData Data { get; }
        Transform Origin { get; }
        Transform GetDoorSocket(DoorDirection direction);
        Transform[] GetSpawnPoints();
    }
}
