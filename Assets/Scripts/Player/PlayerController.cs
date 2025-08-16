using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("이동")]
    public float moveSpeed = 2f;          // 요청값
    public float jumpForce = 4f;          // 요청값
    public LayerMask groundMask;          // Ground 레이어
    public Transform groundCheck;         // 발밑 체크 오브젝트
    public float groundCheckRadius = 0.15f;

    [Header("전투")]
    public Transform meleePoint;          // 손앞 빈 오브젝트
    public float meleeRange = 0.75f;
    public LayerMask enemyMask;           // EnemyHitbox 레이어
    public int damage = 10;
    public float attackCooldown = 0.35f;

    [Header("체력")]
    public int maxHP = 100;
    public int currentHP;

    // 내부
    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    Animator anim;

    Vector2 moveInput;
    bool jumpPressed;
    bool isGrounded;
    bool canJump;             // 점프 가능 여부
    float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        sr = GetComponentInChildren<SpriteRenderer>(true);
        anim = GetComponentInChildren<Animator>(true);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentHP = maxHP;
    }

    void Update()
    {
        // --- 이동 입력 (WASD / 방향키) ---
        float x = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(x, 0);

        // --- 점프 입력 (Space) ---
        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        // --- 바닥 체크 (혼합 방식: Layer + Tag) ---
        if (groundCheck != null)
        {
            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
            if (hit != null && hit.CompareTag("Ground"))
                isGrounded = true;
            else
                isGrounded = false;
        }
        else if (col != null)
        {
            var hit = Physics2D.BoxCast(
                col.bounds.center,
                new Vector2(col.bounds.size.x * 0.95f, col.bounds.size.y * 1.02f),
                0f,
                Vector2.down,
                0.05f,
                groundMask
            );

            if (hit.collider != null && hit.collider.CompareTag("Ground"))
                isGrounded = true;
            else
                isGrounded = false;
        }

        if (isGrounded) canJump = true;

        // --- 애니메이터 ---
        //if (anim)
        //{
            //anim.SetFloat("speed", Mathf.Abs(moveInput.x));
            //anim.SetBool("grounded", isGrounded);
        //}

        // --- 공격 입력 (좌클릭) ---
        if (Input.GetMouseButtonDown(0))
            TryAttack();
    }

    void FixedUpdate()
    {
        // --- 좌우 이동 ---
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // --- 바라보는 방향 (flipX) ---
        if (sr)
        {
            if (moveInput.x > 0.01f) sr.flipX = false;   // 오른쪽
            else if (moveInput.x < -0.01f) sr.flipX = true; // 왼쪽
        }

        // --- 점프 ---
        if (jumpPressed && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; // 공중 점프 방지
        }
        jumpPressed = false;
    }

    // --- 공격 ---
    void TryAttack()
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

    // --- 데미지 처리 ---
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

    // --- 디버그 Gizmos ---
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







