
using UnityEngine;

namespace GameFramework.Enemy
{
    public class GroundEnemyMovementTest : MonoBehaviour
    {
        [SerializeField]
        private GroundEnemyMovement movement;

        [Header("Test Targets")]
        [SerializeField] private Transform leftTarget;
        [SerializeField] private Transform rightTarget;

        private void Update()
        {
            // Q : 왼쪽 목적지로 이동
            if (Input.GetKeyDown(KeyCode.Q))
            {
                movement.MoveTo(leftTarget.position);
            }

            // E : 오른쪽 목적지로 이동
            if (Input.GetKeyDown(KeyCode.E))
            {
                movement.MoveTo(rightTarget.position);
            }

            // S : 정지
            if (Input.GetKeyDown(KeyCode.S))
            {
                movement.Stop();
            }
        }
    }
}
