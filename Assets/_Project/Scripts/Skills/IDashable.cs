using UnityEngine;

namespace Elyqara.Skills
{
    // 회피·돌진 스킬 → PlayerMovement 창구. 일정 시간 입력 속도 대신 dash 속도를 적용.
    // (PlayerMovement 가 매 FixedUpdate 속도를 덮어쓰므로 dash 임펄스를 살리려면 이 인터페이스 경유)
    // asmdef 경계용 — Elyqara.Skills 의 RollDodgeSkill 이 Elyqara.Player 의 PlayerMovement 를 호출.
    public interface IDashable
    {
        void BeginDashServer(Vector3 direction, float speed, float duration);
    }
}
