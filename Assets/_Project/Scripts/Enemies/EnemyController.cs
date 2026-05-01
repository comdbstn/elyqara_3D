using Unity.Netcode;
using UnityEngine;
using Elyqara.Skills;

namespace Elyqara.Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyController : NetworkBehaviour, IEnemy, IDamageable
    {
        [SerializeField] private EnemyData data;

        public EnemyData Data => data;
        public float CurrentHealth => _health.Value;
        public bool IsAlive => _health.Value > 0f;

        private readonly NetworkVariable<float> _health = new(
            writePerm: NetworkVariableWritePermission.Server);

        private Rigidbody _rigidbody;
        private enum State { Idle, Chase, Attack }
        private State _state = State.Idle;
        private Transform _target;
        private float _attackCooldownLeft;
        private float _windupLeft;
        private bool _windupPending;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            if (data != null) _health.Value = data.maxHealth;
        }

        public void ApplyDamageServer(float amount)
        {
            if (!IsServer || !IsAlive) return;
            _health.Value = Mathf.Max(0f, _health.Value - amount);
            if (_health.Value <= 0f) DieServer();
        }

        private void DieServer()
        {
            // 단계 7 드랍 hook 자리 — 1차엔 Despawn 만
            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn(true);
        }

        private void Update()
        {
            if (!IsServer || data == null || !IsAlive) return;

            if (_attackCooldownLeft > 0f) _attackCooldownLeft -= Time.deltaTime;

            UpdateTargetAcquisition();

            switch (_state)
            {
                case State.Idle: TickIdle(); break;
                case State.Chase: TickChase(); break;
                case State.Attack: TickAttack(); break;
            }
        }

        private void UpdateTargetAcquisition()
        {
            // 가장 가까운 player 어그로 (1차 단순 룰)
            var clients = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClientsList : null;
            if (clients == null || clients.Count == 0) { _target = null; _state = State.Idle; return; }

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

            if (_target == null) { _state = State.Idle; return; }

            float dist = Mathf.Sqrt(bestSqr);
            if (_state == State.Idle && dist <= data.aggroRadius) _state = State.Chase;
            else if (dist > data.deaggroRadius) { _state = State.Idle; _target = null; }
        }

        private void TickIdle()
        {
            _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
        }

        private void TickChase()
        {
            if (_target == null) return;
            FaceTarget();
            float dist = Vector3.Distance(transform.position, _target.position);
            if (dist <= data.attackRange && _attackCooldownLeft <= 0f)
            {
                _state = State.Attack;
                _windupLeft = data.attackWindup;
                _windupPending = true;
                _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
                return;
            }
            Vector3 dir = (_target.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f) dir.Normalize();
            Vector3 v = dir * data.moveSpeed;
            v.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = v;
        }

        private void TickAttack()
        {
            FaceTarget();
            _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
            if (!_windupPending) { _state = State.Chase; return; }

            _windupLeft -= Time.deltaTime;
            if (_windupLeft > 0f) return;

            // Windup 끝 — 데미지 적용
            if (_target != null)
            {
                float dist = Vector3.Distance(transform.position, _target.position);
                if (dist <= data.attackRange + 0.3f)
                {
                    var dmg = _target.GetComponent<IDamageable>();
                    if (dmg != null) dmg.ApplyDamageServer(data.attackDamage);
                }
            }
            _windupPending = false;
            _attackCooldownLeft = data.attackCooldown;
            _state = State.Chase;
        }

        private void FaceTarget()
        {
            if (_target == null) return;
            Vector3 dir = _target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            Quaternion want = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, data.turnSpeedDegPerSec * Time.deltaTime);
        }
    }
}
