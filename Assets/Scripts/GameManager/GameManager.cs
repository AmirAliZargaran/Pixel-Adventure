using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI GameObjects")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject musicObject;
    [SerializeField] private GameObject[] healthHearts;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    [Header("Maximum Score")]
    [Tooltip("روشن: سقف امتیاز از مجموع ارزش سیب‌های موجود در شروع مرحله محاسبه می‌شود.")]
    [SerializeField] private bool calculateMaxScoreFromApples = true;

    [Min(1)]
    [Tooltip("وقتی گزینه محاسبه از سیب‌ها خاموش باشد، این عدد سقف امتیاز است.")]
    [SerializeField] private int inspectorMaxScore = 100;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayerHealth playerHealth;
    private PlayerController playerController;
    private GameObject endGoal;

    private bool gameEnded;
    private bool goalFoundAtStart;
    private int score;
    private int maximumScore;
    private int initializedSceneHandle = -1;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // برای زمانی که بازی مستقیماً از صحنهٔ مرحله اجرا می‌شود.
        InitializeScene(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeScene(scene);
    }

    private void InitializeScene(Scene scene)
    {
        // ممکن است Start و sceneLoaded هر دو برای یک صحنه اجرا شوند.
        if(initializedSceneHandle == scene.handle)
            return;

        initializedSceneHandle = scene.handle;

        UnsubscribeFromPlayer();

        gameEnded = false;
        goalFoundAtStart = false;
        endGoal = null;
        playerController = null;
        score = 0;
        maximumScore = 0;

        if(winPanel != null)
            winPanel.SetActive(false);

        UpdateScoreUI();

        if(scene.name == mainMenuSceneName)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if(playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            playerController = playerObject.GetComponent<PlayerController>();

            if(playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                playerHealth.OnDeath += HandlePlayerDeath;
            }
        }

        endGoal = GameObject.FindGameObjectWithTag("End");
        goalFoundAtStart = endGoal != null;

        if(!goalFoundAtStart)
            Debug.LogWarning(
                "Goal با Tag برابر End در این مرحله پیدا نشد."
            );

        maximumScore = calculateMaxScoreFromApples
            ? CalculateMaximumAppleScore(scene)
            : Mathf.Max(1, inspectorMaxScore);

        if(musicObject != null)
            musicObject.SetActive(true);
    }

    private int CalculateMaximumAppleScore(Scene scene)
    {
        int total = 0;

#if UNITY_2022_2_OR_NEWER
        Apple[] apples = FindObjectsByType<Apple>(FindObjectsSortMode.None);
#else
        Apple[] apples = FindObjectsOfType<Apple>();
#endif

        foreach(Apple apple in apples)
        {
            // فقط سیب‌های همین مرحله؛ نه آبجکت‌های CoreSystems.
            if(apple.gameObject.scene.handle == scene.handle)
                total += Mathf.Max(0, apple.ScoreValue);
        }

        return total;
    }

    private void UnsubscribeFromPlayer()
    {
        if(playerHealth == null)
            return;

        playerHealth.OnHealthChanged -= UpdateHealthUI;
        playerHealth.OnDeath -= HandlePlayerDeath;
        playerHealth = null;
    }

    private void Update()
    {
        if(!gameEnded && goalFoundAtStart && endGoal == null)
            WinGame();
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if(healthHearts == null)
            return;

        for(int i = 0; i < healthHearts.Length; i++)
        {
            if(healthHearts[i] != null)
                healthHearts[i].SetActive(currentHealth > i);
        }
    }

    public void AddScore(int amount)
    {
        if(gameEnded)
            return;

        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if(scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void HandlePlayerDeath()
    {
        // منطق فعلی مرگ بازیکن را تغییر نمی‌دهیم.
    }

    public void WinFromBoss()
    {
        if(gameEnded)
            return;

        WinGame();
    }


    private void WinGame()
    {
        if(gameEnded)
            return;
        gameEnded = true;

        if(musicObject != null)
            musicObject.SetActive(false);

        if(playerController != null)
            playerController.FreezePlayer();

        if(levelCompleteUI == null)
        {
            Debug.LogError("LevelCompleteUI در GameManager تنظیم نشده است.");

            if(winPanel != null)
                winPanel.SetActive(true);

            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        bool hasNextLevel =
            currentIndex + 1 < SceneManager.sceneCountInBuildSettings;

        levelCompleteUI.ShowResult(
            score,
            maximumScore,
            currentIndex,
            hasNextLevel
        );
    }

    public void LoadNextLevel()
    {
        AbilityManager.ResetAbilities();

        int nextSceneIndex =
            SceneManager.GetActiveScene().buildIndex + 1;

        if(nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("مرحلهٔ بعدی وجود ندارد.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AbilityManager.ResetAbilities();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
