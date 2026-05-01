using UnityEngine;

namespace Elyqara.Enemies
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Elyqara/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Unknown";

        [Header("Resources")]
        public float maxHealth = 120f;

        [Header("Movement")]
        public float moveSpeed = 3f;
        public float turnSpeedDegPerSec = 360f;

        [Header("Aggro")]
        [Tooltip("Player 감지 반경 — 안에 들어오면 Chase")]
        public float aggroRadius = 8f;
        [Tooltip("Player 가 이 거리 밖으로 나가면 Idle 복귀")]
        public float deaggroRadius = 14f;

        [Header("Attack")]
        public float attackRange = 2f;
        public float attackDamage = 30f;
        public float attackCooldown = 1.8f;
        [Tooltip("Attack 시작 후 데미지 적용까지 windup (telegraph 무게감)")]
        public float attackWindup = 0.5f;
    }
}
