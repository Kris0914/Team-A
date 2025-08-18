using UnityEngine;

public class Door : MonoBehaviour
{
    public BoxCollider2D boxCollider2D;
    public Transform Goto;

    public bool isTel = false;

    private void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTel = true;
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTel = true;
        }
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTel = false;
        }
    }


    private void Update()
    {
        if(isTel && Input.GetKeyDown(KeyCode.F))
        {
            PlayerController.instance.gameObject.transform.position = Goto.position;
            isTel = false;
        }
    }
}
