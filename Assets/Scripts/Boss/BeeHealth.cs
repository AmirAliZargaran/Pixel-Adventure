using System;
using System.Collections;
using UnityEngine;

public class BeeHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    public int MaxHealth => maxHealth;
    public int CurrentHealth
    {
        get; private set;
    }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    private Animator anim;
    private BeeController controller;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        anim = GetComponent<Animator>();
        controller = GetComponent<BeeController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if(IsDead)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth);

        if(hitSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(hitSound);

        if(hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if(CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            if(anim != null)
                anim.SetTrigger("PlayHit");
            StartCoroutine(FlashWhiteRoutine());
            if(controller != null)
                controller.OnTakeHit();
        }
    }

    private void Die()
    {
        if(anim != null)
        {
            anim.SetBool("IsDead", true);
            anim.Play("Bee_Dead");
        }

        if(deathSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(deathSound);

        if(deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        OnDeath?.Invoke();
        if(controller != null)
            controller.OnDeath();
    }

    private IEnumerator FlashWhiteRoutine()
    {
        if(spriteRenderer == null)
            yield break;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}
