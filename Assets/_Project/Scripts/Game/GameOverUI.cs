using UnityEngine;

namespace Elyqara.Game
{
    // GameStateManager.IsGameOver polling 으로 panel 토글.
    // 1차 = 단순 polling. 단계 11+ 이벤트 기반 + 재시작 버튼.
    public sealed class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

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
