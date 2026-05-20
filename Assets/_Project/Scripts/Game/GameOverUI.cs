using UnityEngine;

namespace Elyqara.Game
{
    // GameStateManager.IsGameOver polling 으로 panel 토글.
    // 1차 = 단순 polling. 단계 11+ 이벤트 기반 + 재시작 버튼.
    //
    // 단계 10-A — Lobby 씬 placed Canvas 의 자식으로 두고 호스트/클라 둘 다 활성. DDoL 마크되어 모든 씬 살아남음.
    public sealed class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        private void Awake()
        {
            // 씬 전환 시 살아남도록 root GameObject (Canvas) 를 DDoL 마크.
            // panelRoot 는 자식이라 자동 따라옴.
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Update()
        {
            var gsm = GameStateManager.Instance;
            if (gsm == null || panelRoot == null) return;

            bool isGameOver = gsm.IsGameOver.Value;
            if (panelRoot.activeSelf != isGameOver)
                panelRoot.SetActive(isGameOver);
        }
    }
}
