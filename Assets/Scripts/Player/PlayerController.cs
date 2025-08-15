using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("이동")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("전투")]
    public float attackCooldown = 0.4f;
    public int damage = 10;
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public LayerMask enemyMask;

    [Header("체력")]
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
        // 바닥 체크 간단 버전
        isGrounded = rb.linearVelocity.y == 0;

        if (isGrounded) jumpCount = 0;

        // 애니메이션(없으면 무시됨)
        if (anim)
        {
            anim.SetFloat("speed", Mathf.Abs(moveInput.x));
            anim.SetBool("grounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        // 이동
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // 방향전환
        if (moveInput.x > 0.01f) sr.flipX = false;
        else if (moveInput.x < -0.01f) sr.flipX = true;

        // 점프처리
        if (jumpPressed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
        jumpPressed = false;
    }

    // --- Input System 콜백 ---
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
            enabled = false; // 간단한 죽음 처리
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



