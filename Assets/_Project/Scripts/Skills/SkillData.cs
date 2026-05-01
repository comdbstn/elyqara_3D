using UnityEngine;

namespace Elyqara.Skills
{
    public abstract class SkillData : ScriptableObject, ISkill
    {
        [Header("Skill Cost")]
        [SerializeField] private float cooldownSeconds = 1f;
        [SerializeField] private float staminaCost = 0f;

        public float CooldownSeconds => cooldownSeconds;
        public float StaminaCost => staminaCost;

        public abstract void ActivateOnServer(GameObject owner);
    }
}
