using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraOffsetZone : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("منفی برای پایین‌تر رفتن دوربین، مثبت برای بالاتر رفتن")]
    [SerializeField] private Vector2 targetOffset = new Vector2(0f, -3f);

    private CameraController cameraController;

    private void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        cameraController?.SetTargetOffset(targetOffset);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        cameraController?.ResetTargetOffset();
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
