using UnityEngine;
using UnityEngine.SceneManagement;

public enum DoorType
{
    Transform,
    Scene,
};

public class Door : MonoBehaviour
{
    public DoorType type;
    public BoxCollider2D boxCollider2D;
    public Transform Goto;
    public int NextToNum;

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
            if(type == DoorType.Transform)
            {
                PlayerController.instance.gameObject.transform.position = Goto.position;
                PlayerController.instance.SetCameraArea(NextToNum);
                isTel = false;
            } else if(type == DoorType.Scene)
            {
                LoadSceneManager.instance.LoadSence(NextToNum);
            }

        }
    }
}
