using System.Collections;
using System.Collections.Generic;

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController instance;
    public Slider slider;
    public Animator animator;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpForce = 4f;
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
    bool isHit = true;

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
    public bool isRunning = false;

    private int lastDirection = 1;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool wasGrounded;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        playerAnim = GetComponent<PlayerAnimator>();
        slider = GameObject.Find("HP Bar").GetComponent<Slider>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHP = maxHP;
        slider.maxValue = maxHP;
        slider.value = currentHP;
        


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

        GameObject tempCameraObj = new GameObject("TempStaticCamera");
        CinemachineCamera tempCamera = tempCameraObj.AddComponent<CinemachineCamera>();

        tempCamera.transform.position = originalCamera.transform.position;
        tempCamera.transform.rotation = originalCamera.transform.rotation;
        tempCamera.Priority = originalCamera.Priority + 10;

        originalCamera.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        originalCamera.gameObject.SetActive(true);
        Destroy(tempCameraObj);
    }

    // ================== 공격 부분 수정 ==================
    void HandleInput()
    {
        if (DialougeManager.instance.isActing)
        {
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
            x = lastDirection;
        }

        moveInput = new Vector2(x, 0);
        isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(x) > 0.01f;

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        // 좌클릭 → LeftClick 공격
        if (Input.GetMouseButtonDown(0))
            TryAttack(AttackType.LeftClick);

        // 우클릭 → RightClick 공격
        if (Input.GetMouseButtonDown(1))
            TryAttack(AttackType.RightClick);
    }

    void TryAttack(AttackType type)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        int index = Random.Range(1, 3);
        playerAnim?.TriggerAttack(index);

        DoMeleeDamage(type);
    }

    void DoMeleeDamage(AttackType type)
    {
        
        if (meleePoint == null) return;
        

        var hits = Physics2D.OverlapCircleAll(meleePoint.position, meleeRange, enemyMask);

        foreach (var hit in hits)
        {
            
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(type);
            }
        }
    }
    // ===================================================

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
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
    }

    void UpdateTimers()
    {
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }

    void UpdateAnimator()
    {
        bool isIdle = ((!isRunning) || Mathf.Abs(moveInput.x) < 0.01f) && isGrounded;

        playerAnim?.UpdateAnimator(
            Mathf.Abs(moveInput.x),
            rb.linearVelocity.y,
            isGrounded,
            isRunning,
            isIdle
        );
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        float targetVelocityX = moveInput.x * currentSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);

        if (sr && Mathf.Abs(moveInput.x) > 0.01f)
            sr.flipX = moveInput.x < 0;
    }

    void HandleJump()
    {
        bool canJump = jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0);

        if (canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            playerAnim?.TriggerJump();
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            isGrounded = false;
        }
    }
    IEnumerator Imotal()
    {
        isHit = false;
        yield return new WaitForSeconds(2);
        isHit = true;
    }
    // ====== 체력 관련 ======
    public void TakeDamage(int amount, bool critical = false)
    {
        if (isHit)
        {
            currentHP -= amount;
            slider.value = currentHP;
            animator.SetTrigger("Hit");
            if (currentHP <= 0)
            { 
                currentHP = 0;
                Die();
                //animator.SetTrigger("Death");
            }

            StartCoroutine(Imotal());

        }
  

        //if (critical)
            //playerAnim?.TriggerHitWhite();
        //else
            //playerAnim?.TriggerHit();

        
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, false);
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    private void Die()
    {
        Debug.Log("플레이어 사망");
        //playerAnim?.TriggerDeath();
        animator.SetTrigger("Death");
        enabled = false;
    }
    // =====================

    void OnDrawGizmosSelected()
    {
        if (meleePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
        }

        if (groundCheckPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}
















