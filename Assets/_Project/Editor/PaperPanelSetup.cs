using Elyqara.Game;
using Elyqara.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Elyqara.EditorTools
{
    // 종이 패널 UI 통합. [Tools/Elyqara/Setup Paper Panels].
    // 기존 [InventoryCanvas] 에 UIPanelManager + 3 UIPanel 추가.
    //
    // ★ 레이아웃 = 2D Elyqara 정확 스펙 (SkillPanelUI / TopSkillPanelUI / InventoryPanelUI 의 EnsureExists 추출):
    //   - Z 상태  = 좌측 엣지   anchor(0,0)-(0,1) pivot(0,0.5)  size(640,-40)  pos(20,0)   종이 좌우반전
    //   - X 스킬  = 상단 중앙   anchor(0.5,1)     pivot(0.5,1)   size(580,860)  pos(0,-20)
    //   - C 인벤  = 우측 엣지   anchor(1,0)-(1,1) pivot(1,0.5)   size(640,-40)  pos(-20,0)
    //   엣지 앵커 — 화면 1/3 타일링 X.
    //
    // 각 UIPanel = 3레이어: Background(종이 frame1) + Content + Paper(AnimatedPanelUI). 재실행 가능.
    public static class PaperPanelSetup
    {
        // 배경 = frame 1 (꽉 찬 종이). frame 36 = 투명.
        private const string BgSpritePath = "Assets/_Project/Resources/UI/MiddleUi/1.png";

        [MenuItem("Tools/Elyqara/Setup Paper Panels")]
        public static void Run()
        {
            var inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null)
            {
                Debug.LogError("[PaperPanelSetup] InventoryUI 없음 — 먼저 [Tools/Elyqara/Setup Phase 7] 실행 필요");
                return;
            }
            var canvasGo = inventoryUI.gameObject;

            var so = new SerializedObject(inventoryUI);
            var invContent = so.FindProperty("panelRoot").objectReferenceValue as GameObject;
            if (invContent == null)
            {
                Debug.LogError("[PaperPanelSetup] InventoryUI.panelRoot 비어있음");
                return;
            }
            // 인벤 콘텐츠 항상 활성 — 패널 가시성은 UIPanel 슬라이드가 관리 (InventoryUI 가 Awake 에서 끄지 않게)
            so.FindProperty("startVisible").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 재실행 대비 — 인벤 내용 임시 구출 후 기존 패널/매니저 제거
            invContent.transform.SetParent(canvasGo.transform, false);
            DestroyChild(canvasGo.transform, "StatusPanel");
            DestroyChild(canvasGo.transform, "SkillPanel");
            DestroyChild(canvasGo.transform, "InventoryPanel");
            var oldMgr = canvasGo.GetComponent<UIPanelManager>();
            if (oldMgr != null) Object.DestroyImmediate(oldMgr);

            var invImg = invContent.GetComponent<Image>();
            if (invImg != null) invImg.enabled = false;   // 인벤 어두운 배경 끔 — 종이 배경 보이도록
            UpdateTitle(invContent, "인벤토리 (C)");

            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgSpritePath);
            if (bgSprite == null)
                Debug.LogWarning($"[PaperPanelSetup] 종이 배경 스프라이트 못 찾음: {BgSpritePath}");

            var manager = canvasGo.AddComponent<UIPanelManager>();

            // Z 상태 — 좌측 엣지 (종이 좌우반전)
            var zPanel = BuildPanel(canvasGo.transform, "StatusPanel", PanelId.Status, SlideFrom.Left,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                new Vector2(640f, -40f), new Vector2(20f, 0f), true, bgSprite, BuildPlaceholder("상 태"));

            // X 스킬 — 상단 중앙
            var xPanel = BuildPanel(canvasGo.transform, "SkillPanel", PanelId.Skill, SlideFrom.Top,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(580f, 860f), new Vector2(0f, -20f), false, bgSprite, BuildPlaceholder("스 킬"));

            // C 인벤 — 우측 엣지
            var cPanel = BuildPanel(canvasGo.transform, "InventoryPanel", PanelId.Inventory, SlideFrom.Right,
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                new Vector2(640f, -40f), new Vector2(-20f, 0f), false, bgSprite, invContent);

            var mso = new SerializedObject(manager);
            mso.FindProperty("inventoryPanel").objectReferenceValue = cPanel;
            mso.FindProperty("statusPanel").objectReferenceValue = zPanel;
            mso.FindProperty("skillPanel").objectReferenceValue = xPanel;
            mso.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            EditorSceneManager.SaveScene(canvasGo.scene);
            Debug.Log("[PaperPanelSetup] 완료 — Z(좌엣지)/X(상단중앙)/C(우엣지) 2D 정확 스펙 · 슬라이드 인 · 독립 토글.");
        }

        // UIPanel = 2D 스펙 RectTransform. 자식 3레이어: Background → Content → Paper.
        private static UIPanel BuildPanel(Transform parent, string name, PanelId id, SlideFrom slideFrom,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos,
            bool flip, Sprite bgSprite, GameObject content)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            var panel = go.AddComponent<UIPanel>();

            // 1) Background — 종이 frame 1
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            Stretch(bg.GetComponent<RectTransform>());
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;
            if (flip) bg.transform.localScale = new Vector3(-1f, 1f, 1f);

            // 2) Content
            content.transform.SetParent(go.transform, false);

            // 3) Paper — 종이 애니 오버레이
            var paperGo = new GameObject("Paper", typeof(RectTransform), typeof(Image), typeof(AnimatedPanelUI));
            paperGo.transform.SetParent(go.transform, false);
            Stretch(paperGo.GetComponent<RectTransform>());
            if (flip) paperGo.transform.localScale = new Vector3(-1f, 1f, 1f);

            // 레이어 순서: Background(아래) → Content → Paper(위)
            bg.transform.SetAsFirstSibling();
            content.transform.SetSiblingIndex(1);
            paperGo.transform.SetAsLastSibling();

            var pso = new SerializedObject(panel);
            pso.FindProperty("id").enumValueIndex = (int)id;
            pso.FindProperty("slideFrom").enumValueIndex = (int)slideFrom;
            pso.FindProperty("paper").objectReferenceValue = paperGo.GetComponent<AnimatedPanelUI>();
            pso.ApplyModifiedPropertiesWithoutUndo();

            return panel;
        }

        // Z/X placeholder 내용 = 타이틀 텍스트만 (배경은 종이 레이어가 제공)
        private static GameObject BuildPlaceholder(string title)
        {
            var go = new GameObject("Content", typeof(RectTransform));
            Stretch(go.GetComponent<RectTransform>());

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(go.transform, false);
            Stretch(titleGo.GetComponent<RectTransform>());
            var txt = titleGo.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 30;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.15f, 0.1f, 0.05f);
            txt.text = title + "\n(준비 중)";
            return go;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        private static void UpdateTitle(GameObject content, string text)
        {
            var t = content.transform.Find("Title");
            if (t != null) { var txt = t.GetComponent<Text>(); if (txt != null) txt.text = text; }
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
