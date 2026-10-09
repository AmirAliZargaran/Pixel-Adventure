using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    [Header("Checkpoint Identity")]
    [Tooltip("به‌صورت خودکار و یکتا تولید می‌شود.")]
    [SerializeField] private string checkpointId = "";

    [Header("Feedback")]
    [SerializeField] private bool playFeedback = true;
    [SerializeField] private AudioClip savePointAudio;
    [SerializeField] private string savedMessage = "Saved!";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager در صحنه وجود ندارد.");
            return;
        }

        if (SaveManager.Instance.IsCurrentCheckpoint(checkpointId))
            return;

        SaveManager.Instance.SetCheckpoint(checkpointId, transform.position);

        if (!playFeedback)
            return;

        if (savePointAudio != null)
            SoundManager.Instance?.PlaySound(savePointAudio);

        SaveFeedbackUI.Instance?.Show(savedMessage);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // اگر هنوز شناسه ندارد، یک شناسهٔ یکتا بساز
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = System.Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(this);
        }
    }
#endif

#if UNITY_EDITOR
    [ContextMenu("Generate New ID")]
    private void GenerateNewId()
    {
        checkpointId = System.Guid.NewGuid().ToString("N");
        EditorUtility.SetDirty(this);
    }

    // هشدار هنگام تکراری بودن شناسه‌ها در صحنه
    private void OnDrawGizmos()
    {
        var all = FindObjectsOfType<SavePoint>();
        foreach (var sp in all)
        {
            if (sp != this && sp.checkpointId == checkpointId)
            {
                Debug.LogWarning($"ID تکراری در '{gameObject.name}' و '{sp.gameObject.name}'", this);
            }
        }
    }
#endif

}
