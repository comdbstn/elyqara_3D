using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody _rigidbody;
        private PlayerInput _input;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInput>();
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
            if (!IsOwner) return;
            SubmitMoveServerRpc(_input.Move);
        }

        [ServerRpc]
        private void SubmitMoveServerRpc(Vector2 input)
        {
            Vector3 horizontal = new Vector3(input.x, 0f, input.y);
            if (horizontal.sqrMagnitude > 1f) horizontal.Normalize();

            Vector3 velocity = horizontal * moveSpeed;
            velocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = velocity;
        }
    }
}
