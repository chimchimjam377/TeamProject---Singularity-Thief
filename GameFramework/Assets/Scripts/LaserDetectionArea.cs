using UnityEngine;

public class LaserDetectionArea : MonoBehaviour
{
    [SerializeField] private LaserTrap laserTrap;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        laserTrap.PlayerEntered(other.transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        laserTrap.PlayerExited(other.transform);
    }
}