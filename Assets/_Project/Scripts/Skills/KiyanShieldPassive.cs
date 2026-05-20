using UnityEngine;

namespace Elyqara.Skills
{
    // 단계 13-2 — Kiyan 패시브 「작은 방패」. 적 공격 받을 때 확률로 피해 감소.
    // Player.prefab 에 부착 (1차 단순). 미래 캐릭 추가 시 = CharacterData.passiveSkill 슬롯 + 동적 부착 패턴.
    // PlayerResources.ApplyDamageServer 가 GetComponent 로 query.
    public sealed class KiyanShieldPassive : MonoBehaviour
    {
        [Header("Block Chance")]
        [Tooltip("방어 확률. 0.2 = 20%")]
        [SerializeField] private float blockChance = 0.2f;

        [Tooltip("발동 시 피해 감소율. 0.5 = 50% 감소")]
        [SerializeField] private float damageReduction = 0.5f;

        // PlayerResources 가 호출. 호스트 권위 (Server 만).
        public float Modify(float incoming)
        {
            if (UnityEngine.Random.value < blockChance)
                return incoming * (1f - damageReduction);
            return incoming;
        }
    }
}
