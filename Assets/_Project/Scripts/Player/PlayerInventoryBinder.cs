using Elyqara.Items;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    // Owner-only — 자기 Inventory 를 씬 InventoryUI Singleton 에 Bind + I 키 토글 처리.
    // 다른 Player 의 인벤 표시 X (각자 자기 인벤만 봄).
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Inventory))]
    public sealed class PlayerInventoryBinder : NetworkBehaviour
    {
        private PlayerInput _input;
        private Inventory _inventory;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _inventory = GetComponent<Inventory>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            BindUI();
            _input.InventoryAction.performed += OnInventoryToggle;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            if (_input != null) _input.InventoryAction.performed -= OnInventoryToggle;
            if (InventoryUI.Instance != null) InventoryUI.Instance.Bind(null);
        }

        private void BindUI()
        {
            if (InventoryUI.Instance == null)
            {
                Debug.LogWarning("[PlayerInventoryBinder] InventoryUI 씬에 없음 — Phase7Setup 메뉴 실행 필요");
                return;
            }
            InventoryUI.Instance.Bind(_inventory);
        }

        private void OnInventoryToggle(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            if (InventoryUI.Instance != null) InventoryUI.Instance.TogglePanel();
        }
    }
}
