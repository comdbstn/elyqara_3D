using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Elyqara.Items
{
    // Player 의 Inventory NetworkList 를 uGUI 4x6 그리드로 표시.
    // 1차 = 자동 슬롯 (drag/drop X). I 키 토글은 PlayerInput 의 InventoryAction 가 처리.
    //
    // Owner Player 만 자기 인벤 UI 표시. 다른 Player 인벤 = 표시 X.
    public sealed class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        [SerializeField] private GameObject panelRoot;       // 토글 대상 (켜고 끄기)
        [SerializeField] private Transform slotsParent;      // GridLayoutGroup 의 부모
        [SerializeField] private GameObject slotPrefab;      // Image + count Text 단순 prefab
        [SerializeField] private bool startVisible = false;

        private Inventory _inventory;
        private Image[] _slotIcons;
        private Text[] _slotCounts;
        private bool _slotsBuilt;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (panelRoot != null) panelRoot.SetActive(startVisible);
        }

        public void Bind(Inventory inventory)
        {
            if (_inventory != null) _inventory.Slots.OnListChanged -= OnSlotsChanged;
            _inventory = inventory;
            if (_inventory != null)
            {
                _inventory.Slots.OnListChanged += OnSlotsChanged;
                BuildSlotsIfNeeded();
                Refresh();
            }
        }

        private void OnDestroy()
        {
            if (_inventory != null) _inventory.Slots.OnListChanged -= OnSlotsChanged;
            if (Instance == this) Instance = null;
        }

        public void TogglePanel()
        {
            if (panelRoot == null) return;
            panelRoot.SetActive(!panelRoot.activeSelf);
            if (panelRoot.activeSelf) Refresh();
        }

        private void BuildSlotsIfNeeded()
        {
            if (_slotsBuilt || slotsParent == null || slotPrefab == null) return;

            _slotIcons = new Image[Inventory.SlotCount];
            _slotCounts = new Text[Inventory.SlotCount];

            for (int i = 0; i < Inventory.SlotCount; i++)
            {
                var go = Instantiate(slotPrefab, slotsParent);
                _slotIcons[i] = go.GetComponentInChildren<Image>();
                _slotCounts[i] = go.GetComponentInChildren<Text>();
            }
            _slotsBuilt = true;
        }

        private void OnSlotsChanged(NetworkListEvent<ItemSlot> _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_inventory == null || _slotIcons == null) return;
            var db = ItemDatabase.Instance;

            for (int i = 0; i < _slotIcons.Length && i < _inventory.Slots.Count; i++)
            {
                var slot = _inventory.Slots[i];
                bool empty = slot.itemIndex < 0 || slot.count <= 0;
                ItemData data = empty ? null : (db != null ? db.Get(slot.itemIndex) : null);

                if (_slotIcons[i] != null)
                {
                    _slotIcons[i].sprite = data != null ? data.icon : null;
                    _slotIcons[i].enabled = data != null && data.icon != null;
                }
                if (_slotCounts[i] != null)
                {
                    _slotCounts[i].text = empty || slot.count <= 1 ? string.Empty : slot.count.ToString();
                }
            }
        }
    }
}
