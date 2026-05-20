using Elyqara.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elyqara.Game
{
    // 다음 스테이지 전환 트리거. Player 진입 시 호스트가 NGO SceneManager 로 다음 씬 로드.
    // 1차 = Player 1명만 진입해도 즉시 전환. 단계 12+ = 모든 Player 진입 시 또는 Vote.
    //
    // 단계 10 fix:
    // - 다운된 Player 는 trigger 발동 X (다른 Player 가 끌고가는 케이스 차단)
    // - 다음 씬 로드 시 모든 다운 Player 자동 부활은 GameStateManager.OnLoadEventCompleted 가 처리
    [RequireComponent(typeof(Collider))]
    public sealed class StageTrigger : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "Stage2";
        [SerializeField] private bool oneShot = true;

        private bool _triggered;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        // 단계 13 — RuntimeDungeonGenerator 가 코드 생성 후 호출.
        public void Init(string sceneName)
        {
            nextSceneName = sceneName;
        }

        private void OnTriggerEnter(Collider other)
        {
            // NGO 2.x — NetworkBehaviour 미사용 (RPC/NetworkVariable 없음). NetworkManager 직접 체크.
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (oneShot && _triggered) return;

            // Player NetworkObject 검증 (Wisp 등 다른 NetworkObject 차단)
            var no = other.GetComponentInParent<NetworkObject>();
            if (no == null || !no.IsPlayerObject) return;

            // 다운 Player 는 trigger 발동 X — 다른 Player 가 끌고가는 케이스 차단
            var res = no.GetComponent<PlayerResources>();
            if (res != null && res.IsDown.Value) return;

            _triggered = true;
            LoadNextStage();
        }

        private void LoadNextStage()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("[StageTrigger] nextSceneName 미설정");
                return;
            }
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            var sm = NetworkManager.Singleton.SceneManager;
            if (sm == null)
            {
                Debug.LogError("[StageTrigger] NGO SceneManager null — Enable Scene Management 필수");
                return;
            }

            sm.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
