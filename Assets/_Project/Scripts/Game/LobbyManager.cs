using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elyqara.Game
{
    // 단계 10-C — 로비 매니저. 호스트만 Start 버튼 표시. Start 시 첫 Stage 로 전환.
    // Lobby 씬 placed (NGO placed in-scene NetworkObject 표준).
    //
    // 1차 = OnGUI 단순. 단계 10-D 에서 uGUI 로 폴리싱.
    [RequireComponent(typeof(NetworkObject))]
    public sealed class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private string firstStageScene = "Stage1";

        private void OnGUI()
        {
            if (NetworkManager.Singleton == null) return;
            if (!NetworkManager.Singleton.IsServer) return;

            int connected = NetworkManager.Singleton.ConnectedClientsList.Count;

            GUILayout.BeginArea(new Rect(20, 200, 240, 100));
            GUILayout.Label($"Lobby — Players: {connected}");
            if (GUILayout.Button("Start Run"))
            {
                StartRun();
            }
            GUILayout.EndArea();
        }

        public void StartRun()
        {
            if (!IsServer) return;
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) return;
            if (string.IsNullOrEmpty(firstStageScene)) return;

            NetworkManager.Singleton.SceneManager.LoadScene(firstStageScene, LoadSceneMode.Single);
        }
    }
}
