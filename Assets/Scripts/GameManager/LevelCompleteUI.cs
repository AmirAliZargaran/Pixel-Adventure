using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI Layouts")]
    [SerializeField] private GameObject levelsUI;
    [SerializeField] private GameObject bossUI;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI scoreValue;

    [Header("Stars - ترتیب: چپ، وسط، راست")]
    [SerializeField] private Image[] starImages = new Image[3];
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    [Header("Star thresholds (%)")]
    [Range(0f, 100f)]
    [SerializeField] private float oneStarMaxPercent = 30f;

    [Range(0f, 100f)]
    [SerializeField] private float threeStarMinPercent = 90f;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;

    /// <summary>
    /// برای مرحله‌های عادی استفاده شود.
    /// </summary>
    public void ShowResult(int score, int maximumScore, int levelNumber, bool hasNextLevel)
    {
        ShowResultInternal(score, maximumScore, levelNumber, hasNextLevel, false);
    }

    /// <summary>
    /// برای مرحله باس استفاده شود.
    /// </summary>
    public void ShowBossResult(int score, int maximumScore, int levelNumber)
    {
        ShowResultInternal(score, maximumScore, levelNumber, false, true);
    }

    private void ShowResultInternal(
        int score,
        int maximumScore,
        int levelNumber,
        bool hasNextLevel,
        bool isBossLevel)
    {
        // فقط چیدمان مربوط به نوع مرحله را فعال می‌کنیم.
        if(levelsUI != null)
            levelsUI.SetActive(!isBossLevel);

        if(bossUI != null)
            bossUI.SetActive(isBossLevel);

        if(levelTitleText != null)
        {
            levelTitleText.text = isBossLevel
                ? "Boss Defeated"
                : "Level " + levelNumber;
        }

        if(scoreLabel != null)
            scoreLabel.text = "Score";

        if(scoreValue != null)
            scoreValue.text = score.ToString("N0");

        int earnedStars = CalculateStars(score, maximumScore);

        if(starImages != null)
        {
            for(int i = 0; i < starImages.Length; i++)
            {
                if(starImages[i] == null)
                    continue;

                starImages[i].sprite = i < earnedStars
                    ? filledStarSprite
                    : emptyStarSprite;
            }
        }

        if(nextButton != null)
            nextButton.interactable = hasNextLevel;

        gameObject.SetActive(true);
    }

    private int CalculateStars(int score, int maximumScore)
    {
        if(maximumScore <= 0)
        {
            Debug.LogWarning(
                "سقف امتیاز این مرحله صفر است؛ تعداد سیب‌ها یا مقدار Inspector را بررسی کن."
            );
            return 0;
        }

        float percent = (float)score / maximumScore * 100f;

        if(percent >= threeStarMinPercent)
            return 3;

        if(percent <= oneStarMaxPercent)
            return 1;

        return 2;
    }
}
