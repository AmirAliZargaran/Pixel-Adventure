using System.Collections;
using System.Linq;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager _instance;
    public static RespawnManager Instance => _instance;

    [Header("Player References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Settings")]
    [Tooltip("مدت زمان تاخیر تا ریسپاون (ثانیه)")]
    [SerializeField] private float respawnDelay = 0.5f;

    private Coroutine respawnRoutine;
    private bool isRespawning;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        ResolvePlayerReferences();
    }

    private void Start()
    {
        ResolvePlayerReferences();
    }

    public void HandlePlayerDeath()
    {
        if(isRespawning)
            return;

        isRespawning = true;

        if(respawnRoutine != null)
            StopCoroutine(respawnRoutine);

        respawnRoutine = StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if(!ResolvePlayerReferences())
        {
            Debug.LogError(
                "RespawnManager: Player، Rigidbody2D یا PlayerHealth پیدا نشد.",
                this
            );

            isRespawning = false;
            respawnRoutine = null;
            yield break;
        }

        Vector2 respawnPoint = SaveManager.Instance != null
            ? SaveManager.Instance.GetRespawnPoint()
            : (Vector2)playerTransform.position;

        // پلیر هنوز غیرفعال است؛ ابتدا موقعیت و فیزیک را ریست کن.
        playerRb.velocity = Vector2.zero;
        playerRb.angularVelocity = 0f;
        playerRb.position = respawnPoint;
        playerTransform.position = respawnPoint;

        Physics2D.SyncTransforms();

        // سپس جان را برگردان و در آخر پلیر را فعال کن.
        playerHealth.Reset();
        playerHealth.ActivePlayer();

        foreach(IRespawnable respawnable in FindObjectsOfType<MonoBehaviour>()
                     .OfType<IRespawnable>())
        {
            respawnable.ResetForRespawn();
        }

        respawnRoutine = null;
        isRespawning = false;
    }

    private bool ResolvePlayerReferences()
    {
        // اگر رفرنس‌ها سالم و معتبرند، نیازی به جستجو نیست
        if(playerTransform != null &&
           playerRb != null &&
           playerHealth != null)
        {
            return true;
        }

        // ۱. تلاش برای پیدا کردن PlayerHealth حتی اگر پلیر غیرفعال (Inactive) باشد
        PlayerHealth health = Resources.FindObjectsOfTypeAll<PlayerHealth>()
            .FirstOrDefault(h => h.gameObject.scene.isLoaded && h.CompareTag("Player"));

        // اگر با متد بالا پیدا نشد، از روش عمومی با includeInactive استفاده می‌کنیم
        if(health == null)
        {
            health = FindObjectsOfType<PlayerHealth>(true)
                .FirstOrDefault(h => h.CompareTag("Player"));
        }

        if(health != null)
        {
            playerHealth = health;
            playerTransform = health.transform;
            playerRb = health.GetComponent<Rigidbody2D>();

            return playerTransform != null &&
                   playerRb != null &&
                   playerHealth != null;
        }

        // ۲. پشتیبان: اگر پلیر تگ پلیر داشت و هنوز فعال بود
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerRb = playerObject.GetComponent<Rigidbody2D>();
            playerHealth = playerObject.GetComponent<PlayerHealth>();

            return playerTransform != null &&
                   playerRb != null &&
                   playerHealth != null;
        }

        return false;
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }
}
