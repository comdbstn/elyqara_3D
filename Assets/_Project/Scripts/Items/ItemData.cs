using UnityEngine;

namespace Elyqara.Items
{
    // 모든 아이템의 데이터 정의. ScriptableObject 1장 = 아이템 1종.
    // 새 아이템 추가 = SO 1장 생성 + ItemDatabase.items 배열에 등록.
    [CreateAssetMenu(fileName = "Item", menuName = "Elyqara/Item Data")]
    public sealed class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemName = "Unknown";
        [TextArea] public string description;
        public Sprite icon;

        [Header("Grid")]
        [Tooltip("그리드 슬롯 사이즈. 1x1 기본. 2x1 등 확장은 단계 12+.")]
        public Vector2Int gridSize = Vector2Int.one;

        [Header("Effects")]
        public ItemEffect[] effects;
    }
}
