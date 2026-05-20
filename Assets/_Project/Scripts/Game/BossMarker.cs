using Elyqara.Enemies;
using Unity.Netcode;
using UnityEngine;

namespace Elyqara.Game
{
    // 단계 10-B — 보스 처치 감지. EnemyController 와 같은 GO 에 부착.
    // 호스트에서만 polling — IsAlive == false 시 GameStateManager.IsVictory set (1회만).
    //
    // EnemyController 가 sealed 라 상속 X. 보스 = EnemyData (HP 큼) + BossMarker 컴포넌트 추가.
    // 단계 12+ 보스 phase / 다양한 무브셋은 BossController 별도 NetworkBehaviour 로 승격.
    [RequireComponent(typeof(EnemyController))]
    public sealed class BossMarker : NetworkBehaviour
    {
        private EnemyController _ctrl;
        private bool _reported;

        private void Awake()
        {
            _ctrl = GetComponent<EnemyController>();
        }

        private void Update()
        {
            if (!IsServer || _reported || _ctrl == null) return;
            if (_ctrl.IsAlive) return;

            _reported = true;
            var gsm = GameStateManager.Instance;
            if (gsm != null) gsm.ReportBossDefeatedServer();
        }
    }
}
