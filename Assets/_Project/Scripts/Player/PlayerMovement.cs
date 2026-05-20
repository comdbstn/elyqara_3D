using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Elyqara.Skills;

namespace Elyqara.Player
{
    // 소울식 조작 — 마우스는 카메라 전용(PlayerCamera 소유). 캐릭터 회전은 게임플레이가 결정:
    //  - 평소: 이동 방향으로 회전
    //  - 락온: 타겟을 바라봄
    // 이동은 항상 카메라 yaw 기준(카메라-상대). 호스트 권위 — owner 는 입력만 전송.
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerResources))]
    public sealed class PlayerMovement : NetworkBehaviour, IDashable
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runMultiplier = 1.8f;
        [Tooltip("캐릭터가 이동방향/타겟으로 회전하는 속도 (deg/s)")]
        [SerializeField] private float turnSpeed = 720f;

        private Rigidbody _rigidbody;
        private PlayerInput _input;
        private PlayerResources _resources;
        private PlayerAnimator _playerAnimator;
        private PlayerCamera _camera;
        private PlayerLockOn _lockOn;

        private Vector3 _moveInput;   // 서버 — owner 가 보낸 월드공간 이동 방향
        private bool _isRunning;

        // 구르기 dash — 이 시간까지 입력 속도 대신 _dashVelocity 적용
        private float _dashUntil;
        private Vector3 _dashVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInput>();
            _resources = GetComponent<PlayerResources>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            _camera = GetComponent<PlayerCamera>();
            _lockOn = GetComponent<PlayerLockOn>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _input.EnableAll();
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) _input.DisableAll();
        }

        private void FixedUpdate()
        {
            // Owner — 입력만 서버로 전송 (물리 적용 X)
            if (IsOwner)
            {
                bool down = _resources != null && _resources.IsDown.Value;
                if (down)
                {
                    SubmitMoveServerRpc(Vector3.zero, false);
                }
                else
                {
                    Vector2 input = _input.Move;
                    // WASD = 카메라 yaw 기준 (카메라-상대 이동). 캐릭터 facing 과 무관.
                    float camYaw = _camera != null ? _camera.Yaw : 0f;
                    Quaternion camRot = Quaternion.Euler(0f, camYaw, 0f);
                    Vector3 forward = camRot * Vector3.forward;
                    Vector3 right = camRot * Vector3.right;
                    Vector3 horizontal = right * input.x + forward * input.y;
                    if (horizontal.sqrMagnitude > 1f) horizontal.Normalize();
                    bool running = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
                    SubmitMoveServerRpc(horizontal, running);
                }
            }

            // Server — 물리 + 회전 (호스트 권위)
            if (IsServer)
            {
                Vector3 velocity;
                if (Time.time < _dashUntil)
                {
                    // 구르기 dash 중 — 입력 속도 무시, dash 속도 유지 (임펄스 보존)
                    velocity = _dashVelocity;
                }
                else
                {
                    float spd = _isRunning ? moveSpeed * runMultiplier : moveSpeed;
                    velocity = _moveInput * spd;
                }
                velocity.y = _rigidbody.linearVelocity.y;
                _rigidbody.linearVelocity = velocity;

                UpdateFacing();

                // 이동 상태 — 입력 기반. PlayerAnimator 가 동기·스무딩.
                int ms = (_moveInput.sqrMagnitude < 0.01f) ? 0 : (_isRunning ? 2 : 1);
                if (_playerAnimator != null) _playerAnimator.SetMoveStateServer(ms);
            }
        }

        // 서버 — 캐릭터 회전. 락온 시 타겟 / 평소 이동방향. 정지 + 락온 없음 = 현재 facing 유지.
        private void UpdateFacing()
        {
            if (Time.time < _dashUntil) return;   // 구르기 중 회전 잠금 — dash 방향 고정 (애니 깨짐 방지)

            Vector3 faceDir;
            Transform lockTarget = _lockOn != null ? _lockOn.CurrentTarget : null;
            if (lockTarget != null)
                faceDir = lockTarget.position - transform.position;
            else
                faceDir = _moveInput;
            faceDir.y = 0f;
            if (faceDir.sqrMagnitude < 0.0001f) return;

            Quaternion want = Quaternion.LookRotation(faceDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, want, turnSpeed * Time.fixedDeltaTime);
        }

        // IDashable — 구르기 스킬이 호출. 일정 시간 입력 속도 대신 dash 속도 적용.
        public void BeginDashServer(Vector3 direction, float speed, float duration)
        {
            if (!IsServer) return;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            _dashVelocity = direction.normalized * speed;
            _dashUntil = Time.time + duration;
        }

        [ServerRpc]
        private void SubmitMoveServerRpc(Vector3 horizontal, bool running)
        {
            _moveInput = horizontal;
            _isRunning = running;
        }
    }
}
