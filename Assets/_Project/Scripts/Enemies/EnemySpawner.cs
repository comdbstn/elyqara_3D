using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Enemies
{
    // 호스트 시작 시 자기 위치에 적 1마리 NetworkObject Spawn.
    // 단계 4 멀티 검증 후 EnemyManager 로 승격해서 다중/리스폰 처리.
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject enemyPrefab;
        [SerializeField] private bool spawnOnHostStart = true;

        private NetworkManager _net;
        private bool _spawned;

        private void Awake()
        {
            _net = NetworkManager.Singleton;
        }

        private void OnEnable()
        {
            if (_net == null) _net = NetworkManager.Singleton;
            if (_net == null) return;
            _net.OnServerStarted += OnServerStarted;
        }

        private void OnDisable()
        {
            if (_net != null) _net.OnServerStarted -= OnServerStarted;
        }

        private void OnServerStarted()
        {
            if (!spawnOnHostStart || _spawned || enemyPrefab == null) return;
            var instance = Instantiate(enemyPrefab, transform.position, transform.rotation);
            instance.Spawn(true);
            _spawned = true;
        }
    }
}
