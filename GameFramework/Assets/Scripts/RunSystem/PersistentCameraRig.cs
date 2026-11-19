using UnityEngine;

public class PersistentCameraRig : MonoBehaviour
{
    public static PersistentCameraRig Instance { get; private set; }

    private void Awake()
    {
        // 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 카메라 리그 유지
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}