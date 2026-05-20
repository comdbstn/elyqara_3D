using UnityEngine;

namespace Elyqara.Skills
{
    // 캐릭터 애니메이터 트리거. enum 이름 = Kiyan.controller 트리거 파라미터명과 1:1 일치.
    public enum CharacterAnim { None, Slash, ShieldBash, Resolve, Roll, Hit }

    public abstract class SkillData : ScriptableObject, ISkill
    {
        [Header("Skill Cost")]
        [SerializeField] private float cooldownSeconds = 1f;
        [SerializeField] private float staminaCost = 0f;

        [Header("Animation")]
        [Tooltip("시전 시 발동할 애니 트리거. PlayerSkillExecutor 가 PlayerAnimator 로 전달.")]
        [SerializeField] private CharacterAnim animTrigger = CharacterAnim.None;

        public float CooldownSeconds => cooldownSeconds;
        public float StaminaCost => staminaCost;
        public CharacterAnim AnimTrigger => animTrigger;

        public abstract void ActivateOnServer(GameObject owner);
    }
}
