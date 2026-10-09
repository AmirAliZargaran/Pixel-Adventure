using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private Vector2? _lastCheckpoint;

    /// <summary>
    /// موقعیت آخرین چک‌پوینت ذخیره‌شده
    /// </summary>
    public Vector2? LastCheckpoint => _lastCheckpoint;

    private string _currentCheckpointId;

    /// <summary>
    /// شناسه‌ی چک‌پوینت فعلی بازیکن
    /// </summary>
    public string CurrentCheckpointId => _currentCheckpointId;

    // شناسه‌ی آیتم‌هایی که جمع‌آوری شده‌اند
    // این آیتم‌ها بعد از مرگ بازنمی‌گردند
    public HashSet<string> CollectedItems { get; } = new();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // توجه: DontDestroyOnLoad حذف شده است
    }

    /// <summary>
    /// ثبت یک چک‌پوینت جدید
    /// </summary>
    public void SetCheckpoint(string checkpointId, Vector2 position)
    {
        _currentCheckpointId = checkpointId;
        _lastCheckpoint = position;
    }

    /// <summary>
    /// بررسی می‌کند که آیا این چک‌پوینت، چک‌پوینت فعلی بازیکن است یا نه
    /// </summary>
    public bool IsCurrentCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(_currentCheckpointId))
            return false;

        return _currentCheckpointId == checkpointId;
    }

    /// <summary>
    /// نقطه‌ی ری‌اسپان:
    /// آخرین چک‌پوینت یا موقعیت فعلی پلیر در صورت نبودن چک‌پوینت
    /// </summary>
    public Vector2 GetRespawnPoint()
    {
        if (_lastCheckpoint.HasValue)
            return _lastCheckpoint.Value;

        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
            return player.transform.position;

        return Vector2.zero;
    }

    // ---------- آیتم‌ها ----------

    public bool IsCollected(string id)
    {
        return CollectedItems.Contains(id);
    }

    public void Collect(string id)
    {
        CollectedItems.Add(id);
    }
}
