using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float delay = 5f; // مدت زمان انیمیشن مرگ

    void Start()
    {
        Destroy(gameObject, delay);
    }
}
