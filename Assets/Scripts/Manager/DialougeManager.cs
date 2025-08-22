using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;








public class DialougeManager : MonoBehaviour
{
    public static DialougeManager instance;

    [SerializeField]
    public List<Talk> dialogs;
    public GameObject Dialogue;
    public TextMeshProUGUI text;
    public bool isActing = false;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }



    public IEnumerator StartTalk(int num)
    {
        Talk currentTalk = dialogs[num];
        Dialogue.SetActive(true);
        isActing = true;

        for (int i = 0; i < currentTalk.dialogues.Count; i++)
        {
            text.text = currentTalk.dialogues[i];

            // 스페이스바 누를 때까지 대기
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            // 한 프레임 대기
            yield return null;
        }
        isActing = false;
        Dialogue.SetActive(false);
    }
}
