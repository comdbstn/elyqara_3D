using UnityEngine;

namespace Elyqara.Enemies
{
    public interface IEnemy
    {
        EnemyData Data { get; }
        float CurrentHealth { get; }
        bool IsAlive { get; }
    }
}
