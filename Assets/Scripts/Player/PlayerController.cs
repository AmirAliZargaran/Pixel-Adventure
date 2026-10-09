using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Physics Detection")]
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask layerGround;
    [SerializeField] private float wallCheckDistance = 0.6f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip groundDashSound;
    [SerializeField] private AudioClip midAirDashSound;
    [SerializeField] private AudioClip grabSound;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float wallJumpCooldown = 0.5f;
    [Header("Variable Jump (پرش با نگه داشتن)")]
    [Tooltip("شتاب رو به بالا در حالی که دکمه پرش نگه داشته شده")]
    [SerializeField] private float jumpHoldAccel = 30f;

    [Tooltip("حداکثر مدت اعمال شتاب (ثانیه) — سقف ارتفاع پرش")]
    [SerializeField] private float maxJumpHoldTime = 0.25f;

    [Tooltip("سقف سرعت رو به بالا هنگام پرش (ایمنی)")]
    [SerializeField] private float maxRiseSpeed = 12f;

    private float coyoteCounter;
    private float jumpCooldownTimer;
    private float jumpBufferCounter;
    private float jumpTimeCounter;   // زمان باقی‌مانده از شتاب پرش
    private bool jumpHeldActive;     // آیا شتاب پرش فعال است؟

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.2f;
    private bool isKnockbacked;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashGravityScale = 0f;
    [SerializeField] private int airDashCount = 1;
    [SerializeField] private GameObject groundDashEffectPrefab;
    [SerializeField] private GameObject midAirDashEffectPrefab;
    [SerializeField] private Transform dashEffectPoint;

    // ================== سقوط سریع با دکمه S ==================
    [Header("سقوط سریع (S)")]
    [Tooltip("نیروی ضربه اولیه هنگام فشردن S (Impulse)")]
    [SerializeField] private float fastFallImpulse = 15f;

    [Tooltip("شتاب ادامه‌دار هنگام نگه داشتن S")]
    [SerializeField] private float fastFallAccel = 30f;

    [Tooltip("سقف سرعت سقوط هنگام Fast-Fall")]
    [SerializeField] private float maxFallSpeed = 25f;

    [Tooltip("true = فقط ضربه اولیه؛ false = تا وقتی S نگه داشته شده شتاب ادامه دارد")]
    [SerializeField] private bool onlyOnPress = false;

    private bool fastFallPressed;  // S تازه فشرده شده (یک فریم)
    private bool fastFallHeld;     // S نگه داشته شده

    private bool isDashing;
    private bool canDash = true;
    private int canDashInAir = 1;
    private float defaultGravityScale;

    public bool IsDashing => isDashing;

    private MovingPlatform activePlatform;




    public bool IsGrounded { get; private set; }
    public bool IsWall { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsWinning { get; private set; }
    public bool IsWallSliding { get; private set; }
    public bool IsDoubleJump { get; private set; }
    public float FaceDir => faceDir;

    private Rigidbody2D rb;
    private float xInput;
    private bool faceRight = true;
    private float faceDir = 1f;
    private float xScale;
    private bool wasWall = false;
    private bool canDoubleJump;

    private PlayerAnimation anim;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        anim = GetComponent<PlayerAnimation>();
        xScale = transform.localScale.x;
        defaultGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (IsWinning)
            return;

        xInput = Input.GetAxisRaw("Horizontal");
        IsGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, layerGround);
        IsWall = Physics2D.Raycast(transform.position, Vector2.right * faceDir, wallCheckDistance, layerGround);

        DashControl();

        if (IsGrounded || IsWall)
        {
            canDoubleJump = true;
            canDashInAir = airDashCount;
        }


        CoyoteTimeCounter();
        JumpBufferCounter();
        HandleFastFallInput();

        Flip();
        HandleJumpInputs();
        StopJumpHoldOnRelease();
        HandleWallSlide();
    }

   

    private void FixedUpdate()
    {
        if (IsWinning || IsWallJumping || isKnockbacked || isDashing)
            return;

        if (!IsWall)
        {
            //rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
            float platformXVelocity = activePlatform != null ? activePlatform.GetVelocity().x : 0f;
            float platformYVelocity = activePlatform != null ? activePlatform.GetVelocity().y : 0f;

            rb.velocity = new Vector2((xInput * moveSpeed) + platformXVelocity, rb.velocity.y + platformYVelocity);

        }
        ApplyFastFall();
        ApplyJumpHold();
    }

    private void DashControl()
    {
        if (
            Input.GetKeyDown(KeyCode.LeftShift) 
            && canDash 
            && !isDashing 
            && !IsWinning 
            && !isKnockbacked 
            && !IsWall 
            && canDashInAir > 0 
            && AbilityManager.IsUnlocked(AbilityType.Dash)
            )
        {
            StartCoroutine(DashRoutine());
            canDashInAir -= 1;
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = dashGravityScale;
        rb.velocity = new Vector2(faceDir * dashSpeed, 0f);

        if (IsGrounded)
            SoundManager.Instance.PlaySound(groundDashSound);
        else
            SoundManager.Instance.PlaySound(midAirDashSound);

        SpawnDashEffect();

        // anim.PlayDashAnimation();

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void SpawnDashEffect()
    {
        if (IsGrounded)
        {
            if (groundDashEffectPrefab == null)
                return;
            if (dashEffectPoint == null)
                Instantiate(groundDashEffectPrefab, transform.position, Quaternion.identity);
            else
                Instantiate(groundDashEffectPrefab, dashEffectPoint.position, Quaternion.identity);
        }
        else
        {
            if (midAirDashEffectPrefab == null)
                return;
            if (dashEffectPoint == null)
                Instantiate(midAirDashEffectPrefab, transform.position, Quaternion.identity);
            else
                Instantiate(midAirDashEffectPrefab, dashEffectPoint.position, Quaternion.identity);
        }
    }


    private void JumpBufferCounter()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void CoyoteTimeCounter()
    {
        if (jumpCooldownTimer > 0)
            jumpCooldownTimer -= Time.deltaTime;
        if (IsGrounded && jumpCooldownTimer <= 0)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;
    }


    private void HandleJumpInputs()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            SoundManager.Instance.PlaySound(jumpSound);

            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
            jumpCooldownTimer = coyoteTime;

            jumpTimeCounter = maxJumpHoldTime; 
            jumpHeldActive = true;
        }

        //Wall Jump
        else if (jumpBufferCounter > 0f && IsWall)
        {
            SoundManager.Instance.PlaySound(jumpSound);
            faceRight = !faceRight;
            faceDir = faceRight ? 1f : -1f;
            transform.localScale = new Vector3(faceDir * xScale, transform.localScale.y, transform.localScale.z);
            rb.velocity = new Vector2(faceDir * 6f, jumpForce);

            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
            jumpCooldownTimer = coyoteTime;
            StartCoroutine(WallJumpingRoutine());
        }
        //Double Jump
        else if (jumpBufferCounter > 0f && canDoubleJump && AbilityManager.IsUnlocked(AbilityType.DoubleJump))
        {
            canDoubleJump = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            SoundManager.Instance.PlaySound(doubleJumpSound);

            jumpBufferCounter = 0f;

            anim.PlayDoubleJumpAnimation();

            //Hold Jump
            //jumpTimeCounter = maxJumpHoldTime;  
            //jumpHeldActive = true;
        }
    }

    private void HandleWallSlide()
    {
        IsWallSliding = IsWall && !IsGrounded && rb.velocity.y <= 0f;

        rb.drag = IsWallSliding ? 15f : 0f;

        if (IsWallSliding && !wasWall)
        {
            SoundManager.Instance.PlaySound(grabSound);
        }

        wasWall = IsWall;
    }

    private void Flip()
    {
        if (IsWallJumping)
            return;
        if (xInput == -1f && faceRight)
        { faceRight = false; faceDir = -1f; transform.localScale = new Vector3(-xScale, transform.localScale.y, transform.localScale.z); }
        else if (xInput == 1f && !faceRight)
        { faceRight = true; faceDir = 1f; transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z); }
    }

    private IEnumerator WallJumpingRoutine()
    {
        IsWallJumping = true;
        yield return new WaitForSeconds(wallJumpCooldown);
        IsWallJumping = false;
    }


    public void FreezePlayer()
    {
        IsWinning = true;
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Static;
        enabled = false;
    }

    public void ApplyKnockback(float force)
    {
        Vector2 knockbackForce = new Vector2(-FaceDir * force, 4f);
        if (IsWinning)
            return;

        StartCoroutine(KnockbackRoutine(knockbackForce));
    }

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockbacked = true;

        // اعمال نیرو (جهت مخالف حرکت قبلاً محاسبه و به عنوان پارامتر فرستاده می‌شود)
        rb.velocity = force;

        yield return new WaitForSeconds(knockbackDuration);

        isKnockbacked = false;
    }

    // متدهای کمکی برای پلتفرم‌ها
    public void SetActivePlatform(MovingPlatform platform)
    {
        activePlatform = platform;
    }

    public void ClearActivePlatform()
    {
        activePlatform = null;
    }

    // این ویژگی سرعت لحظه‌ای پلتفرمی که پلیر روی آن است را برمی‌گرداند
    public Vector2 GetActivePlatformVelocity()
    {
        if (activePlatform != null)
        {
            return activePlatform.GetVelocity();
        }
        return Vector2.zero;
    }

    // --- داخل Update() قرار بده ---
    void HandleFastFallInput()
    {
        if (!AbilityManager.IsUnlocked(AbilityType.FastFall))
            return;

        // فقط وقتی در هواست؛ روی زمین S بی‌اثر است
        fastFallPressed = Input.GetKeyDown(KeyCode.S) && !IsGrounded;
        fastFallHeld = Input.GetKey(KeyCode.S) && !IsGrounded;
    }

    // --- داخل FixedUpdate() قرار بده ---
    void ApplyFastFall()
    {
        // ضربه اولیه: یک پالس فوری رو به پایین
        if (fastFallPressed)
        {
            rb.AddForce(Vector2.down * fastFallImpulse, ForceMode2D.Impulse);
            fastFallPressed = false;
        }

        // شتاب ادامه‌دار: تا وقتی S نگه داشته شده، هر فریم فیزیک نیرو بگیر
        if (fastFallHeld && !onlyOnPress)
        {
            rb.AddForce(Vector2.down * fastFallAccel, ForceMode2D.Force);

            // سقف سرعت: جلوگیری از سرعت غیرقابل کنترل (مقادیر منفی‌تر = سریع‌تر به پایین)
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
    }

    // شتاب ادامه‌دار پرش: تا زمان maxJumpHoldTime یا تا رها شدن کلید
    private void ApplyJumpHold()
    {
        if (!jumpHeldActive 
            || isDashing 
            || isKnockbacked 
            || IsWinning 
            || !AbilityManager.IsUnlocked(AbilityType.HoldJump)
            )
            return;

        // زدن S یعنی پلیر می‌خواهد سقوط کند → شتاب پرش را قطع کن
        if (Input.GetKey(KeyCode.S))
        {
            jumpHeldActive = false;
            return;
        }

        bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W);

        // توقف: کلید رها شده / زمان تمام / دیگر رو به بالا نیستیم / روی زمینیم
        if (!jumpKeyHeld || jumpTimeCounter <= 0f || rb.velocity.y <= 0f || (IsGrounded && rb.velocity.y <= 0.1f))
        {
            jumpHeldActive = false;
            return;
        }

        jumpTimeCounter -= Time.fixedDeltaTime;

        // اعمال شتاب رو به بالا (مستقل از جرم — حس یکنواخت‌تر)
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + jumpHoldAccel * Time.fixedDeltaTime);

        // سقف سرعت (ایمنی در برابر شتاب بیش از حد)
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, maxRiseSpeed));
    }

    private void StopJumpHoldOnRelease()
    {
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W))
        {
            jumpHeldActive = false;
            jumpTimeCounter = 0f;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * faceDir), transform.position.y));
    }
}
