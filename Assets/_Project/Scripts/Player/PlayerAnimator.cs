using Unity.Netcode;
using UnityEngine;
using Elyqara.Skills;

namespace Elyqara.Player
{
    // Kiyan Animator 단일 소유 컴포넌트. Animator 에 접근하는 코드는 전부 여기로 모음.
    // 동기 패턴 = 코드베이스 기존 방식 그대로 — "목표 상태는 네트워크, 적용·스무딩은 각 클라 로컬".
    // NetworkAnimator 미사용 (같은 카테고리 2패턴 + MoveState 이중 동기 충돌 회피).
    //  - MoveState : 서버가 SetMoveStateServer 로 목표값(0 정지/1 걷기/2 달리기) 동기 →
    //                전 클라가 Mathf.MoveTowards 로 부드럽게 블렌드 (13-C 과제1 — snap 끊김 제거).
    //  - Dead bool : PlayerResources.IsDown 을 전 클라가 매 프레임 반영.
    //  - 트리거    : 서버가 PlayTriggerServer → ClientRpc 로 전 클라 SetTrigger (스킬/피격).
    [RequireComponent(typeof(PlayerResources))]
    public sealed class PlayerAnimator : NetworkBehaviour
    {
        [SerializeField] private float moveStateBlendSpeed = 6f;

        private readonly NetworkVariable<int> _moveState = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Animator _animator;
        private PlayerResources _resources;
        private float _displayMoveState;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _resources = GetComponent<PlayerResources>();
        }

        private void Update()
        {
            if (_animator == null) return;

            _displayMoveState = Mathf.MoveTowards(
                _displayMoveState, _moveState.Value, moveStateBlendSpeed * Time.deltaTime);
            _animator.SetFloat("MoveState", _displayMoveState);

            _animator.SetBool("Dead", _resources != null && _resources.IsDown.Value);
        }

        // 서버 전용 — PlayerMovement 가 입력 기반으로 판정한 이동 상태를 동기.
        public void SetMoveStateServer(int state)
        {
            if (!IsServer) return;
            if (_moveState.Value != state) _moveState.Value = state;
        }

        // 서버 전용 — 스킬/피격 1회성 애니 트리거. 전 클라에 발동.
        public void PlayTriggerServer(CharacterAnim anim)
        {
            if (!IsServer || anim == CharacterAnim.None) return;
            PlayTriggerClientRpc(anim);
        }

        [ClientRpc]
        private void PlayTriggerClientRpc(CharacterAnim anim)
        {
            // enum 이름 = Kiyan.controller 의 트리거 파라미터명과 1:1 일치.
            if (_animator != null) _animator.SetTrigger(anim.ToString());
        }
    }
}
