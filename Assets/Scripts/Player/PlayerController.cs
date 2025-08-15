using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("이동")]
    public float moveSpeed = 2f;          // 요청값
    public float jumpForce = 4f;          // 요청값
    public LayerMask groundMask;          // Ground 레이어 지정
    public Transform groundCheck;         // 발밑 빈 오브젝트
    public float groundCheckRadius = 0.15f;

    [Header("전투")]
    public Transform meleePoint;          // 손앞 빈 오브젝트
    public float meleeRange = 0.75f;
    public LayerMask enemyMask;           // EnemyHitbox 레이어 지정
    public int damage = 10;
    public float attackCooldown = 0.35f;

    [Header("체력")]
    public int maxHP = 100;
    public int currentHP;

    // --- 내부 ---
    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    Animator anim;

    Vector2 moveInput;
    bool jumpPressed;
    bool isGrounded;
    float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        // 스프라이트를 부모/자식 어디에 두어도 안전하게 찾기
        sr = GetComponentInChildren<SpriteRenderer>(true);
        anim = GetComponentInChildren<Animator>(true);

        // 뒤집힘(회전) 방지 – 물리 회전 고정
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentHP = maxHP;
    }

    void Update()
    {
        // --- 바닥 체크 ---
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        }
        else if (col != null)
        {
            // groundCheck가 비어있어도 안전하게 동작하도록 백업 체크
            var hit = Physics2D.BoxCast(col.bounds.center,
                                        new Vector2(col.bounds.size.x * 0.95f, col.bounds.size.y * 1.02f),
                                        0f, Vector2.down, 0.05f, groundMask);
            isGrounded = hit.collider != null;
        }

        // --- 애니메이터 파라미터(있을 때만) ---
        if (anim)
        {
            anim.SetFloat("speed", Mathf.Abs(moveInput.x));
            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("vy", rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        // --- 좌우 이동 ---
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // --- 좌우 바라보기 (flipX만 사용: 회전 X) ---
        if (sr)
        {
            if (moveInput.x > 0.01f) sr.flipX = false; // 오른쪽
            else if (moveInput.x < -0.01f) sr.flipX = true;  // 왼쪽
        }

        // --- 점프: 반드시 바닥에서만 ---
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        jumpPressed = false; // 한 프레임만 처리
    }

    // === Input System (Player Input: Behavior = Send Messages) ===
    void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    void OnJump() => jumpPressed = true;

    void OnAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        if (anim) anim.SetTrigger("attack");
        DoMeleeDamage();
    }

    void DoMeleeDamage()
    {
        if (meleePoint == null) return;

        var hits = Physics2D.OverlapCircleAll(meleePoint.position, meleeRange, enemyMask);
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
            enabled = false; // 간단한 사망 처리
        }
    }

    void OnDrawGizmosSelected()
    {
        if (meleePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
        }
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}



