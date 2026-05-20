using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    // 단계 10-A — Player NetworkObject 가 씬 전환 시 살아남도록 DontDestroyOnLoad 처리.
    //
    // NGO 2.x SceneManager.LoadScene(Single) 시 dynamic spawn NetworkObject 도 unload.
    // DDoL 마크하면 호스트/클라 모두 GO 살아남음. Inventory NetworkList 등 NetworkVariable 도 유지.
    //
    // NGO 공식 docs 검증:
    //   "If you're using scene switching, you can migrate the NetworkObject into the DDoL
    //    by sending its GameObject to the DDoL using DontDestroyOnLoad(gameObject)."
    //   주의: 한 번 DDoL 로 가면 다른 씬으로 다시 옮기면 안 됨 (late-join sync 깨짐).
    //   우리 케이스 = Player 는 첫 spawn 후 모든 씬에서 DDoL 유지. 안전.
    //
    // 위치 reset 은 PlayerSpawnPositioner 가 OnLoadEventCompleted 콜백에서 처리.
    public sealed class PlayerPersistence : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
