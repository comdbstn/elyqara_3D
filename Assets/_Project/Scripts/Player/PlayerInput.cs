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

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _primaryAction;
        private InputAction _secondaryAction;
        private InputAction _qSkillAction;
        private InputAction _dodgeAction;

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

            _lookAction = new InputAction("Look", InputActionType.Value);
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
        }
    }
}
