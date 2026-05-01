using System;
using Unity.Netcode;

namespace Elyqara.Items
{
    // Inventory.NetworkList<ItemSlot> 의 element. INetworkSerializable + IEquatable 표준.
    // itemIndex = ItemDatabase 의 index. count = 누적 개수 (1차 = 단순 누적, 단계 12+ stack cap).
    public struct ItemSlot : INetworkSerializable, IEquatable<ItemSlot>
    {
        public int itemIndex;
        public int count;

        public ItemSlot(int itemIndex, int count)
        {
            this.itemIndex = itemIndex;
            this.count = count;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref itemIndex);
            serializer.SerializeValue(ref count);
        }

        public bool Equals(ItemSlot other)
        {
            return itemIndex == other.itemIndex && count == other.count;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemSlot other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (itemIndex * 397) ^ count;
        }
    }
}
