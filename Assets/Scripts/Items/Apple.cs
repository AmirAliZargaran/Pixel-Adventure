using UnityEngine;

public class Apple : Item
{
    [SerializeField] private string itemId = "coin_1"; // یکتا و ثابت برای هر آیتم
    [Tooltip("امتیازی که با گرفتن این آیتم می‌گیرد")]
    [SerializeField] private int scoreValue = 1;

    private void Awake()
    {
        // اگر از قبل توی این بازی جمع شده بود، اصلاً در صحنه ظاهر نشود
       // if (SaveManager.Instance.IsCollected(itemId))
         //   gameObject.SetActive(false);
    }


    protected override void OnCollect(PlayerController player)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddScore(scoreValue);
        }
        SaveManager.Instance.Collect(itemId);
    }
}
