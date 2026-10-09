using System.Collections;
using UnityEngine;

public enum BeeState
{
    Wander,
    DiveTelegraph,
    DiveAttack,
    BombingPositioning,
    BombingTelegraph,
    BombingAttack,
    Stunned,
    Dead
}

[RequireComponent(typeof(Rigidbody2D), typeof(BeeHealth), typeof(Collider2D))]
public class BeeController : MonoBehaviour
{
    [Header("Movement & Wander Settings")]
    [Tooltip("Movement speed while wandering around the spawn point")]
    [SerializeField] private float wanderSpeed = 3f;

    [Tooltip("Maximum radius from the starting position for picking wander destinations")]
    [SerializeField] private float wanderRadius = 3f;

    [Tooltip("Time spent wandering before triggering the next attack phase")]
    [SerializeField] private float stateChangeInterval = 3.5f;

    [Header("Dive Attack Settings")]
    [Tooltip("Duration of the telegraph warning shake before performing a dive attack")]
    [SerializeField] private float diveTelegraphTime = 0.8f;

    [Tooltip("Movement speed during the fast dive attack")]
    [SerializeField] private float diveSpeed = 14f;

    [Tooltip("Additional overshoot distance past the player position during dive attack")]
    [SerializeField] private float diveOvershootDistance = 2f;

    [Tooltip("Short pause after the dive ends before returning to wander")]
    [SerializeField] private float diveRecoveryTime = 0.2f;

    [Header("Bombing Attack Settings")]
    [Tooltip("Target ceiling height offset relative to the starting position for bombing runs")]
    [SerializeField] private float arenaCeilingY = 4f;

    [Tooltip("Horizontal tracking speed while aligning above the player during bombing")]
    [SerializeField] private float bombingMoveSpeed = 5f;

    [Tooltip("Maximum time spent aligning horizontally above the player before each shot")]
    [SerializeField] private float maxAlignTime = 0.6f;

    [Tooltip("Bullet prefab to spawn during bombing attacks")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Spawn point transform from which bullets are fired")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Total number of bullets fired during each bombing run")]
    [SerializeField] private int bombsCount = 3;

    [Tooltip("Delay in seconds after the attack animation finishes before the next shot starts")]
    [SerializeField] private float timeBetweenBombs = 0.4f;

    [Header("Attack Animation Settings")]
    [Tooltip("Exact name of the attack state inside the Animator Controller")]
    [SerializeField] private string attackStateName = "Bee_Attack";

    [Tooltip("Total length of the attack clip in seconds (7 frames at 15 samples per second ≈ 0.47s)")]
    [SerializeField] private float attackClipLength = 0.47f;

    [Tooltip("If enabled, the bullet is spawned by the Animation Event placed on frame 5. If disabled, the script spawns it itself")]
    [SerializeField] private bool shootFromAnimationEvent = true;

    [Tooltip("Time from the clip start until frame 5 (5 / 15 ≈ 0.33s). Used only when the animation event is disabled")]
    [SerializeField] private float frame5Time = 0.33f;

    [Header("Stun Settings")]
    [Tooltip("Duration the boss remains stunned after taking damage")]
    [SerializeField] private float stunDuration = 1.2f;

    [Header("Detection / Layers")]
    [Tooltip("Layer mask assigned to the player character")]
    [SerializeField] private LayerMask playerLayer;

    public BeeState CurrentState { get; private set; } = BeeState.Wander;

    private Rigidbody2D rb;
    private Animator anim;
    private BeeHealth health;
    private Transform player;
    private PlayerController playerController;

    private Vector2 startPosition;
    private Vector2 wanderTarget;
    private Vector2 diveTarget;
    private float stateTimer;
    private bool isFacingRight = false;
    private float originalScaleX;

    // جلوگیری از شلیک دوباره در یک سیکل انیمیشن
    private bool shotFiredThisCycle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<BeeHealth>();
        rb.gravityScale = 0f; // پرواز بدون گرانش
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Start()
    {
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }

        PickNewWanderTarget();
    }

    private void Update()
    {
        if(CurrentState == BeeState.Dead)
            return;

        // چرخش جهت نگاه زنبور به سمت بازیکن در حالات غیر حمله
        if(CurrentState == BeeState.Wander ||
            CurrentState == BeeState.DiveTelegraph ||
            CurrentState == BeeState.BombingPositioning)
        {
            LookAtPlayer();
        }

        if(CurrentState == BeeState.Wander)
        {
            HandleWander();
        }
    }

    private void LookAtPlayer()
    {
        if(player == null)
            return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if(shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            transform.localScale = new Vector3(
                isFacingRight ? -originalScaleX : originalScaleX,
                transform.localScale.y,
                transform.localScale.z);
        }
    }

    private void HandleWander()
    {
        transform.position = Vector2.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);

        if(Vector2.Distance(transform.position, wanderTarget) < 0.2f)
        {
            PickNewWanderTarget();
        }

        stateTimer += Time.deltaTime;
        if(stateTimer >= stateChangeInterval)
        {
            stateTimer = 0f;
            DecideNextAttack();
        }
    }

    private void PickNewWanderTarget()
    {
        wanderTarget = startPosition + (Random.insideUnitCircle * wanderRadius);
    }

    private void DecideNextAttack()
    {
        if(player == null)
            return;

        // انتخاب رندوم بین شیرجه و بمباران
        if(Random.value > 0.5f)
        {
            StartCoroutine(DiveAttackRoutine());
        }
        else
        {
            StartCoroutine(BombingRoutine());
        }
    }

    // ---------------------------------------------------------------
    //  حمله مستقیم (Dive)  →  انیمیشن Bee_Attack در این حالت پخش نمی‌شود
    // ---------------------------------------------------------------
    private IEnumerator DiveAttackRoutine()
    {
        CurrentState = BeeState.DiveTelegraph;
        rb.velocity = Vector2.zero;

        // فاز تلگراف (لرزش و آماده‌سازی)
        Vector2 originalPos = transform.position;
        float elapsed = 0f;
        while(elapsed < diveTelegraphTime)
        {
            transform.position = originalPos + (Random.insideUnitCircle * 0.1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        // هدف‌گیری و اجرای شیرجه
        CurrentState = BeeState.DiveAttack;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        diveTarget = (Vector2)player.position + (direction * diveOvershootDistance);

        while(Vector2.Distance(transform.position, diveTarget) > 0.3f)
        {
            transform.position = Vector2.MoveTowards(transform.position, diveTarget, diveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(diveRecoveryTime);
        CurrentState = BeeState.Wander;
    }

    // ---------------------------------------------------------------
    //  حمله بمباران (Bombing)  →  انیمیشن Bee_Attack + شلیک روی فریم ۵
    // ---------------------------------------------------------------
    private IEnumerator BombingRoutine()
    {
        CurrentState = BeeState.BombingPositioning;
        rb.velocity = Vector2.zero;

        // ۱) صعود به سقف زمین
        float ceilingY = startPosition.y + arenaCeilingY;
        while(Mathf.Abs(transform.position.y - ceilingY) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                new Vector2(transform.position.x, ceilingY),
                wanderSpeed * Time.deltaTime);
            yield return null;
        }

        // ۲) فاز هشدار قبل از رگبار
        CurrentState = BeeState.BombingTelegraph;
        yield return new WaitForSeconds(0.4f);

        // ۳) رگبار بمباران
        CurrentState = BeeState.BombingAttack;

        for(int i = 0; i < bombsCount; i++)
        {
            // اگر در میانهٔ کار زنبور گیج یا کشته شد، رگبار متوقف شود
            if(CurrentState != BeeState.BombingAttack)
                yield break;

            // هم‌راستا شدن افقی با بازیکن (قبل از شروع انیمیشن)
            yield return StartCoroutine(AlignAbovePlayerRoutine());

            if(CurrentState != BeeState.BombingAttack)
                yield break;

            // ۴) پخش انیمیشن حمله از فریم اول
            shotFiredThisCycle = false;
            PlayAttackAnimation();

            if(shootFromAnimationEvent)
            {
                // گلوله در فریم ۵ توسط Animation Event شلیک می‌شود؛
                // پس فقط تا پایان کامل کلیپ صبر می‌کنیم.
                yield return new WaitForSeconds(attackClipLength);
            }
            else
            {
                // حالت پشتیبان (بدون Animation Event)
                yield return new WaitForSeconds(frame5Time);

                if(CurrentState == BeeState.BombingAttack && !shotFiredThisCycle)
                {
                    shotFiredThisCycle = true;
                    SpawnBullet();
                }

                yield return new WaitForSeconds(Mathf.Max(0f, attackClipLength - frame5Time));
            }

            // فاصله بین دو شلیک
            if(timeBetweenBombs > 0f)
            {
                yield return new WaitForSeconds(timeBetweenBombs);
            }
        }

        yield return new WaitForSeconds(0.3f);
        shotFiredThisCycle = false;
        CurrentState = BeeState.Wander;
    }

    private IEnumerator AlignAbovePlayerRoutine()
    {
        if(player == null)
            yield break;

        float targetX = player.position.x;
        float moveTimer = 0f;

        while(Mathf.Abs(transform.position.x - targetX) > 0.3f && moveTimer < maxAlignTime)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                new Vector2(targetX, transform.position.y),
                bombingMoveSpeed * Time.deltaTime);

            moveTimer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// انیمیشن حمله را از فریم اول و به صورت قطعی پخش می‌کند.
    /// استفاده از Play به جای SetTrigger باعث می‌شود انیمیشن حتی اگر
    /// الان داخل همان State باشیم یا ترنزیشن شرط/Exit Time داشته باشد، ری‌استارت شود.
    /// </summary>
    private void PlayAttackAnimation()
    {
        if(anim == null)
            return;

        // پاک کردن تریگر باقی‌مانده تا بعداً انیمیشن اضافی پخش نشود
        anim.ResetTrigger("PlayAttack");
        anim.Play(attackStateName, 0, 0f);
    }

    private void SpawnBullet()
    {
        if(bulletPrefab == null)
            return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// این متد توسط Animation Event روی فریم ۵ کلیپ Bee_Attack فراخوانی می‌شود.
    /// نام این متد باید عیناً در فیلد Function ایونت نوشته شود.
    /// </summary>
    public void OnBombShootFrame()
    {
        // فقط در فاز بمباران و فقط یک بار در هر سیکل شلیک شود
        if(CurrentState != BeeState.BombingAttack)
            return;

        if(shotFiredThisCycle)
            return;

        shotFiredThisCycle = true;
        SpawnBullet();
    }

    public void OnTakeHit()
    {
        if(CurrentState == BeeState.Dead)
            return;

        StopAllCoroutines();
        shotFiredThisCycle = false;
        StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        CurrentState = BeeState.Stunned;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(stunDuration);
        CurrentState = BeeState.Wander;
    }

    public void OnDeath()
    {
        CurrentState = BeeState.Dead;
        StopAllCoroutines();
        shotFiredThisCycle = false;

        if(anim != null)
        {
            anim.ResetTrigger("PlayAttack");
            anim.SetBool("IsDead", true);
        }

        rb.velocity = Vector2.zero;
        rb.gravityScale = 1.5f; // سقوط زنبور بعد از مرگ

        Collider2D col = GetComponent<Collider2D>();
        if(col != null)
            col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider2D collision)
    {
        if(CurrentState == BeeState.Dead)
            return;

        PlayerHealth pHealth = collision.GetComponentInParent<PlayerHealth>();
        PlayerController pController = collision.GetComponentInParent<PlayerController>();

        if(pHealth != null && pController != null)
        {
            // قانون Dash-Strike: اگر بازیکن در حال Dash بود و زنبور آسیب‌پذیر بود (Wander یا Stunned)
            if(pController.IsDashing && (CurrentState == BeeState.Wander || CurrentState == BeeState.Stunned))
            {
                health.TakeDamage(1);
            }
            else
            {
                // در غیر این صورت بازیکن آسیب می‌بیند
                pHealth.TakeDamage(1);
            }
        }
    }
}
