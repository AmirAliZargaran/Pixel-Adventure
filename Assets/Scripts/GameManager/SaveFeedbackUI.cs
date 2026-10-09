using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SaveFeedbackUI : MonoBehaviour
{
    public static SaveFeedbackUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Animation")]
    [Tooltip("فاصله متن از مرکز صفحه، پیش از شروع انیمیشن")]
    [SerializeField] private float baseYPosition = 100f;

    [Tooltip("چند پیکسل بالاتر از موقعیت اصلی، نمایش شروع شود")]
    [SerializeField] private float enterOffsetY = 20f;

    [Tooltip("چند پیکسل بالاتر از موقعیت اصلی، Fade Out تمام شود")]
    [SerializeField] private float exitOffsetY = 20f;

    [Tooltip("مدت زمان Fade In و حرکت رو به پایین")]
    [SerializeField] private float fadeInDuration = 0.2f;

    [Tooltip("مدت زمانی که متن کامل دیده می‌شود")]
    [SerializeField] private float visibleDuration = 1.2f;

    [Tooltip("مدت زمان Fade Out و حرکت رو به بالا")]
    [SerializeField] private float fadeOutDuration = 0.25f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Coroutine _feedbackCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (_feedbackCoroutine != null)
            StopCoroutine(_feedbackCoroutine);

        gameObject.SetActive(true);
        _feedbackCoroutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        messageText.text = message;

        Vector2 basePosition = new Vector2(
            _rectTransform.anchoredPosition.x,
            baseYPosition
        );

        // متن از کمی بالاتر شروع می‌شود و در حین Fade In به پایین می‌آید
        Vector2 enterPosition = basePosition + Vector2.up * enterOffsetY;

        _rectTransform.anchoredPosition = enterPosition;
        _canvasGroup.alpha = 0f;

        // Fade In + حرکت به پایین
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / fadeInDuration);

            _canvasGroup.alpha = progress;
            _rectTransform.anchoredPosition =
                Vector2.Lerp(enterPosition, basePosition, progress);

            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = basePosition;

        // نمایش کامل متن
        yield return new WaitForSecondsRealtime(visibleDuration);

        // Fade Out + حرکت دوباره به بالا
        Vector2 exitPosition = basePosition + Vector2.up * exitOffsetY;

        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / fadeOutDuration);

            _canvasGroup.alpha = 1f - progress;
            _rectTransform.anchoredPosition =
                Vector2.Lerp(basePosition, exitPosition, progress);

            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _rectTransform.anchoredPosition = exitPosition;

        gameObject.SetActive(false);
        _feedbackCoroutine = null;
    }
}
