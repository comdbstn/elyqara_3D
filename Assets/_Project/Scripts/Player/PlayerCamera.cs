using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    // 소울식 3인칭 카메라. 마우스로 캐릭터 주위 공전 — 캐릭터 회전과 완전 분리.
    // 락온 중에는 yaw 가 타겟 방향으로 자동 정렬. PlayerMovement 가 Yaw 를 읽어 카메라-상대 이동에 사용.
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private CinemachineCamera vCam;
        [SerializeField] private int activePriority = 100;
        [SerializeField] private int inactivePriority = 0;

        [Header("Third Person — 전신 프레이밍")]
        [Tooltip("Player 부터 카메라까지 거리 (m)")]
        [SerializeField] private float distance = 6f;
        [Tooltip("Player root 기준 pivot 높이 (m). 0 = root(캐릭터 중앙)")]
        [SerializeField] private float verticalOffset = 0.5f;
        [Tooltip("어깨 너머 좌우 offset — 0 = 정중앙")]
        [SerializeField] private float horizontalOffset = 0f;

        [Header("Look (마우스)")]
        [SerializeField] private float yawSensitivity = 0.15f;
        [SerializeField] private float pitchSensitivity = 0.1f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 50f;
        [SerializeField] private float startPitch = 15f;
        [Tooltip("마우스 Y 반전 (true = 마우스 위 → 카메라 위)")]
        [SerializeField] private bool invertY = false;

        [Header("Lock-On")]
        [Tooltip("락온 시 카메라 yaw 가 타겟 방향으로 도는 속도 (deg/s)")]
        [SerializeField] private float lockYawSpeed = 360f;

        [Header("Camera Collision")]
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float collisionOffset = 0.1f;

        private PlayerInput _input;
        private PlayerLockOn _lockOn;
        private float _yaw;
        private float _pitch;

        // PlayerMovement 가 카메라-상대 이동 계산에 사용
        public float Yaw => _yaw;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _lockOn = GetComponent<PlayerLockOn>();
            _pitch = startPitch;
        }

        public override void OnNetworkSpawn()
        {
            if (vCam == null) return;

            _yaw = transform.eulerAngles.y;   // 시작 시 캐릭터 정면 뒤에서 시작

            // 부모 Player 의 transform 변환 영향 분리.
            vCam.transform.SetParent(null, worldPositionStays: true);

            // Player 가 DDoL — vCam 도 같이 살아남아야 씬 전환 후에도 추적.
            DontDestroyOnLoad(vCam.gameObject);

            var priority = vCam.Priority;
            priority.Value = IsOwner ? activePriority : inactivePriority;
            vCam.Priority = priority;

            // CM 3.x ThirdPersonFollow 는 이 환경(NetworkBehaviour + SetParent(null))에서
            // 추적 안 됨 — disable 후 PlayerCamera 가 LateUpdate 에서 직접 추적.
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

            // Pitch — 항상 마우스
            float dy = _input.Look.y * pitchSensitivity;
            _pitch = Mathf.Clamp(_pitch + (invertY ? dy : -dy), minPitch, maxPitch);

            // Yaw — 락온 중엔 타겟 방향 자동 정렬 / 평소엔 마우스 공전
            Transform lockTarget = _lockOn != null ? _lockOn.CurrentTarget : null;
            if (lockTarget != null)
            {
                Vector3 toTarget = lockTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    float targetYaw = Quaternion.LookRotation(toTarget).eulerAngles.y;
                    _yaw = Mathf.MoveTowardsAngle(_yaw, targetYaw, lockYawSpeed * Time.deltaTime);
                }
            }
            else
            {
                _yaw += _input.Look.x * yawSensitivity;
            }

            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pivot = transform.position + Vector3.up * verticalOffset;
            Vector3 worldOffset = rot * new Vector3(horizontalOffset, 0f, -distance);
            Vector3 idealPos = pivot + worldOffset;

            // 벽 충돌 회피 SphereCast — pivot → idealPos 사이 벽 hit 시 카메라를 당김.
            Vector3 dir = idealPos - pivot;
            float dist = dir.magnitude;
            if (dist > 0.01f)
            {
                Vector3 dirNorm = dir / dist;
                if (Physics.SphereCast(pivot, collisionRadius, dirNorm, out RaycastHit hit, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    vCam.transform.position = pivot + dirNorm * Mathf.Max(0.1f, hit.distance - collisionOffset);
                else
                    vCam.transform.position = idealPos;
            }
            else
            {
                vCam.transform.position = idealPos;
            }
            vCam.transform.rotation = rot;
        }
    }
}
