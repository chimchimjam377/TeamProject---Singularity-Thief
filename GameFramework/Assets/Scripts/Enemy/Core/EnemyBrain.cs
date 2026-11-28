
using UnityEngine;

namespace GameFramework.Enemy
{
    public enum EnemyBrainState
    {
        Patrol,
        Chase,
        Investigate
    }

    [RequireComponent(typeof(GroundEnemyMovement))]
    public class EnemyBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GroundEnemyMovement movement;
        [SerializeField] private GroundEnemyPatrol patrol;

        [Header("Chase Settings")]
        [SerializeField] private float loseSightDelay = 1f;
        [SerializeField] private float targetRefreshDistance = 0.15f;

        [Header("Investigate Settings")]
        [SerializeField] private float investigateDuration = 2f;

        // 현재 행동 상태
        public EnemyBrainState CurrentState { get; private set; }

        // 마지막으로 플레이어를 발견한 위치
        public Vector2 LastKnownPosition { get; private set; }

        private float lastSeenTime;

        private float lastCommandedX;
        private bool hasCommandedPosition;

        private bool reachedInvestigationPoint;
        private float investigateTimer;

        private void Awake()
        {
            if (movement == null)
                movement = GetComponent<GroundEnemyMovement>();

            if (patrol == null)
                patrol = GetComponent<GroundEnemyPatrol>();
        }

        private void Start()
        {
            CurrentState = EnemyBrainState.Patrol;
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case EnemyBrainState.Patrol:
                    UpdatePatrol();
                    break;

                case EnemyBrainState.Chase:
                    UpdateChase();
                    break;

                case EnemyBrainState.Investigate:
                    UpdateInvestigate();
                    break;
            }
        }

        // 시각 시스템으로부터 플레이어 발견 정보를 전달받음
        public void ReportVisualTarget(Transform target)
        {
            if (target == null)
                return;

            // 추격 상태로 진입
            if (CurrentState != EnemyBrainState.Chase)
            {
                patrol?.PausePatrol();

                CurrentState = EnemyBrainState.Chase;

                hasCommandedPosition = false;

                Debug.Log($"{name} : 플레이어 발견! 추격 시작");
            }

            // 마지막 발견 위치 갱신
            LastKnownPosition = target.position;

            // 마지막 발견 시간 갱신
            lastSeenTime = Time.time;

            // 플레이어의 위치로 이동 명령
            SetChaseDestination(LastKnownPosition);
        }

        private void UpdatePatrol()
        {
            // 현재 순찰은 GroundEnemyPatrol이 담당
        }

        // 플레이어 추격
        private void UpdateChase()
        {
            // 일정 시간 동안 발견하지 못하면 조사 시작
            if (Time.time - lastSeenTime >= loseSightDelay)
            {
                StartInvestigate();
            }
        }

        // 추격 목적지 갱신
        private void SetChaseDestination(Vector2 position)
        {
            float targetX = position.x;

            if (!hasCommandedPosition ||
                Mathf.Abs(targetX - lastCommandedX) >= targetRefreshDistance)
            {
                movement.MoveTo(position);

                lastCommandedX = targetX;
                hasCommandedPosition = true;
            }
        }

        // 조사 상태 시작
        private void StartInvestigate()
        {
            CurrentState = EnemyBrainState.Investigate;

            reachedInvestigationPoint = false;
            investigateTimer = investigateDuration;

            hasCommandedPosition = false;

            // 마지막으로 발견했던 위치로 이동
            movement.MoveTo(LastKnownPosition);

            Debug.Log($"{name} : 플레이어의 마지막 위치 조사");
        }

        // 마지막 위치 조사
        private void UpdateInvestigate()
        {
            // 조사 위치에 아직 도착하지 않았다면
            if (!reachedInvestigationPoint)
            {
                if (!movement.HasDestination)
                {
                    reachedInvestigationPoint = true;

                    movement.Stop();

                    Debug.Log($"{name} : 주변 탐색 시작");
                }

                return;
            }

            // 주변을 탐색하는 시간
            investigateTimer -= Time.deltaTime;

            if (investigateTimer <= 0f)
            {
                ReturnToPatrol();
            }
        }

        // 순찰 복귀
        private void ReturnToPatrol()
        {
            CurrentState = EnemyBrainState.Patrol;

            hasCommandedPosition = false;

            // 기존 순찰 재개
            if (patrol != null)
            {
                patrol.ResumePatrol();
            }
            else
            {
                movement.Stop();
            }

            Debug.Log($"{name} : 순찰 복귀");
        }
    }
}
