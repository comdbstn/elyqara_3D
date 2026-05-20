using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Enemies
{
    // 호스트 시작 시 자기 위치에 적 1마리 NetworkObject Spawn.
    // 단계 4 멀티 검증 후 EnemyManager 로 승격해서 다중/리스폰 처리.
    //
    // 단계 10-A — 씬 전환 패턴 호환:
    // 첫 씬 (Lobby) 에서 host 시작 시점에는 OnServerStarted 콜백으로 발동.
    // 씬 전환 후 새 씬의 placed EnemySpawner 는 OnEnable 시점에 이미 server started 상태이므로
    // 즉시 spawn 시도 (기존 OnServerStarted 콜백은 다시 발동 X).
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject enemyPrefab;
        [SerializeField] private bool spawnOnHostStart = true;

        private NetworkManager _net;
        private bool _spawned;

        // 단계 13 — RuntimeDungeonGenerator 가 코드 생성 후 호출. 즉시 spawn 발동.
        public void Init(NetworkObject prefab)
        {
            enemyPrefab = prefab;
            TrySpawn();
        }

        private void OnEnable()
        {
            if (_net == null) _net = NetworkManager.Singleton;
            if (_net == null) return;
            _net.OnServerStarted += OnServerStarted;

            // 씬 전환 후 진입 케이스 — 이미 서버 시작된 상태면 즉시 spawn.
            if (_net.IsServer) TrySpawn();
        }

        private void OnDisable()
        {
            if (_net != null) _net.OnServerStarted -= OnServerStarted;
        }

        private void OnServerStarted()
        {
            TrySpawn();
        }

        private void TrySpawn()
        {
            if (!spawnOnHostStart || _spawned || enemyPrefab == null) return;
            if (_net == null || !_net.IsServer) return;

            var instance = Instantiate(enemyPrefab, transform.position, transform.rotation);
            instance.Spawn(true);
            _spawned = true;
        }
    }
}
