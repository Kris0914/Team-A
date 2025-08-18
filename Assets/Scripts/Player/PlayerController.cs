using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController instance;

    [Header("Movement")]
    public float walkSpeed = 2f;      // 기본 걷기 속도
    public float runSpeed = 4f;       // Shift 달리기 속도
    public float jumpForce = 4f;      // 점프 힘

    [Header("Combat")]
    public Transform meleePoint;
    public float meleeRange = 0.75f;
    public LayerMask enemyMask;
    public int damage = 10;
    public float attackCooldown = 0.35f;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    // internal
    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    PlayerAnimator playerAnim;

    public Vector2 moveInput;
    public bool jumpPressed;
    public bool isGrounded = true;
    public float lastAttackTime;
    public bool isRunning = false; // 달리기 여부

    void Awake()
    {

        if(instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }

            rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        playerAnim = GetComponent<PlayerAnimator>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHP = maxHP;
    }

    void Update()
    {
        // 이동 입력
        float x = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(x, 0);

        // Shift 입력 확인 (좌우 이동 중일 때만 런 활성화)
        isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(x) > 0.01f;

        // "Idle 상태" 조건 (OR 처리)
        bool isIdle = ((!isRunning) || Mathf.Abs(x) < 0.01f) && isGrounded;

        // 점프 입력
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpPressed = true;
        }

        // 공격 입력
        if (Input.GetMouseButtonDown(0))
            TryAttack();

        //애니메이터 업데이트
        playerAnim?.UpdateAnimator(
            Mathf.Abs(x),            // speed (절댓값)
            rb.linearVelocity.y,     // y속도
            isGrounded,              // 땅에 붙었는지
            isRunning,               // 달리는 상태
            isIdle                   // Idle 상태
        );
    }

    void FixedUpdate()
    {
        // 현재 속도 선택 (걷기 or 달리기)
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 좌우 이동
        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);

        // 방향 전환 (좌우 반전)
        if (sr)
        {
            if (moveInput.x > 0.01f) sr.flipX = false;
            else if (moveInput.x < -0.01f) sr.flipX = true;
        }

        // 점프
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;

            // 점프 애니메이션 트리거
            playerAnim?.TriggerJump();

            jumpPressed = false;
        }
    }

    // 공격 처리
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        int index = Random.Range(1, 3); // 1 또는 2 랜덤
        playerAnim?.TriggerAttack(index);

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

    // 데미지 처리
    public void TakeDamage(int amount, bool critical = false)
    {
        currentHP -= amount;

        if (critical) playerAnim?.TriggerHitWhite();
        else playerAnim?.TriggerHit();

        if (currentHP <= 0)
        {
            playerAnim?.TriggerDeath();
            enabled = false; // 간단한 죽음 처리
        }
    }

    // IDamageable 인터페이스 구현
    public void TakeDamage(int amount)
    {
        TakeDamage(amount, false);
    }

    void OnDrawGizmosSelected()
    {
        if (meleePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
        }
    }

    // 충돌로 땅 체크
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}













