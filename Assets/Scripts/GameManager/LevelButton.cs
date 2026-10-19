using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("نام دقیق سینی که باید لود شود (مثلاً Level1)")]
    [SerializeField] private string sceneName;

    [Tooltip("شماره مرحله برای نمایش روی دکمه (اختیاری)")]
    [SerializeField] private int levelNumber = 1;

    [Header("UI References (اختیاری)")]
    [SerializeField] private TextMeshProUGUI levelText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        // اگر فیلد متنی اختصاص داده نشده بود، فرزند دکمه را برای متن جستجو کن
        if(levelText == null)
        {
            levelText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // تنظیم خودکار متن روی دکمه بر اساس شماره مرحله
        if(levelText != null && levelNumber > 0)
        {
            levelText.text = levelNumber.ToString();
        }
    }

    private void Start()
    {
        // اضافه کردن لیسنر کلیک به صورت اتوماتیک از طریق کد
        button.onClick.AddListener(OnLevelClicked);
    }

    private void OnLevelClicked()
    {
        if(!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"Loading Scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"نام Scene برای دکمه {gameObject.name} مشخص نشده است!");
        }
    }

    private void OnDestroy()
    {
        // جلوگیری از نشت حافظه (Memory Leak)
        if(button != null)
        {
            button.onClick.RemoveListener(OnLevelClicked);
        }
    }
}
