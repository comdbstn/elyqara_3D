using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Elyqara.Characters;
using Elyqara.Skills;

namespace Elyqara.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerCharacterBinder))]
    [RequireComponent(typeof(PlayerResources))]
    public sealed class PlayerSkillExecutor : NetworkBehaviour
    {
        private PlayerInput _input;
        private PlayerCharacterBinder _binder;
        private PlayerResources _resources;

        private float _primaryCdLeft;
        private float _secondaryCdLeft;
        private float _qSkillCdLeft;
        private float _dodgeCdLeft;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _binder = GetComponent<PlayerCharacterBinder>();
            _resources = GetComponent<PlayerResources>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _input.PrimaryAction.performed += OnPrimary;
            _input.SecondaryAction.performed += OnSecondary;
            _input.QSkillAction.performed += OnQSkill;
            _input.DodgeAction.performed += OnDodge;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            _input.PrimaryAction.performed -= OnPrimary;
            _input.SecondaryAction.performed -= OnSecondary;
            _input.QSkillAction.performed -= OnQSkill;
            _input.DodgeAction.performed -= OnDodge;
        }

        private void Update()
        {
            if (_primaryCdLeft > 0f) _primaryCdLeft -= Time.deltaTime;
            if (_secondaryCdLeft > 0f) _secondaryCdLeft -= Time.deltaTime;
            if (_qSkillCdLeft > 0f) _qSkillCdLeft -= Time.deltaTime;
            if (_dodgeCdLeft > 0f) _dodgeCdLeft -= Time.deltaTime;
        }

        private void OnPrimary(InputAction.CallbackContext _) => TryActivate(0);
        private void OnSecondary(InputAction.CallbackContext _) => TryActivate(1);
        private void OnQSkill(InputAction.CallbackContext _) => TryActivate(2);
        private void OnDodge(InputAction.CallbackContext _) => TryActivate(3);

        private void TryActivate(int slotIndex)
        {
            if (!IsOwner) return;
            CharacterData ch = _binder.Character;
            if (ch == null) return;

            SkillData skill = SlotToSkill(ch, slotIndex);
            if (skill == null) return;

            float cdLeft = SlotCooldown(slotIndex);
            if (cdLeft > 0f) return;

            ActivateServerRpc(slotIndex);
            SetSlotCooldown(slotIndex, skill.CooldownSeconds);
        }

        [ServerRpc]
        private void ActivateServerRpc(int slotIndex)
        {
            CharacterData ch = _binder.Character;
            if (ch == null) return;
            SkillData skill = SlotToSkill(ch, slotIndex);
            if (skill == null) return;

            // 스태미나 비용 호스트 검증
            if (skill.StaminaCost > 0f)
            {
                if (_resources.Stamina.Value < skill.StaminaCost) return;
                _resources.ConsumeStaminaServer(skill.StaminaCost);
            }
            skill.ActivateOnServer(gameObject);
        }

        private static SkillData SlotToSkill(CharacterData ch, int slotIndex) => slotIndex switch
        {
            0 => ch.primarySkill,
            1 => ch.secondarySkill,
            2 => ch.qSkill,
            3 => ch.dodgeSkill,
            _ => null,
        };

        private float SlotCooldown(int slotIndex) => slotIndex switch
        {
            0 => _primaryCdLeft,
            1 => _secondaryCdLeft,
            2 => _qSkillCdLeft,
            3 => _dodgeCdLeft,
            _ => 0f,
        };

        private void SetSlotCooldown(int slotIndex, float v)
        {
            switch (slotIndex)
            {
                case 0: _primaryCdLeft = v; break;
                case 1: _secondaryCdLeft = v; break;
                case 2: _qSkillCdLeft = v; break;
                case 3: _dodgeCdLeft = v; break;
            }
        }
    }
}
