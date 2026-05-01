using Unity.Netcode;
using UnityEngine;
using Elyqara.Characters;
using Elyqara.Skills;

namespace Elyqara.Player
{
    [RequireComponent(typeof(PlayerCharacterBinder))]
    public sealed class PlayerResources : NetworkBehaviour, IDamageable
    {
        public NetworkVariable<float> Health = new(
            writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<float> Stamina = new(
            writePerm: NetworkVariableWritePermission.Server);

        private PlayerCharacterBinder _binder;
        private CharacterData _data;

        private void Awake()
        {
            _binder = GetComponent<PlayerCharacterBinder>();
        }

        public override void OnNetworkSpawn()
        {
            Health.OnValueChanged += OnHealthChanged;
            if (!IsServer) return;

            _data = _binder.Character;
            float maxHp = _data != null ? _data.maxHealth : 100f;
            float maxSt = _data != null ? _data.maxStamina : 100f;
            Health.Value = maxHp;
            Stamina.Value = maxSt;
        }

        public override void OnNetworkDespawn()
        {
            Health.OnValueChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float prev, float now)
        {
            string side = IsServer ? "Host" : (IsOwner ? "Owner" : "Other");
            Debug.Log($"[Player HP cid={OwnerClientId}] {side}: {prev:F0} -> {now:F0}");
        }

        private void Update()
        {
            if (!IsServer || _data == null) return;
            if (Stamina.Value < _data.maxStamina)
            {
                float next = Stamina.Value + _data.staminaRegenPerSec * Time.deltaTime;
                Stamina.Value = Mathf.Min(next, _data.maxStamina);
            }
        }

        public void ApplyDamageServer(float amount)
        {
            if (!IsServer) return;
            Health.Value = Mathf.Max(0f, Health.Value - amount);
        }

        public void ConsumeStaminaServer(float amount)
        {
            if (!IsServer) return;
            Stamina.Value = Mathf.Max(0f, Stamina.Value - amount);
        }
    }
}
