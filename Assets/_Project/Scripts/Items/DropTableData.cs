using UnityEngine;

namespace Elyqara.Items
{
    // 적별 드랍 풀. 가중치 기반 랜덤 선택. 1차 = Wisp 의 5종 균일 확률.
    // 새 드랍 풀 = SO 1장 + EnemyData.dropTable ref 한 줄.
    [CreateAssetMenu(fileName = "DropTable", menuName = "Elyqara/Drop Table")]
    public sealed class DropTableData : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public ItemData item;

            [Tooltip("선택 가중치. 1=균등, 2=두 배 확률.")]
            public float weight;
        }

        public Entry[] entries;

        // 가중치 기반 랜덤 선택. 호스트만 호출.
        public ItemData Roll()
        {
            if (entries == null || entries.Length == 0) return null;

            float total = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].item != null && entries[i].weight > 0f) total += entries[i].weight;
            }
            if (total <= 0f) return null;

            float r = Random.value * total;
            float acc = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].item == null || entries[i].weight <= 0f) continue;
                acc += entries[i].weight;
                if (r <= acc) return entries[i].item;
            }
            return entries[entries.Length - 1].item;
        }
    }
}
