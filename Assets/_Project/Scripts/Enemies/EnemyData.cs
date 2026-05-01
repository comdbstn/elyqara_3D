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
        [Tooltip("Chase 중 Player 와 유지할 거리 (capsule center 기준). attackRange 보다 살짝 작게. 박치기 방지.")]
        public float chaseStopDistance = 1.7f;

        [Header("Aggro")]
        [Tooltip("Player 가 이 거리 안에 들어오면 Chase 진입")]
        public float aggroRadius = 8f;
        [Tooltip("Player 가 이 거리 밖으로 나가면 Idle 복귀")]
        public float deaggroRadius = 14f;

        [Header("Attack — 4-phase telegraph")]
        [Tooltip("Anticipation 진입 / Active hitbox 도달 거리")]
        public float attackRange = 2f;
        public float attackDamage = 30f;
        [Tooltip("Anticipation phase — 무기 들어올리는 windup. Telegraph 무게감")]
        public float attackWindup = 0.5f;
        [Tooltip("Active phase — hitbox 활성. 표준 0.05~0.2s. 진입 순간 hit 검사")]
        public float attackActive = 0.1f;
        [Tooltip("Recovery phase — swing 후 정지 (후 딜). Souls-like 핵심: 이 동안 Player 가 punish 가능")]
        public float attackRecovery = 0.6f;
        [Tooltip("Cooldown — Recovery 끝나고 다음 Anticipation 까지 wait. 이 동안 Chase 가능")]
        public float attackCooldown = 0.8f;

        [Header("Hitbox — Active phase 콘")]
        [Range(0f, 180f)]
        [Tooltip("콘 반각 (도). forward 와 target 방향 각도가 이 값 이하면 hit.")]
        public float hitboxHalfAngleDeg = 60f;
    }
}
