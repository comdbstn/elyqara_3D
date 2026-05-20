using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Elyqara.Networking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public sealed class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private string ipAddress = "127.0.0.1";
        [SerializeField] private ushort port = 7777;

        [Header("Phase 10-A — 글로벌 매니저 dynamic spawn")]
        [Tooltip("호스트 OnServerStarted 시 instantiate + Spawn. GameStateManager + 기타 글로벌 NetworkBehaviour 포함.")]
        [SerializeField] private NetworkObject globalManagersPrefab;

        [Tooltip("NetworkManager.NetworkConfig.PlayerPrefab 이 비어있을 때 자동 wire-up. Lobby 새 NetworkManager 의 default 값 보장용.")]
        [SerializeField] private GameObject playerPrefab;

        private NetworkManager _networkManager;
        private UnityTransport _transport;
        private bool _globalManagersSpawned;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();

            if (_networkManager.NetworkConfig.NetworkTransport == null)
                _networkManager.NetworkConfig.NetworkTransport = _transport;

            // Lobby 의 새 NetworkManager 는 PlayerPrefab inspector 값 비어있을 수 있음.
            // 단계 10-A — 우리가 들고 있는 Player.prefab ref 로 fallback wire-up.
            if (_networkManager.NetworkConfig.PlayerPrefab == null && playerPrefab != null)
                _networkManager.NetworkConfig.PlayerPrefab = playerPrefab;

            _transport.SetConnectionData(ipAddress, port);
        }

        private void OnEnable()
        {
            if (_networkManager == null) _networkManager = GetComponent<NetworkManager>();
            if (_networkManager != null) _networkManager.OnServerStarted += OnServerStarted;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private void OnDisable()
        {
            if (_networkManager != null) _networkManager.OnServerStarted -= OnServerStarted;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        // 호스트 (또는 dedicated server) 시작 시 글로벌 매니저 spawn.
        // Dynamic spawn 채택 = NGO 2.x 권장 패턴 (placed + DDoL 은 late-join sync 깨짐 우려).
        // Networking asmdef 는 Game asmdef 참조 X — 순환 의존 방지 위해 _globalManagersSpawned 플래그로 중복 체크.
        private void OnServerStarted()
        {
            if (globalManagersPrefab == null) return;
            if (_globalManagersSpawned) return;

            var instance = Instantiate(globalManagersPrefab);
            instance.Spawn(destroyWithScene: false);  // 글로벌 매니저 = 씬 전환 시 살아남음. OnNetworkSpawn 에서 DDoL 추가 마크.
            _globalManagersSpawned = true;
        }

#if UNITY_EDITOR
        // NGO UnityTransport 가 Editor PlayMode Stop 시 UDP socket release 누락 케이스 존재.
        // ExitingPlayMode 콜백에서 강제 Shutdown 호출로 다음 Play 시 포트 충돌 방지.
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;
            if (_networkManager == null) return;
            if (_networkManager.IsListening || _networkManager.IsServer || _networkManager.IsClient)
            {
                _networkManager.Shutdown(true);
            }
        }
#endif

        private void OnGUI()
        {
            if (_networkManager.IsClient || _networkManager.IsServer) return;

            GUILayout.BeginArea(new Rect(20, 20, 220, 140));
            GUILayout.Label($"Elyqara_3D\n{ipAddress}:{port}");
            if (GUILayout.Button("Host")) _networkManager.StartHost();
            if (GUILayout.Button("Client")) _networkManager.StartClient();
            if (GUILayout.Button("Server")) _networkManager.StartServer();
            GUILayout.EndArea();
        }
    }
}
