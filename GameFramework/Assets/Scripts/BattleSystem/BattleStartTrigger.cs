using UnityEngine;

public class BattleStartTrigger : MonoBehaviour
{
    // 2D 게임이라면 OnTriggerEnter2D(Collider2D other)로 변경하세요.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.StartBattle();
        }
    }
}