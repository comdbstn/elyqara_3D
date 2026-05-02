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
                // ★ root Image = slot 배경. Icon child 의 Image 정확히 lookup.
                var iconTr = go.transform.Find("Icon");
                _slotIcons[i] = iconTr != null ? iconTr.GetComponent<Image>() : null;

                var countTr = go.transform.Find("Count");
                _slotCounts[i] = countTr != null ? countTr.GetComponent<Text>() : null;

                // count Text 를 슬롯 전체 영역 + 중앙 정렬 + 작은 폰트 — itemName 폴백 표시용
                if (_slotCounts[i] != null)
                {
                    _slotCounts[i].alignment = TextAnchor.MiddleCenter;
                    _slotCounts[i].fontSize = 12;
                    _slotCounts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                    var rt = _slotCounts[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = new Vector2(2, 2);
                        rt.offsetMax = new Vector2(-2, -2);
                    }
                }
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
                    if (data != null && data.icon != null)
                    {
                        // 정상 icon
                        _slotIcons[i].enabled = true;
                        _slotIcons[i].color = Color.white;
                    }
                    else if (data != null)
                    {
                        // icon 미설정 = placeholder (회색 사각형)
                        _slotIcons[i].enabled = true;
                        _slotIcons[i].color = new Color(0.5f, 0.55f, 0.6f, 1f);
                    }
                    else
                    {
                        _slotIcons[i].enabled = false;
                    }
                }
                if (_slotCounts[i] != null)
                {
                    if (empty || data == null) _slotCounts[i].text = string.Empty;
                    else if (slot.count <= 1) _slotCounts[i].text = ShortName(data.itemName);
                    else _slotCounts[i].text = $"{ShortName(data.itemName)} x{slot.count}";
                }
            }
        }

        // 슬롯 안 표시할 짧은 이름. itemName 의 첫 단어 또는 12자 이내.
        private static string ShortName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return string.Empty;
            int underscore = fullName.IndexOf('_');
            string head = underscore > 0 ? fullName.Substring(0, underscore) : fullName;
            return head.Length > 12 ? head.Substring(0, 12) : head;
        }
    }
}
