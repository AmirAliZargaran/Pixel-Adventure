using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRespawnable
{
    void ResetForRespawn();
}

public class Enemy : MonoBehaviour, IRespawnable
{
    private Rigidbody2D rb;
    private Collider2D col;

    [SerializeField] private float Speed=2.5f;
    [SerializeField] private float GroundCheckDistance=1f;
    [SerializeField] private float WallCheckDistance=1f;
    [SerializeField] private Transform CheckNoGroundTransform;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private LayerMask EnemyLayer;


    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private AudioClip DeathSound;


    public bool isGrounded;
    public bool checkNoGround;
    public bool isWall;
    public bool isEnemy;


    private bool faceRight = false;
    private float faceDir=-1;
    private float xScale;


    void Start()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        xScale = transform.localScale.x;
    }

    void Update()
    {

        rb.velocity=new Vector2(faceDir*Speed,rb.velocity.y);

        //flip
        if (isGrounded)
        {
            if (isWall || !checkNoGround || isEnemy)
            {
                faceRight = !faceRight;
            }

        }

        if (faceRight)
        {
            faceDir = 1;
            transform.localScale = new Vector3(-xScale, transform.localScale.y, transform.localScale.z);
        }else
        {
            faceDir = -1;
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, GroundCheckDistance, GroundLayer);
        checkNoGround = Physics2D.Raycast(CheckNoGroundTransform.transform.position, Vector2.down, GroundCheckDistance, GroundLayer);
        isWall = Physics2D.Raycast(transform.position, Vector2.right * faceDir, WallCheckDistance, GroundLayer);

        float offset = col.bounds.extents.x + 0.05f;
        Vector2 origin = (Vector2)transform.position + Vector2.right * faceDir * offset;

        isEnemy = Physics2D.Raycast(origin, Vector2.right * faceDir, WallCheckDistance, EnemyLayer);
    }


    public void Die()
    {
        // ۱. اسپان کردن پریفب انیمیشن مرگ در موقعیت فعلی انمی
        if (deathEffectPrefab != null)
        {
            // حفظ جهت چرخش انمی (Flip) روی افکت مرگ
            if (deathEffectPrefab != null)
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        SoundManager.Instance.PlaySound(DeathSound);
        // ۲. نابود کردن آنی انمی
        Destroy(gameObject);
    }

    public void DestroyEnemy()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
    private void OnDrawGizmos()
    {
        if (col == null) return;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - GroundCheckDistance));
        Gizmos.DrawLine(CheckNoGroundTransform.position, new Vector2(CheckNoGroundTransform.position.x, CheckNoGroundTransform.position.y - GroundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (WallCheckDistance * faceDir), transform.position.y));
    }

    public void ResetForRespawn()
    {
        gameObject.SetActive(true);        // دوباره زنده
    }
}
