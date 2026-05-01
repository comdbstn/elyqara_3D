using UnityEngine;

namespace Elyqara.Skills
{
    [CreateAssetMenu(fileName = "BasicMelee", menuName = "Elyqara/Skills/Basic Melee")]
    public sealed class BasicMeleeSkill : SkillData
    {
        [Header("Hit Cone")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float reach = 2.2f;
        [SerializeField, Range(0f, 180f)] private float halfAngleDeg = 60f;
        [SerializeField] private LayerMask hitLayers = ~0;

        public override void ActivateOnServer(GameObject owner)
        {
            if (owner == null) return;

            Vector3 origin = owner.transform.position;
            Vector3 forward = owner.transform.forward;
            Collider[] candidates = Physics.OverlapSphere(origin, reach, hitLayers);
            float cosThreshold = Mathf.Cos(halfAngleDeg * Mathf.Deg2Rad);

            for (int i = 0; i < candidates.Length; i++)
            {
                Collider c = candidates[i];
                if (c == null || c.attachedRigidbody == null) continue;
                GameObject target = c.attachedRigidbody.gameObject;
                if (target == owner) continue;

                Vector3 toTarget = (c.transform.position - origin);
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude < 0.0001f) continue;
                toTarget.Normalize();

                if (Vector3.Dot(forward, toTarget) < cosThreshold) continue;

                IDamageable dmg = target.GetComponent<IDamageable>();
                if (dmg != null) dmg.ApplyDamageServer(damage);
            }
        }
    }
}
