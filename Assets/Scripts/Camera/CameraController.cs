using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject target;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private Vector2 currentOffset;
    [SerializeField] private Vector2 targetOffset;

    // سرعت نرم شدن خودِ افست (مستقل از سرعت دنبال کردن)
    [SerializeField] private float offsetSmoothSpeed = 2f;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // آفست بهتدریج به سمت هدف نرم میشود
        currentOffset = Vector2.Lerp(currentOffset, targetOffset, offsetSmoothSpeed * Time.deltaTime);

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

    public void ResetTargetOffset() { targetOffset = Vector2.zero; }
}
