using UnityEngine;

namespace Elyqara.Items
{
    // 모든 ItemData 의 인덱스 lookup. NGO 동기화에서 SO reference 대신 int 인덱스 사용.
    // 호스트가 ItemData → 인덱스 결정 → 클라가 같은 인덱스로 ItemDatabase.Get 으로 같은 SO lookup.
    //
    // 1차 = Singleton (Resources.Load). asset 위치 = Assets/_Project/Resources/ItemDatabase.asset
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Elyqara/Item Database")]
    public sealed class ItemDatabase : ScriptableObject
    {
        public ItemData[] items;

        public ItemData Get(int index)
        {
            if (items == null || index < 0 || index >= items.Length) return null;
            return items[index];
        }

        public int IndexOf(ItemData data)
        {
            if (items == null || data == null) return -1;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == data) return i;
            }
            return -1;
        }

        public int Count => items != null ? items.Length : 0;

        private static ItemDatabase _instance;

        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                return _instance;
            }
        }
    }
}
