namespace Elyqara.Skills
{
    // 호스트 권위 데미지 진입점. Player / Enemy 모두 구현.
    public interface IDamageable
    {
        void ApplyDamageServer(float amount);
    }
}
