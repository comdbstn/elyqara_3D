using UnityEngine;

namespace Elyqara.Skills
{
    // 단계 13-2 — Kiyan RMB 구르기. 다크소울 i-frame 정공.
    // 정면 방향으로 dash + 짧은 무적. 호스트 권위 — Rigidbody.linearVelocity 직접 조작.
    [CreateAssetMenu(fileName = "RollDodge", menuName = "Elyqara/Skills/Roll Dodge")]
    public sealed class RollDodgeSkill : SkillData
    {
        [Header("Dash")]
        [Tooltip("순간 속도 변화량 (m/s). 거리 약 4m 정합")]
        [SerializeField] private float dashImpulse = 12f;

        [Header("Invincibility")]
        [Tooltip("i-frame 지속 (초). 다크소울 정공 0.3")]
        [SerializeField] private float invincibleSeconds = 0.3f;

        public override void ActivateOnServer(GameObject owner)
        {
            if (owner == null) return;

            var rb = owner.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = owner.transform.forward;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    dir.Normalize();
                    Vector3 v = rb.linearVelocity;
                    v.x = 0f; v.z = 0f;
                    rb.linearVelocity = v;
                    rb.AddForce(dir * dashImpulse, ForceMode.VelocityChange);
                }
            }

            var inv = owner.GetComponent<IInvincibilityTarget>();
            if (inv != null) inv.SetInvincibleServer(invincibleSeconds);
        }
    }
}
