using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Items
{
    // 호스트 전용 유틸리티. 적 죽음 등 콜백에서 호출. 단계 4 EnemySpawner 패턴 그대로.
    // DroppedItem prefab = 단일 prefab. ItemData 별 prefab variant 만들지 X — 인덱스로 동기화.
    public static class ItemSpawner
    {
        private static GameObject _droppedItemPrefab;

        // 1차 = Resources.Load. 단계 12+ Addressables 또는 직접 ref.
        // prefab path = Assets/_Project/Resources/DroppedItem.prefab
        public static GameObject GetPrefab()
        {
            if (_droppedItemPrefab == null) _droppedItemPrefab = Resources.Load<GameObject>("DroppedItem");
            return _droppedItemPrefab;
        }

        // 호스트만 호출. DropTable 에서 한 개 ItemData 선택 → DroppedItem NetworkObject Spawn.
        public static void SpawnFromTable(DropTableData table, Vector3 worldPosition)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (table == null) return;

            ItemData item = table.Roll();
            if (item == null) return;

            SpawnItem(item, worldPosition);
        }

        public static void SpawnItem(ItemData item, Vector3 worldPosition)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (item == null) return;

            var prefab = GetPrefab();
            if (prefab == null)
            {
                Debug.LogError("[ItemSpawner] DroppedItem prefab 누락 — Resources/DroppedItem.prefab 필요");
                return;
            }

            var db = ItemDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[ItemSpawner] ItemDatabase 누락 — Resources/ItemDatabase.asset 필요");
                return;
            }

            int index = db.IndexOf(item);
            if (index < 0)
            {
                Debug.LogError($"[ItemSpawner] ItemData '{item.name}' 가 ItemDatabase 에 등록되지 않음");
                return;
            }

            var go = Object.Instantiate(prefab, worldPosition, Quaternion.identity);
            var dropped = go.GetComponent<DroppedItem>();
            if (dropped == null)
            {
                Debug.LogError("[ItemSpawner] prefab 에 DroppedItem 컴포넌트 누락");
                Object.Destroy(go);
                return;
            }

            var no = go.GetComponent<NetworkObject>();
            no.Spawn(true);
            dropped.InitializeOnServer(index);
        }
    }
}
