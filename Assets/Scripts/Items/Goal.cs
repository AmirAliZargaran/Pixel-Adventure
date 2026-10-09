using UnityEngine;

public class Goal : Item
{
    protected override void OnCollect(PlayerController player)
    {
        // فقط پایان مرحله
        Debug.Log("Level Complete");
    }
}
