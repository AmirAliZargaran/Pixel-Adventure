using System.Collections.Generic;
using UnityEngine;

// اضافه شدن گزینه None برای آموزش‌های عمومی بدون نیاز به Unlock
public enum AbilityType { None, Dash, DoubleJump, HoldJump, FastFall }

public class AbilityManager : MonoBehaviour
{
    // وضعیت پیش‌فرض توانایی‌ها
    public static readonly Dictionary<AbilityType, bool> Unlocked = new()
    {
        { AbilityType.Dash,       true },
        { AbilityType.DoubleJump, true },
        { AbilityType.HoldJump,   true },
        { AbilityType.FastFall,   true },
    };

    public static void Unlock(AbilityType ability)
    {
        // اگر نوع توانایی None بود، نیازی به ثبت قفل‌گشایی نیست و فقط آموزش نشان داده می‌شود
        if (ability == AbilityType.None)
            return;

        // اگر از قبل باز شده بود، دیگر نیازی به اجرای مجدد افکت‌ها نیست
        if (Unlocked[ability])
            return;

        Unlocked[ability] = true;

        // SoundManager.Instance.PlaySound(unlockSound);
    }

    public static bool IsUnlocked(AbilityType ability) => Unlocked[ability];
}
