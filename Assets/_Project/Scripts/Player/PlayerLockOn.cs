using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Elyqara.Skills;

namespace Elyqara.Player
{
    // 락온(적 타겟팅). 마우스 휠클릭 토글.
    // owner 가 카메라 정면 콘에서 타겟 선정 → 서버 검증 → NetworkVariable 동기.
    // PlayerCamera(프레이밍) / PlayerMovement(캐릭터 facing) 가 CurrentTarget 을 조회.
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerLockOn : NetworkBehaviour
    {
        [Tooltip("락온 가능 최대 거리 (m)")]
        [SerializeField] private float lockRange = 15f;
        [Tooltip("락온 해제 거리 (m). lockRange 보다 크게 — 경계 깜빡임 방지")]
        [SerializeField] private float releaseRange = 18f;
        [Tooltip("후보 콘 — 카메라 정면 기준 dot 하한 (1=정면, 0=측면)")]
        [SerializeField, Range(-1f, 1f)] private float minFacingDot = 0.1f;

        private readonly NetworkVariable<NetworkObjectReference> _target = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private PlayerInput _input;
        private PlayerCamera _camera;

        public bool IsLocked => CurrentTarget != null;

        // 락온 타겟. 미락온 시 null. owner(카메라) / 서버(이동 facing) 둘 다 조회.
        public Transform CurrentTarget
        {
            get
            {
                if (_target.Value.TryGet(out NetworkObject no) && no != null) return no.transform;
                return null;
            }
        }

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _camera = GetComponent<PlayerCamera>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _input.LockOnAction.performed += OnLockOnPressed;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) _input.LockOnAction.performed -= OnLockOnPressed;
        }

        private void OnLockOnPressed(InputAction.CallbackContext _)
        {
            if (!IsOwner) return;
            if (CurrentTarget != null) { ReleaseLockServerRpc(); return; }

            var best = FindBestTarget();
            if (best != null && best.TryGetComponent(out NetworkObject no))
                RequestLockServerRpc(new NetworkObjectReference(no));
        }

        // owner — 카메라 정면 콘 안에서 가장 정면에 가까운 적 선정.
        private Transform FindBestTarget()
        {
            Vector3 origin = transform.position;
            float camYaw = _camera != null ? _camera.Yaw : transform.eulerAngles.y;
            Vector3 camForward = Quaternion.Euler(0f, camYaw, 0f) * Vector3.forward;

            Collider[] hits = Physics.OverlapSphere(origin, lockRange, ~0, QueryTriggerInteraction.Ignore);
            Transform best = null;
            float bestScore = -2f;
            for (int i = 0; i < hits.Length; i++)
            {
                var rb = hits[i].attachedRigidbody;
                if (rb == null) continue;
                var dmg = rb.GetComponent<IDamageable>();
                if (dmg == null || dmg.Faction != DamageFaction.Enemy) continue;

                Vector3 to = rb.transform.position - origin;
                to.y = 0f;
                if (to.sqrMagnitude < 0.01f) continue;
                to.Normalize();

                float dot = Vector3.Dot(camForward, to);
                if (dot < minFacingDot) continue;          // 콘 밖
                if (dot > bestScore) { bestScore = dot; best = rb.transform; }
            }
            return best;
        }

        [ServerRpc]
        private void RequestLockServerRpc(NetworkObjectReference targetRef)
        {
            if (targetRef.TryGet(out NetworkObject no) && no != null)
            {
                float sqr = (no.transform.position - transform.position).sqrMagnitude;
                if (sqr <= lockRange * lockRange) _target.Value = targetRef;
            }
        }

        [ServerRpc]
        private void ReleaseLockServerRpc()
        {
            _target.Value = default;
        }

        private void Update()
        {
            if (!IsServer) return;
            // 서버 — 타겟 범위 이탈 시 해제. (사망 = despawn → CurrentTarget getter 가 자동 null)
            if (_target.Value.TryGet(out NetworkObject no) && no != null)
            {
                float sqr = (no.transform.position - transform.position).sqrMagnitude;
                if (sqr > releaseRange * releaseRange) _target.Value = default;
            }
        }
    }
}
