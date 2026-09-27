using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용")]
    public float interactionRange = 2f;
    public LayerMask evidenceLayer;

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Collider[] objects = Physics.OverlapSphere(
            transform.position,
            interactionRange,
            evidenceLayer
        );

        if (objects.Length == 0)
            return;

        EvidenceObject closestEvidence = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider obj in objects)
        {
            EvidenceObject evidence =
                obj.GetComponent<EvidenceObject>();

            if (evidence == null)
                continue;

            float distance =
                Vector3.Distance(transform.position, obj.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEvidence = evidence;
            }
        }

        if (closestEvidence != null)
        {
            closestEvidence.Interact();
        }
    }
}