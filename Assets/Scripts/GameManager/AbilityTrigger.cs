using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityTrigger : MonoBehaviour
{
    [SerializeField] private AbilityType abilityType = AbilityType.None;
    [TextArea(2, 4)]
    [SerializeField] private string tutorialMessage = "Press W to Jump";

    [Header("Trigger Options")]
    [Tooltip("آیا بعد از خروج بازیکن از تریگر، این آبجکت غیرفعال/حذف شود؟")]
    [SerializeField] private bool destroyTriggerAfterUnlock = false;

    private bool hasUnlocked = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player"))
            return;

        // نمایش آموزش
        TutorialUI.Show(tutorialMessage);

        if(abilityType == AbilityType.None)
        {
            hasUnlocked = true;
            return;
        }

        AbilityManager.Unlock(abilityType);
        hasUnlocked = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.CompareTag("Player"))
            return;

        // پنهان کردن آموزش
        TutorialUI.Hide();

        if(destroyTriggerAfterUnlock && hasUnlocked)
        {
            // نکته: اگر می‌خواهید با ریست برگردد، بهتر است Destroy نکنید، 
            // اما اگر صحنه کاملاً مجدداً بارگذاری (Reload Scene) می‌شود، Destroy موردی ندارد.
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if(col == null)
            return;

        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
