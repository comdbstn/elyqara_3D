using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Items
{
    // 바닥에 떨어진 아이템. NetworkObject 호스트 권위 spawn.
    // ItemDatabase 의 인덱스를 NetworkVariable 로 동기화 — 클라가 같은 인덱스로 ItemData lookup.
    // PlayerPickup 이 F 키 시 ServerRpc 로 호스트에게 픽업 요청.
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public sealed class DroppedItem : NetworkBehaviour, IItem
    {
        private readonly NetworkVariable<int> _itemIndex = new(
            value: -1,
            writePerm: NetworkVariableWritePermission.Server);

        public ItemData Data
        {
            get
            {
                var db = ItemDatabase.Instance;
                return db != null ? db.Get(_itemIndex.Value) : null;
            }
        }

        public int ItemIndex => _itemIndex.Value;

        // 호스트가 Spawn 직후 호출. 클라이언트 동기화는 NetworkVariable 가 처리.
        public void InitializeOnServer(int itemIndex)
        {
            if (!IsServer) return;
            _itemIndex.Value = itemIndex;
        }
    }
}
