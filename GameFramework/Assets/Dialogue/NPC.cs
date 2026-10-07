using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour
{
    public DialogueDataSO myDialogue;           //npc 만의 고유 대화 데이터
    private DialogueManager dialogueManager;    //대화 매니저 참조

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.Log("다이얼 로그 매니저가 없습니다.");
        }
    }

   private void OnMouseDown()
   {
       if (dialogueManager == null) return;                 //매니저가 없으면 실행 안함
       if (dialogueManager.IsDialogueActive()) return;       //이미 대화 중이면 실행 안함
       if (myDialogue == null) return;                      //대화 데이터가 없으면 실행 안함

       dialogueManager.StartDialogue(myDialogue);           //모든 조건이 만족 되면 내 대화 시작
   }

    void Update()
    {
        
    }


  
}
