using UnityEngine;

public class BossBreakableWall : MonoBehaviour
{
    [SerializeField] private BossChainDevice chainDevice;
    private bool broken;
    public void Break()
    {
        if (broken) return;
        broken = true;
        if (chainDevice) chainDevice.Unlock();
        Debug.Log("Chain wall broken");
        gameObject.SetActive(false);
    }
}
