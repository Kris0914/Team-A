using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        // Animator�� �ڽ�(��: PlayerSprite)�� ���� �� ������ �ڽ� ���� Ž��
        animator = GetComponentInChildren<Animator>(true);
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
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isIdle", isIdle);
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
}




