using UnityEngine;
using Elyqara.Skills;

namespace Elyqara.Characters
{
    [CreateAssetMenu(fileName = "Character", menuName = "Elyqara/Character Data")]
    public sealed class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterName = "Unknown";

        [Header("Resources")]
        public float maxHealth = 100f;
        public float maxStamina = 100f;
        public float staminaRegenPerSec = 15f;

        [Header("Skill Slots")]
        [Tooltip("마우스 좌클릭 — 기본공격")]
        public SkillData primarySkill;

        [Tooltip("마우스 우클릭")]
        public SkillData secondarySkill;

        [Tooltip("Q 키 스킬")]
        public SkillData qSkill;

        [Tooltip("스페이스바 — 회피")]
        public SkillData dodgeSkill;
    }
}
