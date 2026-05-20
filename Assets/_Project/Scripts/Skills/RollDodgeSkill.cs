using UnityEngine;

namespace Elyqara.Skills
{
    // 단계 13-2 — Kiyan 구르기. 다크소울 i-frame 정공.
    // 방향 = 현재 이동 입력. 입력 있으면 그 방향 구르기, 없으면 백스텝.
    // dash = PlayerMovement(IDashable) 가 일정 시간 입력 속도 대신 dash 속도 적용 (임펄스 보존).
    [CreateAssetMenu(fileName = "RollDodge", menuName = "Elyqara/Skills/Roll Dodge")]
    public sealed class RollDodgeSkill : SkillData
    {
        [Header("Dash")]
        [Tooltip("dash 속도 (m/s). 거리 ≈ dashSpeed × dashDuration")]
        [SerializeField] private float dashSpeed = 12f;
        [Tooltip("dash 지속 (초). 이 시간 동안 입력 이동 대신 dash 속도 적용")]
        [SerializeField] private float dashDuration = 0.3f;

        [Header("Invincibility")]
        [Tooltip("i-frame 지속 (초). 다크소울 정공 0.3")]
        [SerializeField] private float invincibleSeconds = 0.3f;

        public override void ActivateOnServer(GameObject owner)
        {
            if (owner == null) return;

            var dashable = owner.GetComponent<IDashable>();
            if (dashable != null)
            {
                // 방향 = 현재 이동 입력 (Rigidbody 수평 속도 = PlayerMovement 가 입력으로 설정한 값).
                // 입력 없으면(정지) 백스텝 = 정면 반대.
                Vector3 dir = Vector3.zero;
                var rb = owner.GetComponent<Rigidbody>();
                if (rb != null) { dir = rb.linearVelocity; dir.y = 0f; }

                if (dir.sqrMagnitude > 0.04f)
                {
                    dir.Normalize();
                }
                else
                {
                    dir = -owner.transform.forward;
                    dir.y = 0f;
                    dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
                }

                if (dir != Vector3.zero)
                    dashable.BeginDashServer(dir, dashSpeed, dashDuration);
            }

            var inv = owner.GetComponent<IInvincibilityTarget>();
            if (inv != null) inv.SetInvincibleServer(invincibleSeconds);
        }
    }
}
