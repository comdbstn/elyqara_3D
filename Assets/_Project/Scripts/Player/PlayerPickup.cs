using Elyqara.Items;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    // F 키 픽업. Owner 가 ServerRpc 로 호스트에게 픽업 요청.
    // 호스트가 가까운 DroppedItem 검색 → 가장 가까운 거 픽업 → Player.Inventory 에 추가 → DroppedItem despawn.
    //
    // 분배 메커니즘 X — 먼저 F 키 누른 사람 인벤. 협동의 본질 = 자유 경쟁 (단계 8 = 4명 검증).
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Inventory))]
    public sealed class PlayerPickup : NetworkBehaviour
    {
        [SerializeField] private float pickupRadius = 2.5f;

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
            _input.PickupAction.performed += OnPickupPressed;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            if (_input != null) _input.PickupAction.performed -= OnPickupPressed;
        }

        private void OnPickupPressed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            RequestPickupServerRpc();
        }

        [ServerRpc]
        private void RequestPickupServerRpc()
        {
            if (_inventory == null) return;

            // 가장 가까운 DroppedItem 검색. 1차 = OverlapSphere. 단계 12+ 성능 최적화 시 SpatialIndex.
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
            DroppedItem closest = null;
            float closestSqr = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var dropped = hits[i].GetComponentInParent<DroppedItem>();
                if (dropped == null || !dropped.IsSpawned) continue;

                float sqr = (dropped.transform.position - transform.position).sqrMagnitude;
                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closest = dropped;
                }
            }

            if (closest == null) return;

            int index = closest.ItemIndex;
            if (!_inventory.TryAddOnServer(index)) return;

            // 인벤 추가 성공 → DroppedItem despawn + destroy.
            if (closest.NetworkObject != null && closest.NetworkObject.IsSpawned)
            {
                closest.NetworkObject.Despawn(true);
            }
        }
    }
}
