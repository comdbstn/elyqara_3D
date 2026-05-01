using UnityEngine;

namespace Elyqara.Items
{
    // 아이템 효과 1차 풀. 단계 7 = 누적만, 적용은 단계 9 (데미지 파이프라인) 시점.
    // 새 효과 추가 = enum 값 + Inventory.GetTotalEffect() switch 한 줄.
    public enum ItemEffectType
    {
        SlashDamageBonus,
        BluntDamageBonus,
    }

    [System.Serializable]
    public struct ItemEffect
    {
        public ItemEffectType type;

        [Tooltip("0.12 = 12% 증가. 1차 효과 풀 = 데미지 % 누적만.")]
        public float value;
    }
}
