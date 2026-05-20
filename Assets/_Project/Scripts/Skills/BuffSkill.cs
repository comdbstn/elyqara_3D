using UnityEngine;

namespace Elyqara.Skills
{
    // 단계 13-2 — Kiyan Q 결전. 일정 시간 공격력/방어력 강화.
    // owner 에 TimedBuff 컴포넌트 추가/갱신. 다른 스킬이 query.
    [CreateAssetMenu(fileName = "BuffSkill", menuName = "Elyqara/Skills/Buff Skill")]
    public sealed class BuffSkill : SkillData
    {
        [Header("Buff")]
        [Tooltip("지속 시간 (초)")]
        [SerializeField] private float duration = 8f;
        [Tooltip("공격력 보너스. 0.5 = +50%")]
        [SerializeField] private float attackBonus = 0.5f;
        [Tooltip("방어력 보너스 (받는 피해 감소). 0.5 = 50% 감소")]
        [SerializeField] private float defenseBonus = 0.5f;

        public override void ActivateOnServer(GameObject owner)
        {
            if (owner == null) return;
            var buff = owner.GetComponent<TimedBuff>();
            if (buff == null) buff = owner.AddComponent<TimedBuff>();
            buff.Apply(duration, attackBonus, defenseBonus);
        }
    }
}
