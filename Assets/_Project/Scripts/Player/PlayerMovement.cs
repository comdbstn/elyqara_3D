using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elyqara.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerResources))]
    public sealed class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runMultiplier = 1.8f;
        [SerializeField] private float lookSensitivity = 0.15f;

        private Rigidbody _rigidbody;
        private PlayerInput _input;
        private PlayerResources _resources;
        private PlayerAnimator _playerAnimator;
        private float _yaw;
        private Vector3 _moveInput;
        private bool _isRunning;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInput>();
            _resources = GetComponent<PlayerResources>();
            _playerAnimator = GetComponent<PlayerAnimator>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _input.EnableAll();
            _yaw = transform.eulerAngles.y;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) _input.DisableAll();
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (_resources != null && _resources.IsDown.Value) return;  // 다운 시 입력 차단
            float dx = _input.Look.x * lookSensitivity;
            if (Mathf.Abs(dx) > 0.0001f)
            {
                _yaw += dx;
                SubmitYawServerRpc(_yaw);
            }
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
                    // 입력 (x, y) 를 Player 의 forward/right 기준으로 변환 — yaw 회전 따라 이동 방향 회전
                    Vector3 forward = Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
                    Vector3 right = Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;
                    Vector3 horizontal = right * input.x + forward * input.y;
                    if (horizontal.sqrMagnitude > 1f) horizontal.Normalize();
                    bool running = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
                    SubmitMoveServerRpc(horizontal, running);
                }
            }

            // Server — 저장된 입력을 FixedUpdate 안에서 물리에 적용 (NGO 권장 — RPC 콜백 적용 = jitter).
            if (IsServer)
            {
                float spd = _isRunning ? moveSpeed * runMultiplier : moveSpeed;
                Vector3 velocity = _moveInput * spd;
                velocity.y = _rigidbody.linearVelocity.y;
                _rigidbody.linearVelocity = velocity;

                // 이동 상태 판정 — 입력 기반 deterministic. PlayerAnimator 가 동기·스무딩 담당.
                int ms = (_moveInput.sqrMagnitude < 0.01f) ? 0 : (_isRunning ? 2 : 1);
                if (_playerAnimator != null) _playerAnimator.SetMoveStateServer(ms);
            }
        }

        [ServerRpc]
        private void SubmitMoveServerRpc(Vector3 horizontal, bool running)
        {
            _moveInput = horizontal;
            _isRunning = running;  // 저장만 — 적용은 서버 FixedUpdate
        }

        [ServerRpc]
        private void SubmitYawServerRpc(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
