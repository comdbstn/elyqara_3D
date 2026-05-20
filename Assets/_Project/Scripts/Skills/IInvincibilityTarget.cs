namespace Elyqara.Skills
{
    // 무적 효과 부여 인터페이스. Player asmdef 가 구현 (PlayerResources).
    // RollDodgeSkill 같은 회피 스킬이 Skills asmdef 안에서 호출 — Player asmdef 직접 의존 회피.
    public interface IInvincibilityTarget
    {
        void SetInvincibleServer(float seconds);
    }
}
