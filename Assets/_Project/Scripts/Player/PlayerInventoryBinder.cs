using Elyqara.Items;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    // Owner-only — 자기 Inventory 를 씬 InventoryUI Singleton 에 Bind.
    // 패널 토글(C키)은 UIPanelManager(Elyqara.Game)가 담당 — 여기서 입력 처리 X.
    [RequireComponent(typeof(Inventory))]
    public sealed class PlayerInventoryBinder : NetworkBehaviour
    {
        private Inventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            if (InventoryUI.Instance != null)
                InventoryUI.Instance.Bind(_inventory);
            else
                Debug.LogWarning("[PlayerInventoryBinder] InventoryUI 씬에 없음 — Setup 메뉴 실행 필요");
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            if (InventoryUI.Instance != null) InventoryUI.Instance.Bind(null);
        }
    }
}
