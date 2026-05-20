using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elyqara.Dungeon
{
    // Player.prefab 에 추가하는 컴포넌트. 서버 권위로 OnNetworkSpawn 시 transform.position 설정.
    // NGO 2.x ConnectionApproval 의 Position 파라미터 (0,0,0) 회피 패턴 — server-auth NetworkTransform 이 다음 tick 에 replicate.
    //
    // 단계 10-A 확장: Player 가 DontDestroyOnLoad 라 씬 전환 시 살아남음. 새 씬의 spawn point 로 위치 reset 필요.
    // → NGO SceneManager.OnLoadEventCompleted 구독 (호스트만). 새 씬 로드 완료 시 모든 Player 가 본인 위치 reset.
    public sealed class PlayerSpawnPositioner : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            ApplySpawnPosition();

            if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!IsServer) return;
            // 단계 13 — RuntimeDungeonGenerator 가 OnNetworkSpawn 시점 Generate.
            // DungeonManager.GetStartRoom() 등록 완료까지 대기 (race 회피). timeout 5초.
            StartCoroutine(WaitAndApply());
        }

        private IEnumerator WaitAndApply()
        {
            const float timeout = 5f;
            float elapsed = 0f;
            while ((DungeonManager.Instance == null || DungeonManager.Instance.GetStartRoom() == null) && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            ApplySpawnPosition();
        }

        private void ApplySpawnPosition()
        {
            if (!IsServer) return;
            if (DungeonManager.Instance == null) return;

            var pos = DungeonManager.Instance.GetPlayerSpawnPosition((int)OwnerClientId);
            transform.position = pos;
        }
    }
}
