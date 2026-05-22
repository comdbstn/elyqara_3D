using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Enemies
{
    // 적 1회성 애니 트리거 종류. 이름 = 적 Animator Controller 의 트리거 파라미터명과 1:1 일치.
    public enum EnemyAnim { None, Attack, Hit, Death }

    // 적 Animator 단일 소유 컴포넌트. Animator 에 접근하는 코드는 전부 여기로 모음.
    // 동기 패턴 = PlayerAnimator 와 동일 — "목표 상태는 네트워크, 적용·스무딩은 각 클라 로컬".
    //  - MoveState : 서버가 SetMoveStateServer 로 목표값(0 정지/1 이동) 동기 →
    //                전 클라가 Mathf.MoveTowards 로 부드럽게 블렌드.
    //  - 트리거    : 서버가 PlayTriggerServer → ClientRpc 로 전 클라 SetTrigger (공격/피격/사망).
    // 모델·Animator 가 아직 없으면(캡슐 상태) GetComponentInChildren 가 null → 전 메서드 무동작 (안전).
    public sealed class EnemyAnimator : NetworkBehaviour
    {
        [SerializeField] private float moveStateBlendSpeed = 6f;

        private readonly NetworkVariable<int> _moveState = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Animator _animator;
        private float _displayMoveState;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (_animator == null) return;

            _displayMoveState = Mathf.MoveTowards(
                _displayMoveState, _moveState.Value, moveStateBlendSpeed * Time.deltaTime);
            _animator.SetFloat("MoveState", _displayMoveState);
        }

        // 서버 전용 — EnemyController 가 이동 상태(0 정지 / 1 이동)를 동기.
        public void SetMoveStateServer(int state)
        {
            if (!IsServer) return;
            if (_moveState.Value != state) _moveState.Value = state;
        }

        // 서버 전용 — 공격/피격/사망 1회성 애니 트리거. 전 클라에 발동.
        public void PlayTriggerServer(EnemyAnim anim)
        {
            if (!IsServer || anim == EnemyAnim.None) return;
            PlayTriggerClientRpc(anim);
        }

        [ClientRpc]
        private void PlayTriggerClientRpc(EnemyAnim anim)
        {
            // enum 이름 = 적 Animator Controller 의 트리거 파라미터명과 1:1 일치.
            if (_animator != null) _animator.SetTrigger(anim.ToString());
        }
    }
}
