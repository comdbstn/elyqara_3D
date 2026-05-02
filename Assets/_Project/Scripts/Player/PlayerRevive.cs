using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    // E 키 hold 로 가까운 다운 Player 부활. Souls-like 톤 = hold 시간 = 2초.
    // Owner 가 hold 시간 누적 → 완료 시 ServerRpc → 호스트가 target.ReviveServer 호출.
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerResources))]
    public sealed class PlayerRevive : NetworkBehaviour
    {
        [SerializeField] private float reviveRange = 2.5f;
        [SerializeField] private float holdSeconds = 2f;
        [SerializeField] private float reviveHpPercent = 0.5f;  // 부활 후 HP = max * 0.5

        private PlayerInput _input;
        private PlayerResources _resources;
        private float _holdAccum;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _resources = GetComponent<PlayerResources>();
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (_resources != null && _resources.IsDown.Value) return;  // 자기 다운 시 부활 X

            bool pressed = _input.ReviveAction.IsPressed();
            if (!pressed)
            {
                _holdAccum = 0f;
                return;
            }

            _holdAccum += Time.deltaTime;
            if (_holdAccum < holdSeconds) return;

            // hold 완료 → 가까운 다운 Player 검색 + ServerRpc
            ulong targetId;
            if (!TryFindDownedTargetClientId(out targetId))
            {
                _holdAccum = 0f;
                return;
            }

            RequestReviveServerRpc(targetId);
            _holdAccum = 0f;
        }

        private bool TryFindDownedTargetClientId(out ulong clientId)
        {
            clientId = 0;
            if (NetworkManager.Singleton == null) return false;

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            float bestSqr = reviveRange * reviveRange;
            bool found = false;

            for (int i = 0; i < clients.Count; i++)
            {
                var po = clients[i].PlayerObject;
                if (po == null) continue;
                if (po.OwnerClientId == OwnerClientId) continue;  // 자기 제외

                var res = po.GetComponent<PlayerResources>();
                if (res == null || !res.IsDown.Value) continue;

                float sqr = (po.transform.position - transform.position).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    clientId = po.OwnerClientId;
                    found = true;
                }
            }
            return found;
        }

        [ServerRpc]
        private void RequestReviveServerRpc(ulong targetClientId)
        {
            if (NetworkManager.Singleton == null) return;
            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client)) return;

            var po = client.PlayerObject;
            if (po == null) return;

            var targetRes = po.GetComponent<PlayerResources>();
            if (targetRes == null || !targetRes.IsDown.Value) return;

            // 부활 거리 재검증 (남용 방지)
            float sqr = (po.transform.position - transform.position).sqrMagnitude;
            if (sqr > reviveRange * reviveRange * 1.2f) return;  // 약간 여유

            float maxHp = 100f;  // 기본값. PlayerResources 가 자체 max 검증.
            targetRes.ReviveServer(maxHp * reviveHpPercent);
        }
    }
}
