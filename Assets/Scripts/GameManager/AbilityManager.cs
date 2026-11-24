using System.Collections.Generic;
using UnityEngine;

public enum AbilityType
{
    None, Dash, DoubleJump, HoldJump, FastFall
}

public class AbilityManager : MonoBehaviour
{
    // وضعیت توانایی‌ها
    public static readonly Dictionary<AbilityType, bool> Unlocked = new()
    {
        { AbilityType.Dash,       false },
        { AbilityType.DoubleJump, false },
        { AbilityType.HoldJump,   false },
        { AbilityType.FastFall,   false },
    };

    public static void Unlock(AbilityType ability)
    {
        if(ability == AbilityType.None)
            return;

        if(Unlocked[ability])
            return;

        Unlocked[ability] = true;

        // SoundManager.Instance?.PlaySound(unlockSound);
    }

    public static bool IsUnlocked(AbilityType ability) => Unlocked.ContainsKey(ability) && Unlocked[ability];

    /// <summary>
    /// این متد وضعیت تمام ابیلیتی‌ها را به حالت قفل برمی‌گرداند.
    /// می‌توانید در Start بازی، متد Retry در GameManager یا هنگام ورود به صحنه منو فراخوانی کنید.
    /// </summary>
    public static void ResetAbilities()
    {
        Unlocked[AbilityType.Dash] = false;
        Unlocked[AbilityType.DoubleJump] = false;
        Unlocked[AbilityType.HoldJump] = false;
        Unlocked[AbilityType.FastFall] = false;
    }
}
