using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 5f; // نیروی پرتاب پلیر به بالا
    [Range(0f, 1f)]
    [SerializeField] private float stompThreshold = 0.7f; // دقت تشخیص پرش روی سر (هرچه به ۱ نزدیک‌تر، باید دقیق‌تر از بالا فرود بیاید)

    [SerializeField] private AudioClip hitSound;

    private Enemy enemyParent;

    private void Start()
    {
        enemyParent = GetComponentInParent<Enemy>();
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    // نکته: مطمئن شو کالیژن‌ها Trigger نیستند تا Collision2D کار کند.
    //    // اگر کامپوننت این هیت‌باکس IsTrigger است، متد OnTriggerEnter2D پایین را استفاده کن.
    //    HandleCollision(collision.collider, collision);
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // در صورتی که هیت‌باکس شما Trigger است
        HandleTrigger(collision);
    }

    private void HandleTrigger(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        Rigidbody2D playerRb = collision.GetComponentInParent<Rigidbody2D>();
        if (playerRb != null)
        {
            // ۱. بررسی اینکه پلیر در حال سقوط است (سرعت Y منفی)
            // ۲. بررسی اینکه مرکز پلیر (به همراه آفست کلایدر) بالاتر از بالای سر قارچ است
            float playerBottom = collision.bounds.min.y;
            float enemyTop = GetComponent<Collider2D>().bounds.max.y;

            // اگر کف پای پلیر بالاتر از ۷۰ درصد بالایی کلایدر انمی باشد و سرعت سقوط داشته باشد
            if (playerRb.velocity.y <= 0.1f && playerBottom >= enemyTop - 0.2f)
            {
                ExecuteStomp(playerRb);
                SoundManager.Instance.PlaySound(hitSound);
                return;
            }
        }

        // در غیر این صورت، آسیب به پلیر
        playerHealth.TakeDamage(1);
    }

    private void HandleCollision(Collider2D collision, Collision2D collisionInfo)
    {
        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        Rigidbody2D playerRb = collision.GetComponentInParent<Rigidbody2D>();
        if (playerRb != null)
        {
            // بررسی نقاط برخورد فیزیکی
            foreach (ContactPoint2D contact in collisionInfo.contacts)
            {
                // اگر نرمال برخورد رو به بالا باشد (یعنی ضربه از سقف انمی وارد شده)
                if (contact.normal.y < -stompThreshold)
                {
                    ExecuteStomp(playerRb);
                    return;
                }
            }
        }

        playerHealth.TakeDamage(1);
    }

    private void ExecuteStomp(Rigidbody2D playerRb)
    {
        // پرتاب پلیر به بالا
        playerRb.velocity = new Vector2(playerRb.velocity.x, bounceForce);

        // نابود کردن انمی و اسپان افکت مرگ
        if (enemyParent != null)
        {
            enemyParent.Die();
        }
    }
}
