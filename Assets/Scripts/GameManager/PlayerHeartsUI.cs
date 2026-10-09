using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeartsUI : MonoBehaviour
{
    [Header("Heart Prefab & Container")]
    [SerializeField] private GameObject heartPrefab; // پریفب قلب که Image دارد

    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeart;   // اسپرایت قلب پر
    [SerializeField] private Sprite halfHeart;   // اسپرایت قلب نیمه‌پر
    [SerializeField] private Sprite emptyHeart;  // اسپرایت قلب خالی

    private List<Image> heartImages = new List<Image>();
    private PlayerHealth playerHealth;

    private void Start()
    {
        // پیدا کردن اسکریپت سلامتی پلیر در صحنه
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            // ثبت‌نام در رویداد تغییر جان (در صورتی که در PlayerHealth رویداد داری)
            // در غیر این صورت، در Update یا به صورت مستقیم آن را آپدیت می‌کنیم.
            InitializeHearts();
        }
    }

    private void Update()
    {
        // برای سادگی فعلاً در Update وضعیت را همگام نگه می‌داریم.
        // بعداً می‌توانید این را به Event متصل کنید تا بهینه تر شود.
        if (playerHealth != null)
        {
            UpdateHearts(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    // ایجاد قلب‌ها بر اساس حداکثر جان بازیکن در شروع بازی
    public void InitializeHearts()
    {
        // پاک کردن قلب‌های قدیمی احتمالی
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        // از آنجا که هر قلب نشان‌دهنده ۲ واحد جان است (پر = ۲، نیمه = ۱، خالی = ۰)
        // تعداد کل قلب‌های مورد نیاز برابر است با: MaxHealth / 2
        int totalHearts = playerHealth.MaxHealth / 2;

        for (int i = 0; i < totalHearts; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, transform);
            Image heartImg = newHeart.GetComponent<Image>();
            if (heartImg != null)
            {
                heartImg.preserveAspect = true;
                heartImages.Add(heartImg);
            }
        }
    }

    // آپدیت کردن اسپرایت هر قلب با توجه به جان فعلی
    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        // در این فرمول هر ۱ جان معادل نصف قلب است.
        // مثلاً اگر جان ۵ باشد:
        // قلب اول: پر (۲ جان) -> باقی‌مانده ۳
        // قلب دوم: پر (۲ جان) -> باقی‌مانده ۱
        // قلب سوم: نیمه (۱ جان) -> باقی‌مانده ۰
        // قلب‌های بعدی: خالی (۰ جان)

        int tempHealth = currentHealth;

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (tempHealth >= 2)
            {
                heartImages[i].sprite = fullHeart;
                tempHealth -= 2;
            }
            else if (tempHealth == 1)
            {
                heartImages[i].sprite = halfHeart;
                tempHealth -= 1;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }
}
