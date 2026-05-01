using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    public sealed class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private CinemachineCamera vCam;
        [SerializeField] private int activePriority = 100;
        [SerializeField] private int inactivePriority = 0;

        public override void OnNetworkSpawn()
        {
            if (vCam == null) return;

            // 부모 Player 의 transform 변환과 ThirdPersonFollow 의 자체 위치 계산이
            // 이중 적용돼 카메라가 player 내부에 박히는 문제 방지.
            // world 좌표 유지하며 씬 루트로 분리.
            vCam.transform.SetParent(null, worldPositionStays: true);

            // CM 3.x Priority 는 PrioritySettings struct — Value 명시
            var priority = vCam.Priority;
            priority.Value = IsOwner ? activePriority : inactivePriority;
            vCam.Priority = priority;
        }

        public override void OnNetworkDespawn()
        {
            // 부모 분리한 vCam 은 Player 가 destroy 돼도 안 따라감 — 직접 정리
            if (vCam != null) Destroy(vCam.gameObject);
        }
    }
}
