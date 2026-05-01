using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSensitivity = 0.15f;

        private Rigidbody _rigidbody;
        private PlayerInput _input;
        private float _yaw;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInput>();
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
            float dx = _input.Look.x * lookSensitivity;
            if (Mathf.Abs(dx) > 0.0001f)
            {
                _yaw += dx;
                SubmitYawServerRpc(_yaw);
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            Vector2 input = _input.Move;
            // 입력 (x, y) 를 Player 의 forward/right 기준으로 변환 — yaw 회전 따라 이동 방향 회전
            Vector3 forward = Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;
            Vector3 horizontal = right * input.x + forward * input.y;
            if (horizontal.sqrMagnitude > 1f) horizontal.Normalize();
            SubmitMoveServerRpc(horizontal);
        }

        [ServerRpc]
        private void SubmitMoveServerRpc(Vector3 horizontal)
        {
            Vector3 velocity = horizontal * moveSpeed;
            velocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = velocity;
        }

        [ServerRpc]
        private void SubmitYawServerRpc(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
