using UnityEngine;

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

        [Header("Skill Slots — 캐릭터별 동작 SO. 단계 5에서 ISkill 정의 후 타입 교체")]
        [Tooltip("마우스 좌클릭 — 기본공격")]
        public ScriptableObject primarySkill;

        [Tooltip("마우스 우클릭")]
        public ScriptableObject secondarySkill;

        [Tooltip("Q 키 스킬")]
        public ScriptableObject qSkill;

        [Tooltip("스페이스바 — 회피")]
        public ScriptableObject dodgeSkill;
    }
}
