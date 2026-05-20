using System.Collections.Generic;
using System.IO;
using Elyqara.Dungeon;
using Elyqara.Enemies;
using Elyqara.Game;
using Elyqara.Networking;
using Elyqara.Player;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Elyqara.EditorTools
{
    // 단계 10 통합 setup. 4 메뉴 분리 = A/B/C/D 단계별 안전 처리.
    //
    // 처리:
    // - A: 씬 4개 생성 (Lobby/Stage1/Stage2/Stage3_Boss) + GlobalManagers prefab + Player.prefab PlayerPersistence + Build Settings
    // - B: 보스 prefab + WispBoss.asset + Stage3_Boss 보스룸 spawn
    // - C: Lobby UI 폴리싱 (uGUI Start 버튼) — 1차 = OnGUI 그대로, C 단계에서 uGUI 로 승격
    // - D: HUD HP/Stamina bar + Boss HP bar
    public static class Phase10Setup
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string SourceScenePath = "Assets/Scenes/SampleScene.unity";
        private const string LobbyScenePath = "Assets/Scenes/Lobby.unity";
        private const string Stage1ScenePath = "Assets/Scenes/Stage1.unity";
        private const string Stage2ScenePath = "Assets/Scenes/Stage2.unity";
        private const string Stage3ScenePath = "Assets/Scenes/Stage3_Boss.unity";

        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Networking/Player.prefab";
        private const string GlobalManagersPrefabPath = "Assets/_Project/Prefabs/Networking/GlobalManagers.prefab";
        private const string StartRoomPrefabPath = "Assets/_Project/Prefabs/Dungeon/Rooms/StartRoom.prefab";
        private const string WispPrefabPath = "Assets/_Project/Prefabs/Enemies/Wisp.prefab";
        private const string WispBossPrefabPath = "Assets/_Project/Prefabs/Enemies/WispBoss.prefab";

        private const string LobbyRoomDataPath = "Assets/_Project/Data/Rooms/LobbyRoomData.asset";
        private const string BossRoomDataPath = "Assets/_Project/Data/Rooms/BossRoomData.asset";
        private const string KiyanCharacterPath = "Assets/_Project/Data/Characters/Kiyan.asset";
        private const string WispEnemyDataPath = "Assets/_Project/Data/Enemies/Wisp.asset";
        private const string WispBossDataPath = "Assets/_Project/Data/Enemies/WispBoss.asset";

        // ================================================================
        // Phase 10-A: Scenes & DDoL
        // ================================================================
        [MenuItem("Tools/Elyqara/Setup Phase 10-A (Scenes + DDoL)")]
        public static void RunPhase10A()
        {
            EnsureFolder("Assets/_Project/Prefabs/Networking");
            EnsureFolder("Assets/_Project/Prefabs/Enemies");
            EnsureFolder("Assets/_Project/Data/Rooms");
            EnsureFolder("Assets/_Project/Data/Enemies");
            EnsureFolder(ScenesFolder);

            EnsurePlayerPersistence();
            var globalManagersPrefab = EnsureGlobalManagersPrefab();

            CreateStage1Scene();
            CreateStage2Scene();
            CreateStage3BossSceneSkeleton();
            CreateLobbyScene(globalManagersPrefab);

            RegisterScenesInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase10Setup-A] 완료. Lobby/Stage1/Stage2/Stage3_Boss 4 씬 + GlobalManagers prefab + Player.prefab PlayerPersistence + Build Settings.");
        }

        // ================================================================
        // Phase 10-B: Boss
        // ================================================================
        [MenuItem("Tools/Elyqara/Setup Phase 10-B (Boss)")]
        public static void RunPhase10B()
        {
            EnsureWispBossData();
            EnsureWispBossPrefab();
            PopulateStage3BossScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase10Setup-B] 완료. WispBoss.asset + WispBoss.prefab + Stage3_Boss 씬에 BossSpawner 배치.");
        }

        // ================================================================
        // Phase 10-D: HUD (10-C 는 LobbyManager OnGUI 로 충분, D 에서 uGUI 폴리싱)
        // ================================================================
        [MenuItem("Tools/Elyqara/Setup Phase 10-D (HUD)")]
        public static void RunPhase10D()
        {
            // Lobby 씬 로드 후 HUD Canvas 추가
            var scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
            EnsureHUDCanvasInScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase10Setup-D] 완료. Lobby 씬에 HUDCanvas (HP/Stamina Slider) 추가. DDoL 마크되어 모든 씬 표시.");
        }

        // ================================================================
        // Helpers — Player.prefab
        // ================================================================
        private static void EnsurePlayerPersistence()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Phase10Setup] Player.prefab not found at {PlayerPrefabPath}");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                bool changed = false;
                if (contents.GetComponent<PlayerPersistence>() == null)
                {
                    contents.AddComponent<PlayerPersistence>();
                    changed = true;
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // ================================================================
        // Helpers — GlobalManagers prefab (NetworkObject + GameStateManager)
        // ================================================================
        private static GameObject EnsureGlobalManagersPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(GlobalManagersPrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("GlobalManagers");
            go.AddComponent<NetworkObject>();
            go.AddComponent<GameStateManager>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, GlobalManagersPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ================================================================
        // Helpers — Lobby 씬
        // ================================================================
        private static void CreateLobbyScene(GameObject globalManagersPrefab)
        {
            // Idempotent — 이미 있으면 OpenScene 후 누락 patch. 없으면 NewScene + 전체 생성.
            Scene scene;
            if (File.Exists(LobbyScenePath))
            {
                scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            EnsureLobbyRoomData();
            EnsureLobbyRoomInScene(scene);
            EnsureNetworkBootstrapInScene(scene, globalManagersPrefab);
            EnsureDungeonManagerInScene(scene);
            EnsureLobbyManagerInScene(scene);
            EnsureGameOverCanvasInScene(scene);
            EnsureVictoryCanvasInScene(scene);
            EnsureMainCameraInScene(scene);

            // Phase7Setup 의 InventoryCanvas 자동 배치 + DDoL 마크는 InventoryUI.Awake 가 처리.
            // Lobby 씬에 설치하면 모든 씬에서 살아남음.
            var slotPrefab = Phase7Setup.EnsureInventorySlotPrefab();
            Phase7Setup.EnsureInventoryCanvasInScene(slotPrefab);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LobbyScenePath);
        }

        private static void EnsureLobbyRoomData()
        {
            var existing = AssetDatabase.LoadAssetAtPath<RoomData>(LobbyRoomDataPath);
            if (existing != null) return;

            var data = ScriptableObject.CreateInstance<RoomData>();
            var so = new SerializedObject(data);
            so.FindProperty("roomName").stringValue = "Lobby";
            so.FindProperty("description").stringValue = "길드 느낌 로비. 4 spawn point + Start 버튼.";
            so.FindProperty("isStartRoom").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, LobbyRoomDataPath);
        }

        private static void EnsureLobbyRoomInScene(Scene scene)
        {
            // 이미 있으면 skip
            if (FindRootByName(scene, "LobbyRoom") != null) return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StartRoomPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Phase10Setup] StartRoom prefab not found at {StartRoomPrefabPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "LobbyRoom";

            var room = instance.GetComponent<Room>();
            var lobbyData = AssetDatabase.LoadAssetAtPath<RoomData>(LobbyRoomDataPath);
            if (room != null && lobbyData != null)
            {
                var so = new SerializedObject(room);
                so.FindProperty("data").objectReferenceValue = lobbyData;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureNetworkBootstrapInScene(Scene scene, GameObject globalManagersPrefab)
        {
            var existing = FindRootByName(scene, "[Network]");
            GameObject go;
            if (existing != null)
            {
                go = existing;  // patch 모드 — bootstrap 의 prefab refs 만 갱신
            }
            else
            {
                go = new GameObject("[Network]");
                SceneManager.MoveGameObjectToScene(go, scene);
            }

            // NetworkBootstrap 의 RequireComponent 가 NetworkManager + UnityTransport 자동 추가.
            // NetworkManager.NetworkConfig.NetworkTransport 는 NetworkBootstrap.Awake() 가 런타임에 자동 wire-up.
            // Enable Scene Management 는 NGO 2.x default ON — 별도 set 불필요.
            var bootstrap = go.GetComponent<NetworkBootstrap>();
            if (bootstrap == null) bootstrap = go.AddComponent<NetworkBootstrap>();

            if (bootstrap != null)
            {
                var bSo = new SerializedObject(bootstrap);

                if (globalManagersPrefab != null)
                {
                    var prefabNo = globalManagersPrefab.GetComponent<NetworkObject>();
                    var prop = bSo.FindProperty("globalManagersPrefab");
                    if (prop != null && prefabNo != null) prop.objectReferenceValue = prefabNo;
                }

                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
                if (playerPrefab != null)
                {
                    var pp = bSo.FindProperty("playerPrefab");
                    if (pp != null) pp.objectReferenceValue = playerPrefab;
                }

                bSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureDungeonManagerInScene(Scene scene)
        {
            if (FindRootByName(scene, "[DungeonManager]") != null) return;
            var go = new GameObject("[DungeonManager]");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<DungeonManager>();
        }

        private static void EnsureLobbyManagerInScene(Scene scene)
        {
            if (FindRootByName(scene, "[LobbyManager]") != null) return;
            var go = new GameObject("[LobbyManager]");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<NetworkObject>();
            go.AddComponent<LobbyManager>();
        }

        private static void EnsureGameOverCanvasInScene(Scene scene)
        {
            if (FindRootByName(scene, "[GameOverCanvas]") != null) return;
            var canvasGo = new GameObject("[GameOverCanvas]", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

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

            var ui = canvasGo.AddComponent<GameOverUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureVictoryCanvasInScene(Scene scene)
        {
            if (FindRootByName(scene, "[VictoryCanvas]") != null) return;
            var canvasGo = new GameObject("[VictoryCanvas]", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 210;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

            var textGo = new GameObject("VictoryText", typeof(RectTransform), typeof(Text));
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
            text.color = new Color(0.95f, 0.85f, 0.25f, 1f);
            text.text = "VICTORY";

            var ui = canvasGo.AddComponent<VictoryUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureMainCameraInScene(Scene scene)
        {
            if (FindRootByName(scene, "Main Camera") != null) return;
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CinemachineBrain));
            camGo.tag = "MainCamera";
            SceneManager.MoveGameObjectToScene(camGo, scene);
        }

        private static GameObject FindRootByName(Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == name) return roots[i];
            }
            return null;
        }

        // ================================================================
        // Helpers — HUD Canvas (Phase 10-D)
        // ================================================================
        private static void EnsureHUDCanvasInScene(Scene scene)
        {
            // 이미 있으면 skip
            var rootObjs = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjs.Length; i++)
            {
                if (rootObjs[i].name == "[HUDCanvas]")
                {
                    Debug.Log("[Phase10Setup-D] [HUDCanvas] 이미 존재");
                    return;
                }
            }

            var canvasGo = new GameObject("[HUDCanvas]", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;  // GameOver(200)/Victory(210) 보다 아래
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // HP Slider — 좌측 하단
            var hpSlider = CreateSimpleSlider(canvasGo.transform, "HPSlider",
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(40, 60), new Vector2(440, 100), new Color(0.85f, 0.15f, 0.15f, 1f));

            // Stamina Slider — HP 위
            var staSlider = CreateSimpleSlider(canvasGo.transform, "StaminaSlider",
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(40, 130), new Vector2(440, 70), new Color(0.25f, 0.65f, 0.95f, 1f));

            var hud = canvasGo.AddComponent<PlayerHUD>();
            var so = new SerializedObject(hud);
            so.FindProperty("hpSlider").objectReferenceValue = hpSlider;
            so.FindProperty("staminaSlider").objectReferenceValue = staSlider;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Slider CreateSimpleSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta, Color fillColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            // Background
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

            // Fill Area / Fill
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero; fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.offsetMin = new Vector2(5, 5); fillAreaRt.offsetMax = new Vector2(-5, -5);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = fillColor;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = bg.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.transition = Selectable.Transition.None;

            return slider;
        }

        // ================================================================
        // Helpers — Stage scenes
        // ================================================================
        private static void CreateStage1Scene()
        {
            if (!File.Exists(SourceScenePath))
            {
                Debug.LogError($"[Phase10Setup] SampleScene not found at {SourceScenePath} — Stage1 복제 불가");
                return;
            }

            // Idempotent — 이미 있으면 OpenScene 후 patch (글로벌 strip + StageTrigger 갱신).
            if (!File.Exists(Stage1ScenePath))
            {
                AssetDatabase.CopyAsset(SourceScenePath, Stage1ScenePath);
                AssetDatabase.Refresh();
            }

            var scene = EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
            StripGlobalsFromStage(scene);
            AddStageTriggerToStage(scene, "Stage2");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateStage2Scene()
        {
            if (!File.Exists(Stage1ScenePath))
            {
                Debug.LogError("[Phase10Setup] Stage1.unity 없음 — Stage2 복제 불가");
                return;
            }

            if (!File.Exists(Stage2ScenePath))
            {
                AssetDatabase.CopyAsset(Stage1ScenePath, Stage2ScenePath);
                AssetDatabase.Refresh();
            }

            var scene = EditorSceneManager.OpenScene(Stage2ScenePath, OpenSceneMode.Single);
            StripGlobalsFromStage(scene);
            UpdateStageTriggerNextScene(scene, "Stage3_Boss");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateStage3BossSceneSkeleton()
        {
            Scene scene;
            if (File.Exists(Stage3ScenePath))
            {
                scene = EditorSceneManager.OpenScene(Stage3ScenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            EnsureBossRoomData();
            EnsureBossRoomInScene(scene);
            EnsureDungeonManagerInScene(scene);
            EnsureMainCameraInScene(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, Stage3ScenePath);
        }

        private static void EnsureBossRoomData()
        {
            var existing = AssetDatabase.LoadAssetAtPath<RoomData>(BossRoomDataPath);
            if (existing != null) return;

            var data = ScriptableObject.CreateInstance<RoomData>();
            var so = new SerializedObject(data);
            so.FindProperty("roomName").stringValue = "BossRoom";
            so.FindProperty("description").stringValue = "보스룸. Player Spawn 후 보스 1마리 등장.";
            so.FindProperty("isStartRoom").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, BossRoomDataPath);
        }

        private static void EnsureBossRoomInScene(Scene scene)
        {
            if (FindRootByName(scene, "BossRoom") != null) return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StartRoomPrefabPath);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "BossRoom";

            var room = instance.GetComponent<Room>();
            var bossData = AssetDatabase.LoadAssetAtPath<RoomData>(BossRoomDataPath);
            if (room != null && bossData != null)
            {
                var so = new SerializedObject(room);
                so.FindProperty("data").objectReferenceValue = bossData;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // SampleScene 복제 후 Stage1 의 [Network] / [GameOverCanvas] / [GameStateManager] 제거.
        // 이런 글로벌 객체는 Lobby 에만 두고 DDoL 로 모든 씬 살아남음.
        private static void StripGlobalsFromStage(Scene scene)
        {
            string[] removeNames = { "[Network]", "[GameOverCanvas]", "[GameStateManager]", "[VictoryCanvas]", "[HUDCanvas]", "[LobbyManager]", "[InventoryCanvas]", "EventSystem" };
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var go = roots[i];
                for (int n = 0; n < removeNames.Length; n++)
                {
                    if (go.name == removeNames[n])
                    {
                        Object.DestroyImmediate(go);
                        break;
                    }
                }
            }
        }

        private static void AddStageTriggerToStage(Scene scene, string nextSceneName)
        {
            // 이미 있으면 skip
            var existing = Object.FindFirstObjectByType<StageTrigger>();
            if (existing != null)
            {
                UpdateStageTriggerNextScene(scene, nextSceneName);
                return;
            }

            // Room_E3 의 동쪽 끝에 배치
            var rooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None);
            Room targetRoom = null;
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i].gameObject.name == "Room_E3") { targetRoom = rooms[i]; break; }
            }
            // Room_E3 없으면 가장 동쪽 (x 최대) Room
            if (targetRoom == null)
            {
                float maxX = float.MinValue;
                for (int i = 0; i < rooms.Length; i++)
                {
                    float x = rooms[i].transform.position.x;
                    if (x > maxX) { maxX = x; targetRoom = rooms[i]; }
                }
            }

            Vector3 pos;
            if (targetRoom != null)
            {
                var eastDoor = targetRoom.GetDoorSocket(DoorDirection.East);
                pos = eastDoor != null ? eastDoor.position : targetRoom.transform.position + new Vector3(8f, 1f, 0f);
            }
            else
            {
                pos = new Vector3(20f, 1f, 0f);
            }

            var trigger = new GameObject("[StageTrigger]");
            SceneManager.MoveGameObjectToScene(trigger, scene);
            trigger.transform.position = pos;

            var box = trigger.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 3f, 2f);
            box.center = new Vector3(0f, 1.5f, 0f);

            trigger.AddComponent<NetworkObject>();
            var st = trigger.AddComponent<StageTrigger>();

            var so = new SerializedObject(st);
            so.FindProperty("nextSceneName").stringValue = nextSceneName;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void UpdateStageTriggerNextScene(Scene scene, string nextSceneName)
        {
            var triggers = Object.FindObjectsByType<StageTrigger>(FindObjectsSortMode.None);
            for (int i = 0; i < triggers.Length; i++)
            {
                var so = new SerializedObject(triggers[i]);
                so.FindProperty("nextSceneName").stringValue = nextSceneName;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(triggers[i]);
            }
        }

        // ================================================================
        // Phase 10-B: Boss
        // ================================================================
        private static void EnsureWispBossData()
        {
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(WispBossDataPath) != null) return;

            var wispData = AssetDatabase.LoadAssetAtPath<EnemyData>(WispEnemyDataPath);
            if (wispData == null)
            {
                Debug.LogError($"[Phase10Setup-B] Wisp.asset 없음 — {WispEnemyDataPath}");
                return;
            }

            // Wisp 복제 후 HP / damage 부풀림
            AssetDatabase.CopyAsset(WispEnemyDataPath, WispBossDataPath);
            AssetDatabase.Refresh();
            var bossData = AssetDatabase.LoadAssetAtPath<EnemyData>(WispBossDataPath);
            if (bossData == null) return;

            var so = new SerializedObject(bossData);
            so.FindProperty("enemyName").stringValue = "Wisp Boss";
            so.FindProperty("maxHealth").floatValue = 800f;       // Wisp 의 6~7배
            so.FindProperty("attackDamage").floatValue = 45f;
            so.FindProperty("moveSpeed").floatValue = 3.5f;
            so.FindProperty("aggroRadius").floatValue = 25f;
            so.FindProperty("deaggroRadius").floatValue = 60f;
            so.FindProperty("attackRange").floatValue = 2.5f;
            so.FindProperty("attackWindup").floatValue = 0.7f;
            so.FindProperty("attackRecovery").floatValue = 0.9f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bossData);
        }

        private static void EnsureWispBossPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(WispBossPrefabPath) != null) return;

            var wispPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WispPrefabPath);
            if (wispPrefab == null)
            {
                Debug.LogError($"[Phase10Setup-B] Wisp.prefab 없음 — {WispPrefabPath}");
                return;
            }

            AssetDatabase.CopyAsset(WispPrefabPath, WispBossPrefabPath);
            AssetDatabase.Refresh();

            var contents = PrefabUtility.LoadPrefabContents(WispBossPrefabPath);
            try
            {
                // EnemyData 변경 = WispBoss
                var ec = contents.GetComponent<EnemyController>();
                var bossData = AssetDatabase.LoadAssetAtPath<EnemyData>(WispBossDataPath);
                if (ec != null && bossData != null)
                {
                    var ecSo = new SerializedObject(ec);
                    ecSo.FindProperty("data").objectReferenceValue = bossData;
                    ecSo.ApplyModifiedPropertiesWithoutUndo();
                }

                // 외형 부풀림 = scale 1.8
                contents.transform.localScale = Vector3.one * 1.8f;

                // BossMarker 추가
                if (contents.GetComponent<BossMarker>() == null)
                {
                    contents.AddComponent<BossMarker>();
                }

                PrefabUtility.SaveAsPrefabAsset(contents, WispBossPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void PopulateStage3BossScene()
        {
            if (!File.Exists(Stage3ScenePath))
            {
                Debug.LogError("[Phase10Setup-B] Stage3_Boss.unity 없음 — Phase10-A 먼저 실행");
                return;
            }

            var scene = EditorSceneManager.OpenScene(Stage3ScenePath, OpenSceneMode.Single);

            // BossSpawner 이미 있으면 skip
            var existing = Object.FindFirstObjectByType<EnemySpawner>();
            if (existing != null)
            {
                Debug.Log("[Phase10Setup-B] BossSpawner 이미 존재 — Boss prefab ref 갱신만");
                AssignBossPrefabToSpawner(existing);
            }
            else
            {
                var go = new GameObject("[BossSpawner]");
                SceneManager.MoveGameObjectToScene(go, scene);

                // BossRoom 의 중앙에서 약간 안쪽
                var room = Object.FindFirstObjectByType<Room>();
                if (room != null)
                {
                    go.transform.position = room.transform.position + new Vector3(0f, 1f, 6f);
                }

                var spawner = go.AddComponent<EnemySpawner>();
                AssignBossPrefabToSpawner(spawner);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AssignBossPrefabToSpawner(EnemySpawner spawner)
        {
            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WispBossPrefabPath);
            if (bossPrefab == null) return;
            var bossNo = bossPrefab.GetComponent<NetworkObject>();
            if (bossNo == null) return;

            var so = new SerializedObject(spawner);
            so.FindProperty("enemyPrefab").objectReferenceValue = bossNo;
            so.FindProperty("spawnOnHostStart").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawner);
        }

        // ================================================================
        // Build Settings
        // ================================================================
        private static void RegisterScenesInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            string[] paths = { LobbyScenePath, Stage1ScenePath, Stage2ScenePath, Stage3ScenePath };

            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(paths[i]))
                {
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));
                }
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ================================================================
        // Utilities
        // ================================================================
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
