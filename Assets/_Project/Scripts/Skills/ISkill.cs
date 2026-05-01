using UnityEngine;

namespace Elyqara.Skills
{
    public interface ISkill
    {
        float CooldownSeconds { get; }
        float StaminaCost { get; }
        // owner = 스킬 소유자 (Player). server only 호출 — 호스트 권위 처리
        void ActivateOnServer(GameObject owner);
    }
}
