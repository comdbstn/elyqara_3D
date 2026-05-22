using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elyqara.Game
{
    // 종이 패널 단일 매니저. 3패널(C 인벤 / Z 상태 / X 스킬) 등록 + 토글 + 한 번에 1개만 열림.
    // 2D Elyqara 는 패널마다 Controller 가 흩어져 있었음 — 3D 는 이 매니저 하나로 통합.
    //
    // ★ 입력도 이 매니저가 소유 — UI 패널 입력을 게임플레이 입력(PlayerInput)과 분리.
    //   클라마다 매니저 1개(DDoL) + 로컬 키보드 = 자연히 로컬 전용. asmdef 순환참조 회피.
    public sealed class UIPanelManager : MonoBehaviour
    {
        public static UIPanelManager Instance { get; private set; }

        [SerializeField] private UIPanel inventoryPanel;
        [SerializeField] private UIPanel statusPanel;
        [SerializeField] private UIPanel skillPanel;

        private readonly List<UIPanel> _panels = new();

        private InputAction _inventoryKey;   // C
        private InputAction _statusKey;      // Z
        private InputAction _skillKey;       // X

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // 씬 전환 시 유지 — InventoryUI 등 기존 UI 와 동일 패턴.
            DontDestroyOnLoad(gameObject);
            Register(inventoryPanel);
            Register(statusPanel);
            Register(skillPanel);

            _inventoryKey = new InputAction("PanelInventory", InputActionType.Button, "<Keyboard>/c");
            _statusKey = new InputAction("PanelStatus", InputActionType.Button, "<Keyboard>/z");
            _skillKey = new InputAction("PanelSkill", InputActionType.Button, "<Keyboard>/x");
            _inventoryKey.performed += _ => Toggle(PanelId.Inventory);
            _statusKey.performed += _ => Toggle(PanelId.Status);
            _skillKey.performed += _ => Toggle(PanelId.Skill);
        }

        private void OnEnable()
        {
            _inventoryKey?.Enable();
            _statusKey?.Enable();
            _skillKey?.Enable();
        }

        private void OnDisable()
        {
            _inventoryKey?.Disable();
            _statusKey?.Disable();
            _skillKey?.Disable();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            _inventoryKey?.Dispose();
            _statusKey?.Dispose();
            _skillKey?.Dispose();
        }

        private void Register(UIPanel p)
        {
            if (p != null && !_panels.Contains(p)) _panels.Add(p);
        }

        // 해당 패널 독립 토글. 여러 패널 동시 열림 가능 (Z 좌 / X 중앙 / C 우 = 화면 꽉 참).
        public void Toggle(PanelId id)
        {
            UIPanel target = GetPanel(id);
            if (target != null) target.Toggle();
        }

        public UIPanel GetPanel(PanelId id)
        {
            for (int i = 0; i < _panels.Count; i++)
                if (_panels[i].Id == id) return _panels[i];
            return null;
        }
    }
}
