
using UnityEngine;

namespace GameFramework.Enemy
{
    public class GroundEnemyPatrol : MonoBehaviour
    {
        public enum PatrolMode
        {
            Loop,       // 마지막 지점에서 처음으로 돌아감
            PingPong    // 마지막 지점에서 방향을 바꿈
        }

        [Header("References")]
        [SerializeField]
        private GroundEnemyMovement movement;

        [Header("Patrol Settings")]
        [SerializeField]
        private Transform[] waypoints;

        [SerializeField]
        private PatrolMode patrolMode = PatrolMode.PingPong;

        [SerializeField]
        private float waitTime = 1.5f;

        private int currentWaypointIndex;
        private int direction = 1;

        private float waitTimer;

        private bool isPatrolling;
        private bool isWaiting;
        private bool isPaused;

        // 현재 순찰 상태 확인
        public bool IsPatrolling => isPatrolling;

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<GroundEnemyMovement>();
            }
        }

        private void Start()
        {
            // 이동 컴포넌트 확인
            if (movement == null)
            {
                Debug.LogError(
                    "GroundEnemyMovement를 찾을 수 없습니다.",
                    this
                );

                enabled = false;
                return;
            }

            // 순찰 지점 확인
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning(
                    "순찰 지점이 설정되지 않았습니다.",
                    this
                );

                enabled = false;
                return;
            }

            foreach (Transform waypoint in waypoints)
            {
                if (waypoint == null)
                {
                    Debug.LogError(
                        "Waypoints에 비어 있는 항목이 있습니다.",
                        this
                    );

                    enabled = false;
                    return;
                }
            }

            isPatrolling = true;

            // 첫 번째 순찰 지점으로 이동
            MoveToCurrentWaypoint();
        }

        private void Update()
        {
            if (!isPatrolling || isPaused)
                return;

            // 대기 중이라면 타이머 진행
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                {
                    isWaiting = false;

                    AdvanceWaypoint();
                    MoveToCurrentWaypoint();
                }

                return;
            }

            // 목적지에 도착했는지 확인
            if (!movement.HasDestination)
            {
                StartWaiting();
            }
        }

        // 현재 순찰 지점으로 이동
        private void MoveToCurrentWaypoint()
        {
            Vector2 targetPosition =
                waypoints[currentWaypointIndex].position;

            movement.MoveTo(targetPosition);
        }

        // 도착 후 대기
        private void StartWaiting()
        {
            isWaiting = true;
            waitTimer = waitTime;
        }

        // 다음 순찰 지점 선택
        private void AdvanceWaypoint()
        {
            int count = waypoints.Length;

            if (count <= 1)
                return;

            if (patrolMode == PatrolMode.Loop)
            {
                currentWaypointIndex =
                    (currentWaypointIndex + 1) % count;
            }
            else
            {
                int nextIndex =
                    currentWaypointIndex + direction;

                // 마지막 지점 도착
                if (nextIndex >= count)
                {
                    direction = -1;
                    nextIndex = count - 2;
                }
                // 첫 번째 지점 도착
                else if (nextIndex < 0)
                {
                    direction = 1;
                    nextIndex = 1;
                }

                currentWaypointIndex = nextIndex;
            }
        }

        // 순찰 일시 정지
        public void PausePatrol()
        {
            if (!isPatrolling || isPaused)
                return;

            isPaused = true;

            movement.Stop();
        }

        // 순찰 재개
        public void ResumePatrol()
        {
            if (!isPatrolling || !isPaused)
                return;

            isPaused = false;

            // 이동 중 정지했던 경우 목적지로 다시 이동
            if (!isWaiting)
            {
                MoveToCurrentWaypoint();
            }
        }

        // 순찰 경로 표시
        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            Gizmos.color = Color.yellow;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    continue;

                Gizmos.DrawSphere(
                    waypoints[i].position,
                    0.12f
                );

                if (i < waypoints.Length - 1 &&
                    waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        waypoints[i].position,
                        waypoints[i + 1].position
                    );
                }
            }

            // Loop 방식은 마지막과 처음을 연결
            if (patrolMode == PatrolMode.Loop &&
                waypoints.Length > 1 &&
                waypoints[0] != null &&
                waypoints[waypoints.Length - 1] != null)
            {
                Gizmos.DrawLine(
                    waypoints[waypoints.Length - 1].position,
                    waypoints[0].position
                );
            }
        }
    }
}
