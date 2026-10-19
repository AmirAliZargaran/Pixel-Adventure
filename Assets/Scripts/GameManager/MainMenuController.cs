using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("First Level Name")]
    [Tooltip("نام دقیق صحنه مرحله اول بازی")]
    [SerializeField] private string firstLevelSceneName = "Level_1";

    private void Start()
    {
        // اطمینان از اینکه فقط منوی اصلی در ابتدا باز است
        ShowMainMenu();
    }

    // --- مدیریت پنل‌ها ---
    public void ShowMainMenu()
    {
        if(mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if(levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    public void OpenLevelSelect()
    {
        PlayClickSound();
        if(mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if(levelSelectPanel != null)
            levelSelectPanel.SetActive(true);
    }

    // --- لود صحنه‌ها ---
    public void PlayGame()
    {
        PlayClickSound();
        // مستقیم به مرحله اول می‌رود
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void LoadLevel(string sceneName)
    {
        PlayClickSound();
        // برای استفاده در دکمه‌های پنل Level Select (با نوشتن اسم صحنه در OnClick)
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Quit Game pressed!");
        Application.Quit();
    }

    // --- افکت‌های صوتی ---
    public void PlayClickSound()
    {
        if(uiAudioSource != null && clickSound != null)
            uiAudioSource.PlayOneShot(clickSound);
    }

    public void PlayHoverSound()
    {
        if(uiAudioSource != null && hoverSound != null)
            uiAudioSource.PlayOneShot(hoverSound);
    }
}
