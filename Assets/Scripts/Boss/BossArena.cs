using System.Collections;
using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject boss;
    [SerializeField] private GameObject[] arenaDoors;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip doorCloseSound;

    [Header("End Game Delay")]
    [Tooltip("چند ثانیه پس از مرگ باس، پنل پایان مرحله نمایش داده شود")]
    [SerializeField] private float winDelay = 2.5f;

    private bool fightStarted = false;
    private BeeHealth beeHealth;
    private GameManager gameManager;

    private void Awake()
    {
        if(boss != null)
            beeHealth = boss.GetComponent<BeeHealth>();

        gameManager = FindObjectOfType<GameManager>();

        if(beeHealth != null)
            beeHealth.OnDeath += HandleBossDeath;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(fightStarted || !collision.CompareTag("Player"))
            return;

        StartFight();
    }

    private void StartFight()
    {
        fightStarted = true;

        // ۱. بستن درها (فعال کردن کولایدرها و اسپرایت‌ها)
        if(arenaDoors != null)
        {
            foreach(var door in arenaDoors)
            {
                if(door != null)
                    door.SetActive(true);
            }
        }

        // ۲. پخش افکت صدای بسته شدن در با SoundManager
        if(doorCloseSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(doorCloseSound);
        }

        // ۳. فعال کردن باس
        if(boss != null)
        {
            boss.SetActive(true);
            var controller = boss.GetComponent<BeeController>();
            if(controller != null)
                controller.ActivateBoss();
        }

        // ۴. شروع موزیک با AudioSource اختصاصی
        if(musicSource != null && bossMusic != null)
        {
            musicSource.clip = bossMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    private void HandleBossDeath()
    {
        // شروع کوروتین تاخیر پس از مرگ باس
        StartCoroutine(WinWithDelayRoutine());
    }

    private IEnumerator WinWithDelayRoutine()
    {
        // چند ثانیه صبر می‌کند تا انیمیشن مرگ و پارتیکل‌ها دیده شوند
        yield return new WaitForSeconds(winDelay);

        if(gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if(gameManager != null)
        {
            gameManager.WinFromBoss();
        }
        else
        {
            Debug.LogError("GameManager در صحنه یافت نشد تا پایان مرحله با مرگ باس اجرا شود.");
        }
    }

    private void OnDestroy()
    {
        if(beeHealth != null)
            beeHealth.OnDeath -= HandleBossDeath;
    }
}
