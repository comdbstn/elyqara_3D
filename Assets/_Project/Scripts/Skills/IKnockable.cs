using UnityEngine;

namespace Elyqara.Skills
{
    // 외부 임펄스 받을 수 있는 대상. EnemyController 가 구현.
    // BasicMeleeSkill (방패 강타 등) 가 hit 시 GetComponent<IKnockable> query 후 호출.
    // Skills asmdef 가 Enemies asmdef 참조 회피용 인터페이스 분리.
    //
    // 1차 단순화: Player 는 구현 X (Souls 톤 = 플레이어 knockback 안 받음). M12+ 백로그.
    public interface IKnockable
    {
        void ApplyKnockbackServer(Vector3 direction, float force, float duration);
    }
}
