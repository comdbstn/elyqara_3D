using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Dungeon
{
    // Player.prefab 에 추가하는 컴포넌트. 서버 권위로 OnNetworkSpawn 시 transform.position 설정.
    // NGO 2.x ConnectionApproval 의 Position 파라미터 (0,0,0) 회피 패턴 — server-auth NetworkTransform 이 다음 tick 에 replicate.
    public sealed class PlayerSpawnPositioner : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            if (DungeonManager.Instance == null) return;

            var pos = DungeonManager.Instance.GetPlayerSpawnPosition((int)OwnerClientId);
            transform.position = pos;
        }
    }
}
