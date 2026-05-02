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

        // 단계 9 — 다운 상태. HP 0 시 true. 다운 중 추가 데미지 X. ReviveServer 로 복귀.
        public NetworkVariable<bool> IsDown = new(
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
            if (IsDown.Value) return;  // 다운 중 추가 데미지 차단
            Health.Value = Mathf.Max(0f, Health.Value - amount);
            if (Health.Value <= 0f) IsDown.Value = true;
        }

        public void ConsumeStaminaServer(float amount)
        {
            if (!IsServer) return;
            Stamina.Value = Mathf.Max(0f, Stamina.Value - amount);
        }

        // 단계 9 — 다른 Player 가 부활시킬 때 호출. 호스트만.
        public void ReviveServer(float reviveHp)
        {
            if (!IsServer) return;
            if (!IsDown.Value) return;
            float maxHp = _data != null ? _data.maxHealth : 100f;
            Health.Value = Mathf.Clamp(reviveHp, 1f, maxHp);
            IsDown.Value = false;
        }
    }
}
