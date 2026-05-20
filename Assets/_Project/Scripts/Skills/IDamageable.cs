namespace Elyqara.Skills
{
    // 단계 10 fix — FF off. 같은 진영끼리 데미지 차단 위해 Faction 추가.
    // 단계 12+ FF on 스킬 (도발 / 충격파 등) = ApplyDamageServer 호출자가 Faction 무시.
    public enum DamageFaction
    {
        Player,
        Enemy
    }

    // 호스트 권위 데미지 진입점. Player / Enemy 모두 구현.
    public interface IDamageable
    {
        void ApplyDamageServer(float amount);
        DamageFaction Faction { get; }
    }
}
