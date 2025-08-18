using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{

    public Animator animator;

    private void Awake()
    {

            
            
            // Animator�� �ڽ�(��: PlayerSprite)�� ���� �� ������ �ڽ� ���� Ž��
            animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PlayerAnimator] Animator not found. Put an Animator on this object or a child.");
        }
    }

    // PlayerController���� �� ������ ȣ��
    public void UpdateAnimator(float moveX, float vy, bool isGrounded, bool isRunning, bool isIdle)
    {
        if (animator == null) return;

        animator.SetFloat("speed", Mathf.Abs(moveX));
        animator.SetFloat("vy", vy);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isRunning", isRunning);
    }

    // Ʈ���ŵ� (PlayerController���� ȣ��)
    public void TriggerJump()
    {
        if (animator == null) return;
        animator.SetTrigger("isJump");
    }

    public void TriggerAttack(int index)
    {
        if (animator == null) return;
        animator.SetTrigger("isAttack");
        animator.SetInteger("attackIndex", index);
    }

    public void TriggerHit()
    {
        if (animator == null) return;
        animator.SetTrigger("isHit");
    }

    public void TriggerHitWhite()
    {
        if (animator == null) return;
        animator.SetTrigger("isHitWhite");
    }

    public void TriggerDeath()
    {
        if (animator == null) return;
        animator.SetTrigger("isDead");
    }



    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("Attack1");
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.SetTrigger("Attack2");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Hit");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Death");
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            LoadSceneManager.instance.LoadSence(0);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            LoadSceneManager.instance.LoadSence(1);
        }



        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) {
            animator.SetBool("isRunning", true);
        } else if(!Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            animator.SetBool("isRunning", false);
        }
    }
}




