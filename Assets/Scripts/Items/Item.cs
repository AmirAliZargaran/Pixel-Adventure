using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Base Item Settings")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private AudioClip collectSound;

    private bool isCollected = false; // برای جلوگیری از دوبار اجرا شدن در یک فریم

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected)
            return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            isCollected = true;

            // ۱. اعمال ویژگی خاص آیتم (توسط کلاس‌های فرزند پیاده‌سازی می‌شود)
            OnCollect(player);

            // ۲. افکت‌های صوتی و تصویری عمومی
            if (collectSound != null)
            {
                SoundManager.Instance.PlaySound(collectSound);
            }

            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
            }

            // ۳. نابودی آبجکت
            Destroy(gameObject);
        }
    }

    // این متد در فرزندان override می‌شود
    protected virtual void OnCollect(PlayerController player)
    {
        // رفتار پیش‌فرض (می‌تواند خالی باشد)
    }
}
