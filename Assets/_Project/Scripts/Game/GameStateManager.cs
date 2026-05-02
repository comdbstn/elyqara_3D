using System.Collections.Generic;
using Elyqara.Player;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Game
{
    // 모든 Player 다운 감지 → 게임오버 상태 진입.
    // 호스트만 판정. 클라는 _isGameOver NetworkVariable 구독 → UI 표시.
    // 단계 10 진입 시 보스 사망 / 한 런 클리어 처리도 여기 확장.
    public sealed class GameStateManager : NetworkBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public NetworkVariable<bool> IsGameOver = new(
            writePerm: NetworkVariableWritePermission.Server);

        // 호스트가 매 프레임 polling. 1차 단순. 단계 10+ 이벤트 기반 최적화.
        [SerializeField] private float pollIntervalSeconds = 0.5f;
        private float _pollAccum;

        // 클라 측 콜백 (UI 처리 등). UI 은 별도 컴포넌트가 IsGameOver.OnValueChanged 구독.
        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (IsGameOver.Value) return;

            _pollAccum += Time.deltaTime;
            if (_pollAccum < pollIntervalSeconds) return;
            _pollAccum = 0f;

            CheckAllDown();
        }

        private void CheckAllDown()
        {
            if (NetworkManager.Singleton == null) return;
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            if (clients.Count == 0) return;

            int aliveCount = 0;
            for (int i = 0; i < clients.Count; i++)
            {
                var po = clients[i].PlayerObject;
                if (po == null) continue;
                var res = po.GetComponent<PlayerResources>();
                if (res == null) continue;
                if (!res.IsDown.Value) aliveCount++;
            }

            if (aliveCount == 0) IsGameOver.Value = true;
        }

        // 단계 10+ 보스 사망 / 런 클리어 시 호출 예정 (자리만 추가).
        public void RestartRunServer()
        {
            if (!IsServer) return;
            // TODO 단계 10+: 모든 Player HP 회복 + 던전 재생성 + IsGameOver 해제
        }
    }
}
