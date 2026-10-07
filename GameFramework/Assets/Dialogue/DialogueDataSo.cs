using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DialogueChoice
{
    public string choiceText;                             //선택지 버튼에 드어갈 텍스트
    public DialogueDataSO nextDialogue;                   //선택 시 연결될 다음 대화 데이터
}

[CreateAssetMenu(fileName = "DialogueDataSO", menuName = "Scriptable Objects/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    [Header("캐릭터 정보")]
    public string characterName = "캐릭터";                //대화 창에 표시된 캐릭터 이름
    public Sprite characterImage;                          //캐릭터 초상화

    [Header("대화 내용")]
    [TextArea(3,10)]                                         //인스펙터 창에서 여러 줄 입력 가능하게 창 설정
    public List <string> dialogueLines = new List<string>();  //대화 내용들

    [Header("대화 선택지")]
    public List <DialogueChoice> choices = new List<DialogueChoice>();  //대화가 끝난 후 등장할 선택지 목록

}
