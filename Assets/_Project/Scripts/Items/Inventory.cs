using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Items
{
    // Player 의 인벤토리. 호스트 권위 NetworkList<ItemSlot> 동기화.
    // 1차 = 단순 슬롯 누적 (4x6 = 24 슬롯). 단계 7 검증용. drag/drop 은 단계 8+.
    //
    // 효과 누적 — GetTotalEffect(ItemEffectType) 메서드. 단계 9 (데미지 파이프라인) 시 BasicMeleeSkill 이 호출.
    public sealed class Inventory : NetworkBehaviour
    {
        public const int Width = 4;
        public const int Height = 6;
        public const int SlotCount = Width * Height;

        private readonly NetworkList<ItemSlot> _slots = new();

        public NetworkList<ItemSlot> Slots => _slots;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            if (_slots.Count == 0) InitializeEmptySlots();
        }

        private void InitializeEmptySlots()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                _slots.Add(new ItemSlot(itemIndex: -1, count: 0));
            }
        }

        // 호스트 전용. 인벤에 아이템 1개 추가. 빈 슬롯 첫 번째에 넣음. 가득 차면 false.
        public bool TryAddOnServer(int itemIndex)
        {
            if (!IsServer) return false;
            if (itemIndex < 0) return false;

            // 1차 = 같은 itemIndex 의 슬롯 찾아서 count++ 우선. 그 다음 빈 슬롯.
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].itemIndex == itemIndex)
                {
                    _slots[i] = new ItemSlot(itemIndex, _slots[i].count + 1);
                    return true;
                }
            }
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].itemIndex < 0)
                {
                    _slots[i] = new ItemSlot(itemIndex, 1);
                    return true;
                }
            }
            return false;  // 가득 참
        }

        // 효과 누적. ItemEffectType 별 합산 (% 단순 합).
        // 단계 9 (데미지 파이프라인) 시 BasicMeleeSkill 이 호출.
        public float GetTotalEffect(ItemEffectType type)
        {
            var db = ItemDatabase.Instance;
            if (db == null) return 0f;

            float total = 0f;
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.itemIndex < 0 || slot.count <= 0) continue;

                var data = db.Get(slot.itemIndex);
                if (data == null || data.effects == null) continue;

                for (int e = 0; e < data.effects.Length; e++)
                {
                    if (data.effects[e].type == type) total += data.effects[e].value * slot.count;
                }
            }
            return total;
        }
    }
}
