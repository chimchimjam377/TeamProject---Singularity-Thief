using UnityEngine;

public class KeyWall : MonoBehaviour
{
    private bool unlocked;

    public void Unlock()
    {
        if (unlocked)
            return;

        unlocked = true;

        Debug.Log("ø≠ºË ∫Æ «ÿ¡¶!");

        Destroy(gameObject);
    }
}