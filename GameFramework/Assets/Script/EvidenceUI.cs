using UnityEngine;
using TMPro;

public class EvidenceUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI evidenceNameText;
    public TextMeshProUGUI evidenceDescriptionText;

    public void ShowEvidence(string evidenceName, string evidenceDescription)
    {
        panel.SetActive(true);

        evidenceNameText.text = evidenceName;
        evidenceDescriptionText.text = evidenceDescription;
    }

    public void HideEvidence()
    {
        panel.SetActive(false);
    }
}