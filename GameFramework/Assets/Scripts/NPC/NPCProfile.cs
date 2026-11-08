using UnityEngine;

[CreateAssetMenu(
    fileName = "NPCProfile",
    menuName = "Game/NPC/NPC Profile"
)]
public class NPCProfile : ScriptableObject
{
    [Header("NPC 정보")]
    [SerializeField] private string npcName;
    [SerializeField] private string factionName;

    [Header("이미지")]
    [SerializeField] private Sprite displaySprite;
    [SerializeField] private Sprite portraitSprite;

    [Header("대사")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;

    public string NPCName => npcName;
    public string FactionName => factionName;

    public Sprite DisplaySprite => displaySprite;
    public Sprite PortraitSprite => portraitSprite;

    public string[] DialogueLines => dialogueLines;
}