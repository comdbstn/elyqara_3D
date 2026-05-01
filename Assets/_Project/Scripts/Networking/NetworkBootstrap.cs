using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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

        private void OnGUI()
        {
            if (_networkManager.IsClient || _networkManager.IsServer) return;

            GUILayout.BeginArea(new Rect(20, 20, 220, 140));
            GUILayout.Label($"Elyqara_3D — Phase 1\n{ipAddress}:{port}");
            if (GUILayout.Button("Host")) _networkManager.StartHost();
            if (GUILayout.Button("Client")) _networkManager.StartClient();
            if (GUILayout.Button("Server")) _networkManager.StartServer();
            GUILayout.EndArea();
        }
    }
}
