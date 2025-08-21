using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHP = 20;
    private int currentHP;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectRange = 5f; // 플레이어 추적 시작 범위
    public Transform groundCheck;
    public LayerMask groundMask;

    private Rigidbody2D rb;
    private Transform player;

    private int nextMove; // 랜덤 이동용
    private bool isDead = false;
    private bool isChasing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;

        if (groundCheck == null)
        {
            GameObject g = new GameObject("GroundCheck");
            g.transform.SetParent(transform);
            g.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = g.transform;
        }
    }

    void Start()
    {
        player = PlayerController.instance.transform;
        Invoke("Think", 2f); // 랜덤 이동 시작
    }

    void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            isChasing = dist < detectRange; // 범위 안에 들어오면 추적
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isChasing)
        {
            // 플레이어 추적
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // 랜덤 이동 (EnemyMove 로직)
            rb.linearVelocity = new Vector2(nextMove * moveSpeed, rb.linearVelocity.y);

            // 땅 체크 → 맵 밖으로 나가지 않도록
            RaycastHit2D rayHit = Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundMask);
            if (rayHit.collider == null)
            {
                nextMove *= -1;
                CancelInvoke();
                Invoke("Think", 2f);
            }
        }
    }

    // 랜덤 이동 결정
    void Think()
    {
        if (isChasing) return; // 추적 중에는 랜덤 이동 안 함

        nextMove = Random.Range(-1, 2); // -1, 0, 1
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    // 플레이어와 충돌 시 데미지
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var playerCtrl = collision.gameObject.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(5);
            }
        }
    }

    // 플레이어에게 공격 당함
    public void TakeDamage(AttackType type)
    {
        if (isDead) return;

        currentHP -= 5; // 좌클릭/우클릭 동일하게 5
        Debug.Log($"{gameObject.name} 피격! 현재 HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log($"{gameObject.name} 사망!");

        // 오브젝트 삭제 대신 비활성화
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange); // ✅ 오타 수정
    }
}

