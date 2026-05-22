using UnityEngine;
using UnityEngine.InputSystem;

namespace Elyqara.Player
{
    public sealed class PlayerInput : MonoBehaviour
    {
        public Vector2 Move => _moveAction.ReadValue<Vector2>();
        public Vector2 Look => _lookAction.ReadValue<Vector2>();

        public InputAction PrimaryAction => _primaryAction;
        public InputAction SecondaryAction => _secondaryAction;
        public InputAction QSkillAction => _qSkillAction;
        public InputAction DodgeAction => _dodgeAction;
        public InputAction PickupAction => _pickupAction;
        public InputAction ReviveAction => _reviveAction;
        public InputAction LockOnAction => _lockOnAction;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _primaryAction;
        private InputAction _secondaryAction;
        private InputAction _qSkillAction;
        private InputAction _dodgeAction;
        private InputAction _pickupAction;
        private InputAction _reviveAction;
        private InputAction _lockOnAction;

        private bool _isEnabled;

        private void Awake()
        {
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            // Mouse delta 는 매 frame raw input — PassThrough 가 표준. Value 타입은
            // Initial value 비교 로직 때문에 ReadValue 가 0 으로 읽히는 함정 케이스 있음.
            _lookAction = new InputAction("Look", InputActionType.PassThrough, expectedControlType: "Vector2");
            _lookAction.AddBinding("<Mouse>/delta");
            _lookAction.AddBinding("<Gamepad>/rightStick");

            _primaryAction = new InputAction("Primary", InputActionType.Button, "<Mouse>/leftButton");
            _primaryAction.AddBinding("<Gamepad>/rightTrigger");

            _secondaryAction = new InputAction("Secondary", InputActionType.Button, "<Mouse>/rightButton");
            _secondaryAction.AddBinding("<Gamepad>/leftTrigger");

            _qSkillAction = new InputAction("QSkill", InputActionType.Button, "<Keyboard>/q");
            _qSkillAction.AddBinding("<Gamepad>/buttonNorth");

            _dodgeAction = new InputAction("Dodge", InputActionType.Button, "<Keyboard>/space");
            _dodgeAction.AddBinding("<Gamepad>/buttonSouth");

            // 단계 7 — 픽업 액션
            _pickupAction = new InputAction("Pickup", InputActionType.Button, "<Keyboard>/f");
            _pickupAction.AddBinding("<Gamepad>/buttonWest");

            // 단계 9 — 부활 (E 키 hold)
            _reviveAction = new InputAction("Revive", InputActionType.Button, "<Keyboard>/e");
            _reviveAction.AddBinding("<Gamepad>/buttonEast");

            // 조작 개편 — 락온 토글 (마우스 휠클릭 / 게임패드 R3)
            _lockOnAction = new InputAction("LockOn", InputActionType.Button, "<Mouse>/middleButton");
            _lockOnAction.AddBinding("<Gamepad>/rightStickPress");
        }

        public void EnableAll()
        {
            if (_isEnabled) return;
            _moveAction.Enable();
            _lookAction.Enable();
            _primaryAction.Enable();
            _secondaryAction.Enable();
            _qSkillAction.Enable();
            _dodgeAction.Enable();
            _pickupAction.Enable();
            _reviveAction.Enable();
            _lockOnAction.Enable();
            _isEnabled = true;
        }

        public void DisableAll()
        {
            if (!_isEnabled) return;
            _moveAction.Disable();
            _lookAction.Disable();
            _primaryAction.Disable();
            _secondaryAction.Disable();
            _qSkillAction.Disable();
            _dodgeAction.Disable();
            _pickupAction.Disable();
            _reviveAction.Disable();
            _lockOnAction.Disable();
            _isEnabled = false;
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _lookAction?.Dispose();
            _primaryAction?.Dispose();
            _secondaryAction?.Dispose();
            _qSkillAction?.Dispose();
            _dodgeAction?.Dispose();
            _pickupAction?.Dispose();
            _reviveAction?.Dispose();
            _lockOnAction?.Dispose();
        }
    }
}
