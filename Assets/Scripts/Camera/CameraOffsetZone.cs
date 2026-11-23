using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraOffsetZone : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("منفی برای پایین‌تر رفتن دوربین، مثبت برای بالاتر رفتن")]
    [SerializeField] private Vector2 targetOffset = new Vector2(0f, -3f);

    [Header("Zoom Settings")]
    [Tooltip("اگر فعال باشد، این ناحیه اندازه نمای دوربین را تغییر می‌دهد")]
    [SerializeField] private bool changeZoom = true;

    [Tooltip("برای Orthographic: عدد کمتر = Zoom In و عدد بیشتر = Zoom Out")]
    [Min(0.01f)]
    [SerializeField] private float targetOrthographicSize = 5f;

    private CameraController cameraController;

    private void Start()
    {
        Camera mainCamera = Camera.main;

        if(mainCamera != null)
            cameraController = mainCamera.GetComponent<CameraController>();

        if(cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        if(cameraController == null)
            Debug.LogWarning("CameraOffsetZone نتوانست CameraController را پیدا کند.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player") || cameraController == null)
            return;

        if(changeZoom)
            cameraController.SetTargetOffsetAndZoom(targetOffset, targetOrthographicSize);
        else
            cameraController.SetTargetOffset(targetOffset);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.CompareTag("Player") || cameraController == null)
            return;

        cameraController.ResetTargetOffsetAndZoom();
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if(col == null)
            return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
