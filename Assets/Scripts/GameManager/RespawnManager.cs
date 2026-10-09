using System.Collections;
using System.Linq;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager _instance;
    public static RespawnManager Instance => _instance;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("مدت زمان تاخیر تا ریسپاون (ثانیه)")]
    [SerializeField] private float respawnDelay = 0.5f;

    private void Awake() => _instance = this;

    /// صدا بزن وقتی جان پلیر تمام شد
    public void HandlePlayerDeath()
    {
        // جلوگیری از اجرای هم‌زمان چند ریسپاون
        StopAllCoroutines();
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // مکث تا انیمیشن مرگ / افکت نمایش داده شود
        yield return new WaitForSeconds(respawnDelay);

        // 1) برگرد به آخرین چک‌پوینت
        playerTransform.position = SaveManager.Instance.GetRespawnPoint();
        playerRb.velocity = Vector2.zero;   // حرکت افقی/عمودی صفر

        // 2) جان پلیر برگردد
        playerHealth?.Reset();
        playerHealth?.ActivePlayer();

        // 3) دشمن‌ها را زنده کن (آیتم‌ها به‌خاطر SaveManager دست‌نخورده می‌مانند)
        foreach (var respawnable in FindObjectsOfType<MonoBehaviour>().OfType<IRespawnable>())
            respawnable.ResetForRespawn();
    }
}
