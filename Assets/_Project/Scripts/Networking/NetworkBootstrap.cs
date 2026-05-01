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

        private NetworkManager _networkManager;
        private UnityTransport _transport;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();

            if (_networkManager.NetworkConfig.NetworkTransport == null)
                _networkManager.NetworkConfig.NetworkTransport = _transport;

            _transport.SetConnectionData(ipAddress, port);
        }

#if UNITY_EDITOR
        // NGO UnityTransport 가 Editor PlayMode Stop 시 UDP socket release 누락 케이스 존재.
        // ExitingPlayMode 콜백에서 강제 Shutdown 호출로 다음 Play 시 포트 충돌 방지.
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

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
