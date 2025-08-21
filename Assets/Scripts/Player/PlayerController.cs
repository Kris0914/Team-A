using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController instance;

    [Header("Movement")]
    public float walkSpeed = 2f;      // 기본 걷기 속도
    public float runSpeed = 4f;       // Shift 달리기 속도
    public float jumpForce = 4f;      // 점프 힘
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    public LayerMask groundMask = 1;

    [Header("Combat")]
    public Transform meleePoint;
    public float meleeRange = 0.75f;
    public LayerMask enemyMask;
    public int damage = 10;
    public float attackCooldown = 0.35f;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("카메라 가용 영역")]
    public List<PolygonCollider2D> CameraAreas = new List<PolygonCollider2D>();
    public int CurrentMap;

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

    private int lastDirection = 1;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool wasGrounded;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        playerAnim = GetComponent<PlayerAnimator>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHP = maxHP;

        if (groundCheckPoint == null)
        {
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(transform);
            groundCheck.transform.localPosition = new Vector3(0, -0.25f, 0);
            groundCheckPoint = groundCheck.transform;
        }
    }

    void Update()
    {
        HandleInput();
        CheckGrounded();
        UpdateTimers();
        UpdateAnimator();
    }

    public void SetCameraArea(int num)
    {
        CinemachineConfiner2D cinemachineConfiner2D = GameObject.Find("CinemachineCamera").GetComponent<CinemachineConfiner2D>();
        cinemachineConfiner2D.BoundingShape2D = CameraAreas[num];
        cinemachineConfiner2D.InvalidateBoundingShapeCache();

    }

    public IEnumerator NoFollowCamera()
    {
        CinemachineCamera originalCamera = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();

        // 임시 고정 카메라 생성
        GameObject tempCameraObj = new GameObject("TempStaticCamera");
        CinemachineCamera tempCamera = tempCameraObj.AddComponent<CinemachineCamera>();

        // 현재 카메라 위치와 회전을 임시 카메라에 복사
        tempCamera.transform.position = originalCamera.transform.position;
        tempCamera.transform.rotation = originalCamera.transform.rotation;

        // 임시 카메라를 더 높은 우선순위로 설정 (Follow 기능 없음)
        tempCamera.Priority = originalCamera.Priority + 10;

        // 기존 카메라 비활성화
        originalCamera.gameObject.SetActive(false);

        // 맵 전환 시간만큼 대기
        yield return new WaitForSeconds(0.5f);

        // 기존 카메라 다시 활성화
        originalCamera.gameObject.SetActive(true);

        // 임시 카메라 삭제
        Destroy(tempCameraObj);
    }

    void HandleInput()
    {
        if (DialougeManager.instance.isActing)
        {
            // 대화 중일 때는 입력을 모두 초기화
            moveInput = Vector2.zero;
            isRunning = false;
            return;
        }
        float x = 0f;

        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (left && !right)
        {
            x = -1f;
            lastDirection = -1;
        }
        else if (right && !left)
        {
            x = 1f;
            lastDirection = 1;
        }
        else if (left && right)
        {
            // 둘 다 눌렸을 때 마지막 방향 유지
            x = lastDirection;
        }

        moveInput = new Vector2(x, 0);

        // Shift 입력 확인 (좌우 이동 중일 때만 런 활성화)
        isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(x) > 0.01f;

        // 점프 입력
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }

        // 공격 입력
        if (Input.GetMouseButtonDown(0))
            TryAttack();
    }

    void CheckGrounded()
    {
        wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapBox(
            groundCheckPoint.position,
            groundCheckSize,
            0f,
            groundMask
        );

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    void UpdateTimers()
    {
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }

    void UpdateAnimator()
    {
        // "Idle 상태" 조건 (OR 처리)
        bool isIdle = ((!isRunning) || Mathf.Abs(moveInput.x) < 0.01f) && isGrounded;

        //애니메이터 업데이트
        playerAnim?.UpdateAnimator(
            Mathf.Abs(moveInput.x),  // speed (절댓값)
            rb.linearVelocity.y,     // y속도
            isGrounded,              // 땅에 붙었는지
            isRunning,               // 달리는 상태
            isIdle                   // Idle 상태
        );
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        // 현재 속도 선택 (걷기 or 달리기)
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 좌우 이동
        float targetVelocityX = moveInput.x * currentSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);

        // 방향 전환 (좌우 반전)
        if (sr && Mathf.Abs(moveInput.x) > 0.01f)
        {
            sr.flipX = moveInput.x < 0;
        }
    }

    void HandleJump()
    {
        bool canJump = jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0);

        if (canJump)
        {
            // 점프
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // 점프 애니메이션 트리거
            playerAnim?.TriggerJump();

            jumpBufferTimer = 0;
            coyoteTimer = 0;

            isGrounded = false;
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
        foreach (var hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
        }
    }

    public void TakeDamage(int amount, bool critical = false)
    {
        currentHP -= amount;

        if (critical)
            playerAnim?.TriggerHitWhite();
        else
            playerAnim?.TriggerHit();

        if (currentHP <= 0)
        {
            playerAnim?.TriggerDeath();
            enabled = false;
        }
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, false);
    }

    void OnDrawGizmosSelected()
    {
        // 공격 범위 기즈모
        if (meleePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
        }

        // 지상 체크 기즈모
        if (groundCheckPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}














