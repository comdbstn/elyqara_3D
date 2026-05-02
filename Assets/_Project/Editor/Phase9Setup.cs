using Elyqara.Game;
using Elyqara.Player;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Elyqara.EditorTools
{
    // 단계 9 통합. [Tools/Elyqara/Setup Phase 9] 한 메뉴.
    //
    // 처리:
    // - Player.prefab 에 PlayerRevive 컴포넌트 추가
    // - 씬 [GameStateManager] GameObject (NetworkObject + GameStateManager) — 씬 placed NetworkObject
    // - 씬 [GameOverCanvas] (Canvas + Panel + "GAME OVER" Text + GameOverUI)
    public static class Phase9Setup
    {
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Networking/Player.prefab";

        [MenuItem("Tools/Elyqara/Setup Phase 9")]
        public static void Run()
        {
            EnsurePlayerComponents();
            EnsureGameStateManagerInScene();
            EnsureGameOverCanvasInScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase9Setup] 완료. Player.prefab PlayerRevive / 씬 [GameStateManager] / [GameOverCanvas] 처리됨.");
        }

        private static void EnsurePlayerComponents()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Phase9Setup] Player.prefab not found at {PlayerPrefabPath}");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                bool changed = false;
                if (contents.GetComponent<PlayerRevive>() == null)
                {
                    contents.AddComponent<PlayerRevive>();
                    changed = true;
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void EnsureGameStateManagerInScene()
        {
            var existing = Object.FindFirstObjectByType<GameStateManager>();
            if (existing != null)
            {
                Debug.Log("[Phase9Setup] GameStateManager 이미 씬에 존재");
                return;
            }

            var go = new GameObject("[GameStateManager]");
            go.AddComponent<NetworkObject>();
            go.AddComponent<GameStateManager>();
            EditorSceneManager.MarkSceneDirty(go.scene);
            EditorSceneManager.SaveScene(go.scene);
        }

        private static void EnsureGameOverCanvasInScene()
        {
            var existing = Object.FindFirstObjectByType<GameOverUI>();
            if (existing != null)
            {
                Debug.Log("[Phase9Setup] GameOverCanvas 이미 씬에 존재");
                return;
            }

            // Canvas root
            var canvasGo = new GameObject("[GameOverCanvas]", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;  // 인벤토리 (100) 위
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Panel (전체 검정 반투명)
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panelImg = panel.GetComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.85f);

            // Text "GAME OVER"
            var textGo = new GameObject("GameOverText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(panel.transform, false);
            var textRT = textGo.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(900, 200);
            textRT.anchoredPosition = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 96;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.85f, 0.15f, 0.15f, 1f);
            text.text = "GAME OVER";

            // GameOverUI 컴포넌트
            var ui = canvasGo.AddComponent<GameOverUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            EditorSceneManager.SaveScene(canvasGo.scene);
        }
    }
}
