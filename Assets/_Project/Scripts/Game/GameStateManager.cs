using System.Collections.Generic;
using Elyqara.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elyqara.Game
{
    // 글로벌 게임 상태. 호스트 OnServerStarted 시 NetworkBootstrap 가 dynamic spawn.
    // 단계 10-A — 씬 전환 시 살아남도록 OnNetworkSpawn 에서 DontDestroyOnLoad.
    //
    // 책임:
    // - IsGameOver: 모든 Player 다운 시 호스트가 set
    // - IsVictory: 보스 처치 시 호스트가 ReportBossDefeatedServer 로 set (단계 10-B)
    // - 씬 전환 시 다운 Player 자동 부활 (Stage 진입 시점, Lobby 제외)
    //
    // Dynamic spawn 채택 이유 = NGO 2.x docs 권장. placed + DDoL 조합은 late-join sync 깨짐 우려.
    public sealed class GameStateManager : NetworkBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public NetworkVariable<bool> IsGameOver = new(
            writePerm: NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> IsVictory = new(
            writePerm: NetworkVariableWritePermission.Server);

        // 호스트가 매 프레임 polling. 1차 단순. 단계 11+ 이벤트 기반 최적화.
        [SerializeField] private float pollIntervalSeconds = 0.5f;
        [SerializeField] private float reviveOnStageLoadPercent = 1f;  // Stage 진입 시 부활 HP percent
        private float _pollAccum;

        public override void OnNetworkSpawn()
        {
            // 단계 10 fix — Singleton dup 시 NGO Spawn 된 NetworkObject 안전 정리.
            if (Instance != null && Instance != this)
            {
                if (NetworkObject != null && NetworkObject.IsSpawned && IsServer)
                    NetworkObject.Despawn(true);
                else
                    Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (IsGameOver.Value || IsVictory.Value) return;

            _pollAccum += Time.deltaTime;
            if (_pollAccum < pollIntervalSeconds) return;
            _pollAccum = 0f;

            CheckAllDown();
        }

        // 단계 10 fix — race 회피. PlayerObject 아직 spawn 전 케이스 (validCount==0) 시 skip.
        private void CheckAllDown()
        {
            if (NetworkManager.Singleton == null) return;
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            if (clients.Count == 0) return;

            int aliveCount = 0;
            int validCount = 0;
            for (int i = 0; i < clients.Count; i++)
            {
                var po = clients[i].PlayerObject;
                if (po == null) continue;
                var res = po.GetComponent<PlayerResources>();
                if (res == null) continue;
                validCount++;
                if (!res.IsDown.Value) aliveCount++;
            }

            if (validCount == 0) return;  // PlayerObject spawn 전 race 회피
            if (aliveCount == 0) IsGameOver.Value = true;
        }

        // 단계 10-B — 보스 처치 시 BossMarker 가 호출.
        public void ReportBossDefeatedServer()
        {
            if (!IsServer) return;
            if (IsGameOver.Value) return;
            IsVictory.Value = true;
        }

        // 단계 10 fix — Stage 진입 시 모든 다운 Player 자동 부활. StageTrigger 와 연동.
        // Lobby 진입 시는 부활 X (의미 X — 다 살아있는 시작 시점).
        private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            if (sceneName == "Lobby") return;
            ReviveAllDownServer(reviveOnStageLoadPercent);
        }

        // 호스트만. 모든 다운 Player 부활. percent 0~1.
        public void ReviveAllDownServer(float reviveHpPercent)
        {
            if (!IsServer || NetworkManager.Singleton == null) return;
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            for (int i = 0; i < clients.Count; i++)
            {
                var po = clients[i].PlayerObject;
                if (po == null) continue;
                var res = po.GetComponent<PlayerResources>();
                if (res == null || !res.IsDown.Value) continue;
                res.ReviveServer(reviveHpPercent);
            }
        }
    }
}
