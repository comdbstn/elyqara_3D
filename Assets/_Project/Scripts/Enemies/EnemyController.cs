using Unity.Netcode;
using UnityEngine;
using Elyqara.Skills;
using Elyqara.Items;

namespace Elyqara.Enemies
{
    // 모든 적의 공통 베이스. 데이터 주도 (EnemyData SO).
    //
    // FSM: Idle → Chase → Anticipation → Active → Recovery → Chase (cooldown 후)
    //
    // 핵심 룰 (Souls-like 표준 — 모든 적 동일 적용):
    //   - Stopping distance: Chase 중 chaseStopDistance 유지. forward 박치기 X
    //   - 4-phase attack: Anticipation/Active/Recovery/Cooldown 분리. Recovery 동안 정지 (telegraph 무게감)
    //   - Active hitbox: 진입 순간 콘 OverlapSphere 한 번 (BasicMeleeSkill 과 동일 패턴)
    //   - 명시적 transition: Transition(AIState next) 한 곳에서만 _state 변경
    //   - Update = 로직 / FixedUpdate = 물리 (Rigidbody.linearVelocity)
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyController : NetworkBehaviour, IEnemy, IDamageable
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private bool logStateTransitions = true;

        public EnemyData Data => data;
        public float CurrentHealth => _health.Value;
        public bool IsAlive => _state != AIState.Dead && _health.Value > 0f;

        private readonly NetworkVariable<float> _health = new(
            writePerm: NetworkVariableWritePermission.Server);

        private enum AIState { Idle, Chase, Anticipation, Active, Recovery, Dead }
        private AIState _state = AIState.Idle;
        private Transform _target;
        private float _phaseTimer;     // 현재 phase 잔여 시간
        private float _cooldownLeft;   // Recovery 끝 후 다음 Anticipation 까지 wait

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            _health.OnValueChanged += OnHealthChanged;
            if (!IsServer) return;
            if (data != null) _health.Value = data.maxHealth;
        }

        public override void OnNetworkDespawn()
        {
            _health.OnValueChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float prev, float now)
        {
            string side = IsServer ? "Host" : "Client";
            Debug.Log($"[Wisp HP] {side}: {prev:F0} -> {now:F0}");
        }

        public void ApplyDamageServer(float amount)
        {
            if (!IsServer || _state == AIState.Dead) return;
            _health.Value = Mathf.Max(0f, _health.Value - amount);
            if (_health.Value <= 0f) Die();
        }

        private void Die()
        {
            Transition(AIState.Dead);

            // 단계 7 — 드랍. Despawn 전에 호출 (transform.position 사용 위해).
            if (data != null && data.dropTable != null)
            {
                ItemSpawner.SpawnFromTable(data.dropTable, transform.position);
            }

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn(true);
        }

        private void Update()
        {
            if (!IsServer || data == null || _state == AIState.Dead) return;

            if (_cooldownLeft > 0f) _cooldownLeft -= Time.deltaTime;

            AcquireTarget();
            TickState();
        }

        private void FixedUpdate()
        {
            if (!IsServer || _state == AIState.Dead) return;
            ApplyMovement();
        }

        // ==== Target ====
        private void AcquireTarget()
        {
            var clients = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClientsList : null;
            if (clients == null || clients.Count == 0) { _target = null; return; }

            float bestSqr = float.MaxValue;
            Transform best = null;
            for (int i = 0; i < clients.Count; i++)
            {
                var po = clients[i].PlayerObject;
                if (po == null) continue;
                float sqr = (po.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = po.transform; }
            }
            _target = best;
            if (_target != null && bestSqr > data.deaggroRadius * data.deaggroRadius)
                _target = null;
        }

        // ==== State logic ====
        private void TickState()
        {
            switch (_state)
            {
                case AIState.Idle:
                    if (_target != null && HorizontalDistance(_target.position, transform.position) <= data.aggroRadius)
                        Transition(AIState.Chase);
                    break;

                case AIState.Chase:
                    if (_target == null) { Transition(AIState.Idle); break; }
                    float dist = HorizontalDistance(_target.position, transform.position);
                    if (dist > data.deaggroRadius) { Transition(AIState.Idle); break; }
                    if (dist <= data.attackRange && _cooldownLeft <= 0f)
                        Transition(AIState.Anticipation);
                    break;

                case AIState.Anticipation:
                    _phaseTimer -= Time.deltaTime;
                    if (_phaseTimer <= 0f) Transition(AIState.Active);
                    break;

                case AIState.Active:
                    _phaseTimer -= Time.deltaTime;
                    if (_phaseTimer <= 0f) Transition(AIState.Recovery);
                    break;

                case AIState.Recovery:
                    _phaseTimer -= Time.deltaTime;
                    if (_phaseTimer <= 0f)
                    {
                        _cooldownLeft = data.attackCooldown;
                        Transition(AIState.Chase);
                    }
                    break;
            }
        }

        // 한 곳에서만 _state 변경. 진입 시 phase 초기화 + Active 진입 = swing 발사.
        private void Transition(AIState next)
        {
            AIState prev = _state;
            _state = next;
            switch (next)
            {
                case AIState.Anticipation: _phaseTimer = data.attackWindup; break;
                case AIState.Active:
                    _phaseTimer = data.attackActive;
                    PerformHit();   // ★ Anticipation 끝 = swing 순간 = hitbox 한 번
                    break;
                case AIState.Recovery: _phaseTimer = data.attackRecovery; break;
            }
            if (logStateTransitions) Debug.Log($"[{name}] {prev} -> {next}");
        }

        // ==== Movement (FixedUpdate) ====
        private void ApplyMovement()
        {
            Vector3 vel = _rigidbody.linearVelocity;

            if (_state == AIState.Chase && _target != null)
            {
                Face(_target.position);
                Vector3 toTarget = _target.position - transform.position;
                toTarget.y = 0f;
                float dist = toTarget.magnitude;
                if (dist > data.chaseStopDistance && dist > 0.01f)
                {
                    Vector3 dir = toTarget / dist;
                    vel.x = dir.x * data.moveSpeed;
                    vel.z = dir.z * data.moveSpeed;
                }
                else
                {
                    vel.x = 0f; vel.z = 0f;
                }
            }
            else if (_state == AIState.Anticipation || _state == AIState.Active || _state == AIState.Recovery)
            {
                if (_target != null && _state == AIState.Anticipation) Face(_target.position);
                // Active/Recovery 동안 회전 잠금 — swing 후 punish window 의 정직성
                vel.x = 0f; vel.z = 0f;
            }
            else // Idle
            {
                vel.x = 0f; vel.z = 0f;
            }

            _rigidbody.linearVelocity = vel;
        }

        private void Face(Vector3 worldPos)
        {
            Vector3 dir = worldPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            Quaternion want = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, data.turnSpeedDegPerSec * Time.fixedDeltaTime);
        }

        // ==== Hitbox ====
        private void PerformHit()
        {
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;
            float reach = data.attackRange + 0.5f;   // capsule radius 합 흡수용 약간의 여유
            Collider[] hits = Physics.OverlapSphere(origin, reach, ~0);
            float cosThreshold = Mathf.Cos(data.hitboxHalfAngleDeg * Mathf.Deg2Rad);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider c = hits[i];
                if (c == null || c.attachedRigidbody == null) continue;
                GameObject t = c.attachedRigidbody.gameObject;
                if (t == gameObject) continue;
                Vector3 toT = c.transform.position - origin;
                toT.y = 0f;
                if (toT.sqrMagnitude < 0.0001f) continue;
                toT.Normalize();
                if (Vector3.Dot(forward, toT) < cosThreshold) continue;

                IDamageable dmg = t.GetComponent<IDamageable>();
                if (dmg != null) dmg.ApplyDamageServer(data.attackDamage);
            }
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            Vector3 d = a - b; d.y = 0f;
            return d.magnitude;
        }
    }
}
