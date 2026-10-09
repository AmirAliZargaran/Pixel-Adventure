using UnityEngine;
using TMPro;
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

    private GameObject playerObject;
    private PlayerHealth playerHealth;
    private PlayerController playerController;
    private GameObject endGoal;

    private bool gameEnded = false;
    private int score = 0;

    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        endGoal = GameObject.FindGameObjectWithTag("End");

        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            playerController = playerObject.GetComponent<PlayerController>();
            playerHealth.OnHealthChanged += UpdateHealthUI;
            playerHealth.OnDeath += HandlePlayerDeath;
        }

        UpdateScoreUI();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void Update()
    {
        if (!gameEnded && endGoal == null)
            WinGame();
    }

    private void UpdateHealthUI(int currentHealth)
    {
        for (int i = 0; i < healthHearts.Length; i++)
            healthHearts[i].SetActive(currentHealth > i);
    }

    public void AddScore(int amount)
    {
        if (gameEnded)
            return;

        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void HandlePlayerDeath()
    {
        // gameEnded = true;
        // losePanel.SetActive(true);

       // if (musicObject != null)
           // musicObject.SetActive(false);
    }

    private void WinGame()
    {
        gameEnded = true;
        winPanel.SetActive(true);

        if (musicObject != null)
            musicObject.SetActive(false);

        if (playerController != null)
            playerController.FreezePlayer();
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("There is no next level.");
        }
    }

}
