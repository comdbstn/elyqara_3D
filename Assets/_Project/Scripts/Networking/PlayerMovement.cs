using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elyqara.Networking
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody _rigidbody;
        private InputAction _moveAction;
        private Vector2 _inputBuffer;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _moveAction.Enable();
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) _moveAction.Disable();
        }

        private void Update()
        {
            if (!IsOwner) return;
            _inputBuffer = _moveAction.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            SubmitMoveServerRpc(_inputBuffer);
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
