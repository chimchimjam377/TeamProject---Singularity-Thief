using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private NPCProfile profile;
    [SerializeField] private Transform focusPoint;

    public NPCProfile Profile => profile;

    public Transform FocusPoint =>
        focusPoint != null ? focusPoint : transform;
}