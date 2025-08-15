using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("�̵�")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("����")]
    public float attackCooldown = 0.4f;
    public int damage = 10;
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public LayerMask enemyMask;

    [Header("ü��")]
    public int maxHP = 100;
    public int currentHP;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    Vector2 moveInput;
    int jumpCount;
    bool isGrounded;
    bool jumpPressed;
    float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        currentHP = maxHP;
    }

    void Update()
    {
        // �ٴ� üũ ���� ����
        isGrounded = rb.linearVelocity.y == 0;

        if (isGrounded) jumpCount = 0;

        // �ִϸ��̼� (������ ���õ�)
        if (anim)
        {
            anim.SetFloat("speed", Mathf.Abs(moveInput.x));
            anim.SetBool("grounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        // �̵�
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // ���� ��ȯ
        if (moveInput.x > 0.01f) sr.flipX = false;
        else if (moveInput.x < -0.01f) sr.flipX = true;

        // ���� ó��
        if (jumpPressed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
        jumpPressed = false;
    }

    // --- Input System �ݹ� ---
    void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    void OnJump() => jumpPressed = true;

    void OnAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        if (anim) anim.SetTrigger("attack");
        DoAttack();
    }

    void DoAttack()
    {
        if (attackPoint == null) return;
        var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyMask);
        foreach (var h in hits)
        {
            var dmg = h.GetComponent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (anim) anim.SetTrigger("hurt");
        if (currentHP <= 0)
        {
            if (anim) anim.SetTrigger("die");
            enabled = false; // ������ ���� ó��
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}

public interface IDamageable
{
    void TakeDamage(int amount);
}

