using System.IO;
using Elyqara.Enemies;
using Elyqara.Items;
using Elyqara.Player;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Elyqara.EditorTools
{
    // 단계 7 prefab/씬/asset 통합. [Tools/Elyqara/Setup Phase 7] 한 번 메뉴로 끝.
    //
    // 생성/갱신:
    // - 5 ItemData asset (Sword/Shield/Amulet_Slash/Amulet_Blunt/Ring)
    // - ItemDatabase asset (Resources/) — 5 items 등록
    // - WispDropTable asset — 5 entries 균일 가중치
    // - DroppedItem prefab (Resources/) — Capsule + Collider + NetworkObject + DroppedItem
    // - Wisp.asset dropTable ref 갱신
    // - Player.prefab 에 Inventory + PlayerPickup + PlayerInventoryBinder 컴포넌트 추가
    // - 씬 [InventoryCanvas] (Canvas + Panel + GridLayoutGroup + 24 슬롯 인스턴스 + InventoryUI)
    // - InventorySlot.prefab (UI/) — Image + Text 단순 슬롯
    //
    // NetworkPrefabsList 등록 = 사용자 manual (NetworkManager Inspector). Console 안내.
    public static class Phase7Setup
    {
        private const string ItemDataFolder = "Assets/_Project/Data/Items";
        private const string DropTableFolder = "Assets/_Project/Data/DropTables";
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string UIPrefabFolder = "Assets/_Project/Prefabs/UI";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Networking/Player.prefab";
        private const string WispDataPath = "Assets/_Project/Data/Enemies/Wisp.asset";
        private const string ItemDatabasePath = "Assets/_Project/Resources/ItemDatabase.asset";
        private const string DroppedItemPrefabPath = "Assets/_Project/Resources/DroppedItem.prefab";
        private const string SlotPrefabPath = "Assets/_Project/Prefabs/UI/InventorySlot.prefab";
        private const string WispDropTablePath = "Assets/_Project/Data/DropTables/WispDropTable.asset";

        [MenuItem("Tools/Elyqara/Setup Phase 7")]
        public static void Run()
        {
            EnsureFolders();

            var sword = EnsureItemData("Sword_Common", "검 (1차 — Slash 데미지 +12%)", ItemEffectType.SlashDamageBonus, 0.12f);
            var shield = EnsureItemData("Shield_Common", "방패 (1차 — Blunt 데미지 +12%)", ItemEffectType.BluntDamageBonus, 0.12f);
            var amuletSlash = EnsureItemData("Amulet_Slash", "참격 부적 (Slash +18%)", ItemEffectType.SlashDamageBonus, 0.18f);
            var amuletBlunt = EnsureItemData("Amulet_Blunt", "타격 부적 (Blunt +18%)", ItemEffectType.BluntDamageBonus, 0.18f);
            var ring = EnsureRingItemData();

            var items = new[] { sword, shield, amuletSlash, amuletBlunt, ring };

            var database = EnsureItemDatabase(items);
            var dropTable = EnsureWispDropTable(items);
            EnsureWispDropTableRef(dropTable);

            var droppedPrefab = EnsureDroppedItemPrefab();
            var slotPrefab = EnsureInventorySlotPrefab();

            EnsurePlayerComponents();
            EnsureInventoryCanvasInScene(slotPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase7Setup] 완료. 5 ItemData / ItemDatabase / WispDropTable / DroppedItem prefab / Player 컴포넌트 / InventoryCanvas 처리됨.");
            Debug.LogWarning("[Phase7Setup] ★ 수동 작업 필요: NetworkManager Inspector 의 NetworkConfig > NetworkPrefabsList 또는 NetworkPrefabs 에 DroppedItem prefab 추가 부탁드립니다 (NGO 2.x = 동적 spawn 시 prefab 등록 필수).");
        }

        // ==== 폴더 ====
        private static void EnsureFolders()
        {
            EnsureFolder(ItemDataFolder);
            EnsureFolder(DropTableFolder);
            EnsureFolder(ResourcesFolder);
            EnsureFolder(UIPrefabFolder);
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath).Replace("\\", "/");
            string name = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        // ==== ItemData ====
        private static ItemData EnsureItemData(string fileName, string itemName, ItemEffectType type, float value)
        {
            string path = $"{ItemDataFolder}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<ItemData>();
            var so = new SerializedObject(data);
            so.FindProperty("itemName").stringValue = itemName;
            so.FindProperty("description").stringValue = $"{itemName} (단계 7 1차 풀)";

            var effects = so.FindProperty("effects");
            effects.arraySize = 1;
            var effect = effects.GetArrayElementAtIndex(0);
            effect.FindPropertyRelative("type").enumValueIndex = (int)type;
            effect.FindPropertyRelative("value").floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        // Ring = Slash + Blunt 둘 다 +6%. 효과 2개.
        private static ItemData EnsureRingItemData()
        {
            string path = $"{ItemDataFolder}/Ring_Generic.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<ItemData>();
            var so = new SerializedObject(data);
            so.FindProperty("itemName").stringValue = "Ring_Generic";
            so.FindProperty("description").stringValue = "범용 반지 (Slash + Blunt 둘 다 +6%)";

            var effects = so.FindProperty("effects");
            effects.arraySize = 2;

            var e0 = effects.GetArrayElementAtIndex(0);
            e0.FindPropertyRelative("type").enumValueIndex = (int)ItemEffectType.SlashDamageBonus;
            e0.FindPropertyRelative("value").floatValue = 0.06f;

            var e1 = effects.GetArrayElementAtIndex(1);
            e1.FindPropertyRelative("type").enumValueIndex = (int)ItemEffectType.BluntDamageBonus;
            e1.FindPropertyRelative("value").floatValue = 0.06f;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        // ==== ItemDatabase ====
        private static ItemDatabase EnsureItemDatabase(ItemData[] items)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
            ItemDatabase db;
            if (existing != null) db = existing;
            else
            {
                db = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(db, ItemDatabasePath);
            }

            var so = new SerializedObject(db);
            var arr = so.FindProperty("items");
            arr.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                arr.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            return db;
        }

        // ==== WispDropTable ====
        private static DropTableData EnsureWispDropTable(ItemData[] items)
        {
            var existing = AssetDatabase.LoadAssetAtPath<DropTableData>(WispDropTablePath);
            DropTableData table;
            if (existing != null) table = existing;
            else
            {
                table = ScriptableObject.CreateInstance<DropTableData>();
                AssetDatabase.CreateAsset(table, WispDropTablePath);
            }

            var so = new SerializedObject(table);
            var arr = so.FindProperty("entries");
            arr.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                var entry = arr.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("item").objectReferenceValue = items[i];
                entry.FindPropertyRelative("weight").floatValue = 1f;  // 균일
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            return table;
        }

        private static void EnsureWispDropTableRef(DropTableData dropTable)
        {
            var wisp = AssetDatabase.LoadAssetAtPath<EnemyData>(WispDataPath);
            if (wisp == null)
            {
                Debug.LogError($"[Phase7Setup] Wisp.asset not found at {WispDataPath}");
                return;
            }
            var so = new SerializedObject(wisp);
            so.FindProperty("dropTable").objectReferenceValue = dropTable;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ==== DroppedItem prefab (Resources/) ====
        private static GameObject EnsureDroppedItemPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(DroppedItemPrefabPath);
            if (existing != null) return existing;

            // 단순 Capsule visual + Capsule Collider (trigger) + NetworkObject + DroppedItem
            var go = new GameObject("DroppedItem");
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(go.transform, false);
            capsule.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // 시각용 Capsule 의 collider 제거. Trigger collider 는 root 에 추가.
            Object.DestroyImmediate(capsule.GetComponent<Collider>());

            var col = go.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;
            col.height = 1f;

            go.AddComponent<NetworkObject>();
            go.AddComponent<DroppedItem>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, DroppedItemPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ==== InventorySlot prefab (UI/) ====
        private static GameObject EnsureInventorySlotPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);
            if (existing != null) return existing;

            var slot = new GameObject("InventorySlot", typeof(RectTransform), typeof(Image));
            var slotImage = slot.GetComponent<Image>();
            slotImage.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

            // 아이콘 (자식)
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(slot.transform, false);
            var iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(4, 4);
            iconRT.offsetMax = new Vector2(-4, -4);
            var iconImage = icon.GetComponent<Image>();
            iconImage.enabled = false;
            iconImage.preserveAspect = true;

            // count Text (우하단)
            var countGo = new GameObject("Count", typeof(RectTransform), typeof(Text));
            countGo.transform.SetParent(slot.transform, false);
            var countRT = countGo.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0.5f, 0f);
            countRT.anchorMax = Vector2.one;
            countRT.offsetMin = Vector2.zero;
            countRT.offsetMax = new Vector2(-2, -2);
            var countText = countGo.GetComponent<Text>();
            countText.alignment = TextAnchor.LowerRight;
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 14;
            countText.color = Color.white;
            countText.text = string.Empty;

            var prefab = PrefabUtility.SaveAsPrefabAsset(slot, SlotPrefabPath);
            Object.DestroyImmediate(slot);
            return prefab;
        }

        // ==== Player.prefab 컴포넌트 추가 ====
        private static void EnsurePlayerComponents()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Phase7Setup] Player.prefab not found at {PlayerPrefabPath}");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                bool changed = false;
                if (contents.GetComponent<Inventory>() == null) { contents.AddComponent<Inventory>(); changed = true; }
                if (contents.GetComponent<PlayerPickup>() == null) { contents.AddComponent<PlayerPickup>(); changed = true; }
                if (contents.GetComponent<PlayerInventoryBinder>() == null) { contents.AddComponent<PlayerInventoryBinder>(); changed = true; }

                if (changed) PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // ==== 씬 [InventoryCanvas] ====
        private static void EnsureInventoryCanvasInScene(GameObject slotPrefab)
        {
            var existing = Object.FindFirstObjectByType<InventoryUI>();
            if (existing != null)
            {
                Debug.Log("[Phase7Setup] InventoryCanvas 이미 씬에 존재");
                return;
            }

            // Canvas root
            var canvasGo = new GameObject("[InventoryCanvas]", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Panel (toggle 대상)
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(420, 620);
            panelRT.anchoredPosition = Vector2.zero;
            var panelImg = panel.GetComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.75f);

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(panel.transform, false);
            var titleRT = titleGo.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.sizeDelta = new Vector2(0, 50);
            titleRT.anchoredPosition = Vector2.zero;
            var titleText = titleGo.GetComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 22;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.text = "Inventory (I 키로 토글)";

            // SlotsParent (GridLayoutGroup)
            var slotsParent = new GameObject("Slots", typeof(RectTransform), typeof(GridLayoutGroup));
            slotsParent.transform.SetParent(panel.transform, false);
            var slotsRT = slotsParent.GetComponent<RectTransform>();
            slotsRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotsRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotsRT.pivot = new Vector2(0.5f, 0.5f);
            slotsRT.sizeDelta = new Vector2(380, 540);
            slotsRT.anchoredPosition = new Vector2(0, -20);

            var grid = slotsParent.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80, 80);
            grid.spacing = new Vector2(8, 8);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Inventory.Width;

            // InventoryUI 컴포넌트 (Canvas root 에 박음)
            var ui = canvasGo.AddComponent<InventoryUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("slotsParent").objectReferenceValue = slotsParent.transform;
            so.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
            so.FindProperty("startVisible").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();

            // EventSystem 보장 (uGUI 입력 필수)
            EnsureEventSystem();

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            EditorSceneManager.SaveScene(canvasGo.scene);
        }

        private static void EnsureEventSystem()
        {
            var existing = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existing != null) return;
            var go = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }
    }
}
