using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private CanvasGroup canvasGroup; // برای اعمال شفافیت نرم
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine activeFadeRoutine;
    public static TutorialUI Instance;

    private void Awake()
    {
        Instance = this;
        // در شروع بازی مطمئن می‌شویم متن پنهان است
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public static void Show(string message)
    {
        if (Instance == null)
            return;
        Instance.StartFade(message, 1f);
    }

    public static void Hide()
    {
        if (Instance == null)
            return;
        Instance.StartFade("", 0f);
    }

    private void StartFade(string message, float targetAlpha)
    {
        if (activeFadeRoutine != null)
            StopCoroutine(activeFadeRoutine);

        activeFadeRoutine = StartCoroutine(FadeRoutine(message, targetAlpha));
    }

    private IEnumerator FadeRoutine(string message, float targetAlpha)
    {
        // در هنگام شروع انیمیشن ظاهر شدن، متن را مقداردهی می‌کنیم
        if (targetAlpha > 0f)
        {
            tutorialText.text = message;
        }

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // پس از پایان انیمیشن محو شدن کامل، متن را خالی می‌کنیم
        if (targetAlpha == 0f)
        {
            tutorialText.text = "";
        }
    }
}
