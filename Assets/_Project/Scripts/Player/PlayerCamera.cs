using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private CinemachineCamera vCam;
        [SerializeField] private int activePriority = 100;
        [SerializeField] private int inactivePriority = 0;

        [Header("Third Person — Souls 톤")]
        [Tooltip("Player 부터 카메라까지 거리 (m)")]
        [SerializeField] private float distance = 5.5f;
        [Tooltip("Player 머리/어깨 높이 (m)")]
        [SerializeField] private float verticalOffset = 1.6f;
        [Tooltip("어깨 너머 좌우 offset — 0 = Player 정중앙")]
        [SerializeField] private float horizontalOffset = 0f;

        [Header("Pitch (마우스 Y)")]
        [SerializeField] private float pitchSensitivity = 0.1f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 50f;
        [SerializeField] private float startPitch = 10f;
        [Tooltip("마우스 Y 반전 (true = 마우스 위 → 카메라 위)")]
        [SerializeField] private bool invertY = false;

        private PlayerInput _input;
        private float _pitch;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _pitch = startPitch;
        }

        public override void OnNetworkSpawn()
        {
            if (vCam == null) return;

            // 부모 Player 의 transform 변환 영향 분리.
            vCam.transform.SetParent(null, worldPositionStays: true);

            var priority = vCam.Priority;
            priority.Value = IsOwner ? activePriority : inactivePriority;
            vCam.Priority = priority;

            // CM 3.x ThirdPersonFollow 가 우리 환경 (NetworkBehaviour + SetParent(null))
            // 에서 Tracking Target 추적 안 됨 — disable 박고 PlayerCamera 가 LateUpdate 에서 직접 추적.
            // prefab 의 vCam 자식에 컴포넌트 박혀있으면 안전망으로 비활성. 컴포넌트 제거는 prefab 정리 시 처리.
            var follow = vCam.GetComponent<CinemachineThirdPersonFollow>();
            if (follow != null) follow.enabled = false;
        }

        public override void OnNetworkDespawn()
        {
            if (vCam != null) Destroy(vCam.gameObject);
        }

        private void LateUpdate()
        {
            if (vCam == null || !IsOwner) return;

            // Pitch — 카메라 자체만 회전 (Player 회전과 무관, ServerRpc 불필요)
            float dy = _input.Look.y * pitchSensitivity;
            _pitch += invertY ? dy : -dy;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            // Yaw — Player 의 yaw 받아씀 (PlayerMovement 가 호스트 권위로 박음)
            float yaw = transform.eulerAngles.y;

            Quaternion rot = Quaternion.Euler(_pitch, yaw, 0f);
            Vector3 pivot = transform.position + Vector3.up * verticalOffset;
            Vector3 worldOffset = rot * new Vector3(horizontalOffset, 0f, -distance);

            vCam.transform.position = pivot + worldOffset;
            vCam.transform.rotation = rot;
        }
    }
}
