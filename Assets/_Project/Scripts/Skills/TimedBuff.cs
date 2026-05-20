using UnityEngine;

namespace Elyqara.Skills
{
    // 단계 13-2 — 시한 buff 컴포넌트. BuffSkill 가 owner 에 AddComponent + Apply 호출.
    // 1차 단순화: 한 컴포넌트 = 한 buff (다중 buff X). M12+ buff stack 시스템.
    // PlayerResources / BasicMeleeSkill 가 GetComponent<TimedBuff> 로 query.
    public sealed class TimedBuff : MonoBehaviour
    {
        public float AttackBonus { get; private set; }   // 0.5 = +50% 데미지
        public float DefenseBonus { get; private set; }  // 0.5 = 받는 피해 50% 감소
        public bool IsActive => Time.time < _endTime;

        private float _endTime;

        public void Apply(float duration, float atkBonus, float defBonus)
        {
            // 기존 buff 가 있으면 덮어쓰기 (재적용 = refresh)
            AttackBonus = atkBonus;
            DefenseBonus = defBonus;
            _endTime = Time.time + duration;
        }
    }
}
