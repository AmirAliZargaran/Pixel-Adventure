using UnityEngine;

public class BeeBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Downward movement speed of the bullet")]
    [SerializeField] private float bulletSpeed = 6f;

    [Tooltip("Damage dealt to the player on hit")]
    [SerializeField] private int bulletDamage = 1;

    [Tooltip("Auto destroy delay if the bullet never hits anything")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Effects")]
    [Tooltip("Particle / sprite prefab spawned at the impact point")]
    [SerializeField] private GameObject bulletHitEffectPrefab;

    [Tooltip("Sound played on impact")]
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D rb;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // شلیک مستقیم رو به پایین
        if(rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.down * bulletSpeed;
        }

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // پشتیبان: اگر پریفب اصلاً Rigidbody2D نداشت
        if(rb == null)
        {
            transform.Translate(Vector2.down * bulletSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D collision)
    {
        if(hasHit || collision == null)
            return;

        // به خود زنبور آسیب نزند
        if(collision.CompareTag("Enemy"))
            return;

        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        if(playerHealth != null)
        {
            playerHealth.TakeDamage(bulletDamage);
            Hit();
            return;
        }

        // برخورد به زمین یا موانع
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Hit();
        }
    }

    private void Hit()
    {
        if(hasHit)
            return;

        hasHit = true;

        if(bulletHitEffectPrefab != null)
        {
            Instantiate(bulletHitEffectPrefab, transform.position, Quaternion.identity);
        }

        if(hitSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(hitSound);
        }

        Destroy(gameObject);
    }
}
