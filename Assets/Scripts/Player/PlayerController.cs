using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController instance;

    [Header("Movement")]
    public float walkSpeed = 2f;      // �⺻ �ȱ� �ӵ�
    public float runSpeed = 4f;       // Shift �޸��� �ӵ�
    public float jumpForce = 4f;      // ���� ��

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
    public bool isRunning = false; // �޸��� ����

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
        // �̵� �Է�
        float x = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(x, 0);

        // Shift �Է� Ȯ�� (�¿� �̵� ���� ���� �� Ȱ��ȭ)
        isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(x) > 0.01f;

        // "Idle ����" ���� (OR ó��)
        bool isIdle = ((!isRunning) || Mathf.Abs(x) < 0.01f) && isGrounded;

        // ���� �Է�
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpPressed = true;
        }

        // ���� �Է�
        if (Input.GetMouseButtonDown(0))
            TryAttack();

        //�ִϸ����� ������Ʈ
        playerAnim?.UpdateAnimator(
            Mathf.Abs(x),            // speed (����)
            rb.linearVelocity.y,     // y�ӵ�
            isGrounded,              // ���� �پ�����
            isRunning,               // �޸��� ����
            isIdle                   // Idle ����
        );
    }

    void FixedUpdate()
    {
        // ���� �ӵ� ���� (�ȱ� or �޸���)
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // �¿� �̵�
        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);

        // ���� ��ȯ (�¿� ����)
        if (sr)
        {
            if (moveInput.x > 0.01f) sr.flipX = false;
            else if (moveInput.x < -0.01f) sr.flipX = true;
        }

        // ����
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;

            // ���� �ִϸ��̼� Ʈ����
            playerAnim?.TriggerJump();

            jumpPressed = false;
        }
    }

    // ���� ó��
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        int index = Random.Range(1, 3); // 1 �Ǵ� 2 ����
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

    // ������ ó��
    public void TakeDamage(int amount, bool critical = false)
    {
        currentHP -= amount;

        if (critical) playerAnim?.TriggerHitWhite();
        else playerAnim?.TriggerHit();

        if (currentHP <= 0)
        {
            playerAnim?.TriggerDeath();
            enabled = false; // ������ ���� ó��
        }
    }

    // IDamageable �������̽� ����
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

    // �浹�� �� üũ
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}













