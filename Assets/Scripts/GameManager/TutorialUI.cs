using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine activeFadeRoutine;
    public static TutorialUI Instance
    {
        get; private set;
    }

    private void Awake()
    {
        // ثبت امن Instance
        Instance = this;

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        // در صورت از بین رفتن این آبجکت با عوض شدن صحنه، رفرنس استاتیک خالی شود
        if(Instance == this)
        {
            Instance = null;
        }
    }

    public static void Show(string message)
    {
        // اگر به هر دلیلی در جابجایی صحنه Instance قطع شده بود، یک بار در صحنه جستجو می‌کنیم
        if(Instance == null)
        {
            Instance = FindObjectOfType<TutorialUI>();
        }

        if(Instance == null)
        {
            Debug.LogWarning("[TutorialUI] هیچ آبجکتی با اسکریپت TutorialUI در صحنه فعال نیست!");
            return;
        }

        Instance.StartFade(message, 1f);
    }

    public static void Hide()
    {
        if(Instance == null)
        {
            Instance = FindObjectOfType<TutorialUI>();
        }

        if(Instance == null)
            return;

        Instance.StartFade("", 0f);
    }

    private void StartFade(string message, float targetAlpha)
    {
        if(activeFadeRoutine != null)
            StopCoroutine(activeFadeRoutine);

        activeFadeRoutine = StartCoroutine(FadeRoutine(message, targetAlpha));
    }

    private IEnumerator FadeRoutine(string message, float targetAlpha)
    {
        if(canvasGroup == null || tutorialText == null)
        {
            Debug.LogError("[TutorialUI] کامپوننت‌های tutorialText یا canvasGroup روی این آبجکت ست نشده‌اند!");
            yield break;
        }

        if(targetAlpha > 0f)
        {
            tutorialText.text = message;
        }

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while(time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if(targetAlpha == 0f)
        {
            tutorialText.text = "";
        }
    }
}
