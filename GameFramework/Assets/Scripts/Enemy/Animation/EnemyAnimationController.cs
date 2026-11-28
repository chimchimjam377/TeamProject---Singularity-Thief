
using UnityEngine;

namespace GameFramework.Enemy
{
    public class EnemyAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Animation Settings")]
        [SerializeField] private float crossFadeDuration = 0.05f;

        private string currentAnimation;
        private bool isDead;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        // 애니메이션 재생 공통 메서드
        private void PlayAnimation(string animationName)
        {
            if (animator == null || isDead)
                return;

            // 이미 재생 중인 애니메이션이라면 무시
            if (currentAnimation == animationName)
                return;

            int animationHash = Animator.StringToHash(animationName);

            // Animator Controller에 상태가 존재하는지 확인
            if (!animator.HasState(0, animationHash))
            {
                Debug.LogWarning(
                    $"[{name}] 애니메이션 상태를 찾을 수 없습니다: {animationName}"
                );

                return;
            }

            currentAnimation = animationName;

            animator.CrossFadeInFixedTime(
                animationHash,
                crossFadeDuration
            );
        }

        public void PlayIdle()
        {
            PlayAnimation("Idle");
        }

        public void PlayWalk()
        {
            PlayAnimation("Walk");
        }

        public void PlayAttack()
        {
            PlayAnimation("Attack01");
        }

        public void PlayHit()
        {
            PlayAnimation("Hit");
        }

        public void PlayDeath()
        {
            if (isDead)
                return;

            isDead = true;
            currentAnimation = "Dead";

            animator.CrossFadeInFixedTime(
                Animator.StringToHash("Dead"),
                crossFadeDuration
            );
        }
    }
}
