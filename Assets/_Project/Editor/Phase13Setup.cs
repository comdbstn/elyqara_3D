using System.Reflection;
using Elyqara.Characters;
using Elyqara.Items;
using Elyqara.Skills;
using UnityEditor;
using UnityEngine;

namespace Elyqara.EditorTools
{
    // 단계 13-2 통합. [Tools/Elyqara/Setup Phase 13 (Kiyan Skills)] 한 메뉴.
    //
    // 처리:
    // - 3 SO 생성 (ShieldBash / KiyanResolve / KiyanRoll). BasicMelee 는 이미 존재.
    // - Kiyan.asset 4 슬롯 wire-up (primary/secondary/q/dodge)
    // - Player.prefab 에 KiyanShieldPassive 컴포넌트 부착
    //
    // SO ref 는 Reflection 우회 (메모리 feedback_serializedobject_so_ref_fail.md 정합).
    // 단순 float/enum 값은 SerializedObject 안전.
    public static class Phase13Setup
    {
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Networking/Player.prefab";
        private const string KiyanPath = "Assets/_Project/Data/Characters/Kiyan.asset";
        private const string SkillsFolder = "Assets/_Project/Data/Skills";
        private const string BasicMeleePath = SkillsFolder + "/BasicMelee.asset";
        private const string ShieldBashPath = SkillsFolder + "/ShieldBash.asset";
        private const string ResolvePath = SkillsFolder + "/KiyanResolve.asset";
        private const string RollPath = SkillsFolder + "/KiyanRoll.asset";

        [MenuItem("Tools/Elyqara/Setup Phase 13 (Kiyan Skills)")]
        public static void Run()
        {
            EnsureFolder(SkillsFolder);
            EnsureShieldBash();
            EnsureResolve();
            EnsureRoll();
            WireKiyanCharacter();
            EnsurePlayerPassive();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase13Setup] 완료. ShieldBash / KiyanResolve / KiyanRoll 생성 + Kiyan.asset 4 슬롯 wire-up + Player.prefab KiyanShieldPassive 부착.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            var name = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        // 방패 강타 — Blunt 분리. 적 1m 뒤로 밀치고 stagger 효과 (knockback duration 동안 AI velocity 보존).
        private static void EnsureShieldBash()
        {
            var existing = AssetDatabase.LoadAssetAtPath<BasicMeleeSkill>(ShieldBashPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<BasicMeleeSkill>();
                AssetDatabase.CreateAsset(existing, ShieldBashPath);
            }
            var so = new SerializedObject(existing);
            so.FindProperty("cooldownSeconds").floatValue = 1.5f;
            so.FindProperty("staminaCost").floatValue = 25f;
            so.FindProperty("damage").floatValue = 35f;
            so.FindProperty("reach").floatValue = 1.8f;
            so.FindProperty("halfAngleDeg").floatValue = 45f;
            so.FindProperty("damageType").enumValueIndex = (int)ItemEffectType.BluntDamageBonus;
            so.FindProperty("knockbackForce").floatValue = 8f;
            so.FindProperty("knockbackDuration").floatValue = 0.3f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(existing);
        }

        // 결전 — 8s 동안 공/방 +50%. 1차 buff 풀 단순.
        private static void EnsureResolve()
        {
            var existing = AssetDatabase.LoadAssetAtPath<BuffSkill>(ResolvePath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<BuffSkill>();
                AssetDatabase.CreateAsset(existing, ResolvePath);
            }
            var so = new SerializedObject(existing);
            so.FindProperty("cooldownSeconds").floatValue = 30f;
            so.FindProperty("staminaCost").floatValue = 30f;
            so.FindProperty("duration").floatValue = 8f;
            so.FindProperty("attackBonus").floatValue = 0.5f;
            so.FindProperty("defenseBonus").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(existing);
        }

        // 구르기 — 정면 dash + 0.3s i-frame. 다크소울 정공 값.
        private static void EnsureRoll()
        {
            var existing = AssetDatabase.LoadAssetAtPath<RollDodgeSkill>(RollPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<RollDodgeSkill>();
                AssetDatabase.CreateAsset(existing, RollPath);
            }
            var so = new SerializedObject(existing);
            so.FindProperty("cooldownSeconds").floatValue = 0.8f;
            so.FindProperty("staminaCost").floatValue = 25f;
            so.FindProperty("dashImpulse").floatValue = 12f;
            so.FindProperty("invincibleSeconds").floatValue = 0.3f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(existing);
        }

        private static void WireKiyanCharacter()
        {
            var kiyan = AssetDatabase.LoadAssetAtPath<CharacterData>(KiyanPath);
            if (kiyan == null) { Debug.LogError($"[Phase13Setup] {KiyanPath} 누락"); return; }

            var primary = AssetDatabase.LoadAssetAtPath<SkillData>(BasicMeleePath);
            var secondary = AssetDatabase.LoadAssetAtPath<SkillData>(ShieldBashPath);
            var q = AssetDatabase.LoadAssetAtPath<SkillData>(ResolvePath);
            var dodge = AssetDatabase.LoadAssetAtPath<SkillData>(RollPath);

            if (primary == null) Debug.LogError($"[Phase13Setup] {BasicMeleePath} 누락");
            if (secondary == null) Debug.LogError($"[Phase13Setup] {ShieldBashPath} 누락");
            if (q == null) Debug.LogError($"[Phase13Setup] {ResolvePath} 누락");
            if (dodge == null) Debug.LogError($"[Phase13Setup] {RollPath} 누락");

            SetField(kiyan, "primarySkill", primary);
            SetField(kiyan, "secondarySkill", secondary);
            SetField(kiyan, "qSkill", q);
            SetField(kiyan, "dodgeSkill", dodge);
            EditorUtility.SetDirty(kiyan);
        }

        private static void EnsurePlayerPassive()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null) { Debug.LogError($"[Phase13Setup] {PlayerPrefabPath} 누락"); return; }

            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                bool changed = false;
                if (contents.GetComponent<KiyanShieldPassive>() == null)
                {
                    contents.AddComponent<KiyanShieldPassive>();
                    changed = true;
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // SO ref 는 Reflection. NetworkBehaviour + SO ref 조합 race 회피 (메모리 정합). 일관성 명목 모든 ref 동일 패턴.
        private static void SetField(object obj, string name, object value)
        {
            if (obj == null) return;
            var t = obj.GetType();
            while (t != null)
            {
                var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null) { f.SetValue(obj, value); return; }
                t = t.BaseType;
            }
            Debug.LogWarning($"[Phase13Setup] field '{name}' not found on {obj.GetType().Name}");
        }
    }
}
