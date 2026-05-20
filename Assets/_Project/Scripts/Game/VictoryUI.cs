using UnityEngine;

namespace Elyqara.Game
{
    // 단계 10-B — 보스 처치 시 GameStateManager.IsVictory polling 으로 panel 토글.
    // GameOverUI 와 같은 패턴. DDoL 마크.
    public sealed class VictoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        private void Awake()
        {
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

            bool isVictory = gsm.IsVictory.Value;
            if (panelRoot.activeSelf != isVictory)
                panelRoot.SetActive(isVictory);
        }
    }
}
