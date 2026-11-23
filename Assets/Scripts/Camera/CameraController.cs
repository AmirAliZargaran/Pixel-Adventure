using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private GameObject target;
    private Camera targetCamera;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private Vector2 currentOffset;
    [SerializeField] private Vector2 targetOffset;

    // سرعت نرم شدن خودِ افست، مستقل از سرعت دنبال کردن
    [SerializeField] private float offsetSmoothSpeed = 2f;

    [Header("Zoom")]
    [Tooltip("سرعت نرم شدن تغییر زوم")]
    [SerializeField, Min(0.01f)] private float zoomSmoothSpeed = 3f;

    private float defaultOrthographicSize;
    private float targetOrthographicSize;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();

        if(targetCamera.orthographic)
        {
            defaultOrthographicSize = targetCamera.orthographicSize;
            targetOrthographicSize = defaultOrthographicSize;
        }
        else
        {
            Debug.LogWarning(
                "CameraController برای زوم Orthographic تنظیم شده، اما دوربین Perspective است.",
                this
            );
        }
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");

        if(target == null)
            Debug.LogWarning("CameraController نتوانست آبجکتی با Tag برابر Player پیدا کند.", this);
    }

    private void LateUpdate()
    {
        if(targetCamera != null && targetCamera.orthographic)
        {
            targetCamera.orthographicSize = Mathf.Lerp(
                targetCamera.orthographicSize,
                targetOrthographicSize,
                zoomSmoothSpeed * Time.deltaTime
            );
        }

        if(target == null)
            target = GameObject.FindWithTag("Player");

        currentOffset = Vector2.Lerp(
            currentOffset,
            targetOffset,
            offsetSmoothSpeed * Time.deltaTime
        );

        Vector3 targetPosition = new Vector3(
            target.transform.position.x + currentOffset.x,
            target.transform.position.y + currentOffset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    public void SetTargetOffset(Vector2 newOffset)
    {
        targetOffset = newOffset;
    }

    public void ResetTargetOffset()
    {
        targetOffset = Vector2.zero;
    }

    public void SetTargetOffsetAndZoom(Vector2 newOffset, float orthographicSize)
    {
        SetTargetOffset(newOffset);

        if(targetCamera == null || !targetCamera.orthographic)
            return;

        targetOrthographicSize = Mathf.Max(0.01f, orthographicSize);
    }

    public void ResetTargetOffsetAndZoom()
    {
        ResetTargetOffset();

        if(targetCamera == null || !targetCamera.orthographic)
            return;

        targetOrthographicSize = defaultOrthographicSize;
    }
}
