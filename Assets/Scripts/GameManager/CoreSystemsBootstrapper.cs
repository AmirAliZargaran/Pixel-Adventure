using UnityEngine;

[DisallowMultipleComponent]
public class CoreSystemsBootstrapper : MonoBehaviour
{
    public static CoreSystemsBootstrapper Instance
    {
        get; private set;
    }

    private void Awake()
    {
        // پیاده‌سازی الگوی Singleton برای جلوگیری از تکثیر آبجکت‌ها موقع تعویض مرحله
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // زنده نگه داشتن کل ساختار CoreSystems و تمام فرزندانش در طول بازی
        DontDestroyOnLoad(gameObject);
    }
}
