

using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private int invincibleLoopCount= 7;
    [SerializeField] private GameObject dieEffect;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip dieSound;

    public int MaxHealth => maxHealth;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    public int CurrentHealth { get; private set; }
    public bool IsInvincible { get; private set; }

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private PlayerAnimation anim;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<PlayerAnimation>();
    }

    private void Start() => OnHealthChanged?.Invoke(CurrentHealth);

    public void TakeDamage(int damage)
    {
        if (CurrentHealth <= 0 || IsInvincible || playerController.IsWinning)
            return;

        CurrentHealth -= damage;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        { Die(); return; }

        SoundManager.Instance.PlaySound(hitSound);
        playerController.ApplyKnockback(4f);
        anim?.PlayHitAnimation();
        StartCoroutine(InvincibilityRoutine());
    }

    public void Die()
    {
        SoundManager.Instance.PlaySound(dieSound);
        if (dieEffect != null)
            Instantiate(dieEffect, transform.position, Quaternion.identity);
        OnDeath?.Invoke();
        gameObject.SetActive(false);
        RespawnManager.Instance.HandlePlayerDeath();
    }

    public void ActivePlayer()
    {
        gameObject.SetActive(true);
    }

    public void Heal(int healAmount)
    {
        CurrentHealth += healAmount;
    }

    public void Reset()
    {
        CurrentHealth = maxHealth;
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        for (int i = invincibleLoopCount; i > 0; i--)
        {
            SetSpriteAlpha(0.1f * i);
            yield return new WaitForSeconds(0.05f * i);
            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(0.05f * i);

        }
        IsInvincible = false;
    }

    private void SetSpriteAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            TakeDamage(1);
    }
}
