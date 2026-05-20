using UnityEngine;
using UnityEngine.UI;
using Elyqara.Player;
using Elyqara.Characters;

namespace Elyqara.Game
{
    // 단계 10-D — Local Player 의 HP/Stamina HUD. uGUI Slider 2장.
    // DDoL 마크되어 모든 씬 살아남음. 본인 PlayerObject 만 추적.
    public sealed class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider staminaSlider;

        private PlayerResources _resources;
        private PlayerCharacterBinder _binder;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_resources == null || _binder == null)
            {
                FindLocalPlayer();
                return;
            }

            CharacterData ch = _binder.Character;
            if (ch == null) return;

            float maxHp = ch.maxHealth > 0f ? ch.maxHealth : 100f;
            float maxSt = ch.maxStamina > 0f ? ch.maxStamina : 100f;

            if (hpSlider != null) hpSlider.value = Mathf.Clamp01(_resources.Health.Value / maxHp);
            if (staminaSlider != null) staminaSlider.value = Mathf.Clamp01(_resources.Stamina.Value / maxSt);
        }

        private void FindLocalPlayer()
        {
            var nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsClient) return;

            var lc = nm.LocalClient;
            if (lc == null || lc.PlayerObject == null) return;

            _resources = lc.PlayerObject.GetComponent<PlayerResources>();
            _binder = lc.PlayerObject.GetComponent<PlayerCharacterBinder>();
        }
    }
}
