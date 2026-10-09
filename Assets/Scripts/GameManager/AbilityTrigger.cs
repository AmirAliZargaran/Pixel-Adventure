using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityTrigger : MonoBehaviour
{
    [SerializeField] private AbilityType abilityType = AbilityType.None;
    [SerializeField] private string tutorialMessage = "Press W to Jump";

    [Header("Trigger Options")]
    [SerializeField] private bool destroyTriggerAfterUnlock = false;

    private bool hasUnlocked = false;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        TutorialUI.Show(tutorialMessage);

        if (abilityType == AbilityType.None)
            return;

        AbilityManager.Unlock(abilityType);
        hasUnlocked = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        TutorialUI.Hide();

        if (destroyTriggerAfterUnlock && hasUnlocked)
        {
            Destroy(gameObject);
        }
    }
}
