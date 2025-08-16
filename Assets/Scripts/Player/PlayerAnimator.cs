using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator  animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if ( Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("isJump");
        }
    }
}
