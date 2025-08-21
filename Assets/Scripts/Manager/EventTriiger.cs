using UnityEngine;
using System.Collections;



public class EventTriiger : MonoBehaviour
{
    public BoxCollider2D boxCollider2D;
    public int dialogueNum;
    public bool isUse = false;

    private void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !isUse)
        {
            StartCoroutine(DialougeManager.instance.StartTalk(dialogueNum));
            isUse = true;
        }
    }
}
