using Unity.Netcode;
using UnityEngine;
using Elyqara.Characters;
using Elyqara.Skills;

namespace Elyqara.Player
{
    [RequireComponent(typeof(PlayerCharacterBinder))]
    public sealed class PlayerResources : NetworkBehaviour, IDamageable, IInvincibilityTarget
    {
        public NetworkVariable<float> Health = new(
            writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<float> Stamina = new(
            writePerm: NetworkVariableWritePermission.Server);

        // 단계 9 — 다운 상태. HP 0 시 true. 다운 중 추가 데미지 X. ReviveServer 로 복귀.
        public NetworkVariable<bool> IsDown = new(
            writePerm: NetworkVariableWritePermission.Server);

        // 단계 13-2 — i-frame 무적 (RollDodge 등). 호스트 only. 시각 동기화 X (M12+).
        private float _invincibleUntil;
        public bool IsInvincible => Time.time < _invincibleUntil;

        public DamageFaction Faction => DamageFaction.Player;

        private PlayerCharacterBinder _binder;
        private PlayerAnimator _animator;
        private CharacterData _data;

        public float MaxHealth => _data != null ? _data.maxHealth : 100f;

        private void Awake()
        {
            _binder = GetComponent<PlayerCharacterBinder>();
            _animator = GetComponent<PlayerAnimator>();
        }

        public override void OnNetworkSpawn()
        {
#if UNITY_EDITOR
            Health.OnValueChanged += OnHealthChanged;
#endif
            if (!IsServer) return;

            _data = _binder.Character;
            float maxHp = _data != null ? _data.maxHealth : 100f;
            float maxSt = _data != null ? _data.maxStamina : 100f;
            Health.Value = maxHp;
            Stamina.Value = maxSt;
        }

        public override void OnNetworkDespawn()
        {
#if UNITY_EDITOR
            Health.OnValueChanged -= OnHealthChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnHealthChanged(float prev, float now)
        {
            string side = IsServer ? "Host" : (IsOwner ? "Owner" : "Other");
            Debug.Log($"[Player HP cid={OwnerClientId}] {side}: {prev:F0} -> {now:F0}");
        }
#endif

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
            if (IsDown.Value) return;       // 다운 중 추가 데미지 차단
            if (IsInvincible) return;       // 단계 13-2 — RollDodge i-frame

            // 단계 13-2 — Q 결전 buff 방어 보너스 적용
            var buff = GetComponent<TimedBuff>();
            if (buff != null && buff.IsActive)
                amount *= Mathf.Max(0f, 1f - buff.DefenseBonus);

            // 단계 13-2 — Kiyan 패시브 「작은 방패」 확률 감소
            var shield = GetComponent<KiyanShieldPassive>();
            if (shield != null) amount = shield.Modify(amount);

            Health.Value = Mathf.Max(0f, Health.Value - amount);
            if (Health.Value <= 0f)
                IsDown.Value = true;                                  // 사망 애니 = Dead bool (PlayerAnimator)
            else if (_animator != null)
                _animator.PlayTriggerServer(CharacterAnim.Hit);       // 비치명타 = 피격 애니
        }

        // 단계 13-2 — IInvincibilityTarget 구현. RollDodgeSkill 등 회피 스킬이 호출.
        public void SetInvincibleServer(float seconds)
        {
            if (!IsServer) return;
            _invincibleUntil = Time.time + seconds;
        }

        public void ConsumeStaminaServer(float amount)
        {
            if (!IsServer) return;
            Stamina.Value = Mathf.Max(0f, Stamina.Value - amount);
        }

        // 단계 9 — 다른 Player 가 부활시킬 때 호출. 호스트만.
        // 단계 10 fix — percent 전달로 변경. 캐릭터 max 가 100 아닌 경우 의도 깨짐 방지.
        public void ReviveServer(float reviveHpPercent)
        {
            if (!IsServer) return;
            if (!IsDown.Value) return;
            float maxHp = _data != null ? _data.maxHealth : 100f;
            float reviveHp = Mathf.Clamp01(reviveHpPercent) * maxHp;
            Health.Value = Mathf.Max(1f, reviveHp);
            IsDown.Value = false;
        }
    }
}
