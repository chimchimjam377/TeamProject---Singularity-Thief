
using UnityEngine;

namespace GameFramework.Enemy
{
    public class EnemyAnimationTest : MonoBehaviour
    {
        [SerializeField]
        private EnemyAnimationController animationController;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                animationController.PlayIdle();

            if (Input.GetKeyDown(KeyCode.Alpha2))
                animationController.PlayWalk();

            if (Input.GetKeyDown(KeyCode.Alpha3))
                animationController.PlayAttack();

            if (Input.GetKeyDown(KeyCode.Alpha4))
                animationController.PlayHit();

            if (Input.GetKeyDown(KeyCode.Alpha5))
                animationController.PlayDeath();
        }
    }
}
