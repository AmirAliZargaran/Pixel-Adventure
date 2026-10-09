using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MovementMode
    {
        PingPong,
        Loop,
        OneWay
    }

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private MovementMode movementMode = MovementMode.PingPong;
    [SerializeField] private float waitTimeAtPoint = 0f;
    [SerializeField] private bool startAutomatically = true;

    [Header("Path")]
    [SerializeField] private Transform[] waypoints;

    [Header("Player Carry")]
    [SerializeField] private bool carryPlayer = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Detector")]
    [SerializeField] private Collider2D playerDetector;  

    private Rigidbody2D rb;
    private Vector2[] waypointPositions;

    private int currentWaypointIndex;
    private int direction = 1;

    private float waitTimer;
    private bool isWaiting;
    private bool isMoving;

    private Transform carriedPlayer;
    private Transform originalPlayerParent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"MovingPlatform روی {gameObject.name} به Rigidbody2D نیاز دارد.");
            enabled = false;
            return;
        }

        if (playerDetector == null)
        {
            Debug.LogError($"در MovingPlatform روی {gameObject.name} باید playerDetector را در Inspector معرفی کنی.");
            enabled = false;
            return;
        }

        CacheWaypointPositions();

        if (waypointPositions == null || waypointPositions.Length < 2)
        {
            Debug.LogError($"MovingPlatform روی {gameObject.name} حداقل به دو Waypoint نیاز دارد.");
            enabled = false;
            return;
        }

        currentWaypointIndex = 1;
        isMoving = startAutomatically;
    }

    private void FixedUpdate()
    {
        if (!isMoving)
            return;

        if (waypointPositions == null || waypointPositions.Length < 2)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
                isWaiting = false;

            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = waypointPositions[currentWaypointIndex];

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, targetPosition) <= 0.01f)
        {
            rb.MovePosition(targetPosition);
            ReachedWaypoint();
        }
    }

    private void CacheWaypointPositions()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        waypointPositions = new Vector2[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
                waypointPositions[i] = waypoints[i].position;
        }
    }

    private void ReachedWaypoint()
    {
        if (waitTimeAtPoint > 0f)
        {
            waitTimer = waitTimeAtPoint;
            isWaiting = true;
        }

        switch (movementMode)
        {
            case MovementMode.PingPong:
                MovePingPong();
                break;
            case MovementMode.Loop:
                MoveLoop();
                break;
            case MovementMode.OneWay:
                MoveOneWay();
                break;
        }
    }

    private void MovePingPong()
    {
        if (waypointPositions.Length == 2)
        {
            direction *= -1;
            currentWaypointIndex += direction;
            return;
        }

        currentWaypointIndex += direction;

        if (currentWaypointIndex >= waypointPositions.Length)
        {
            direction = -1;
            currentWaypointIndex = waypointPositions.Length - 2;
        }
        else if (currentWaypointIndex < 0)
        {
            direction = 1;
            currentWaypointIndex = 1;
        }
    }

    private void MoveLoop()
    {
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypointPositions.Length)
            currentWaypointIndex = 0;
    }

    private void MoveOneWay()
    {
        if (currentWaypointIndex >= waypointPositions.Length - 1)
        {
            isMoving = false;
            return;
        }

        currentWaypointIndex++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // اگر پلیر وارد شد
        if (other.CompareTag(playerTag))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetActivePlatform(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // اگر پلیر خارج شد
        if (other.CompareTag(playerTag))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ClearActivePlatform();
            }
        }
    }


    private void CarryPlayer(Transform player)
    {
        if (carriedPlayer == player)
            return;

        carriedPlayer = player;
        originalPlayerParent = player.parent;

        player.SetParent(transform, true);
    }

    private void StopCarryingPlayer()
    {
        if (carriedPlayer == null)
            return;

        carriedPlayer.SetParent(originalPlayerParent, true);

        carriedPlayer = null;
        originalPlayerParent = null;
    }

    public void StartPlatform()
    {
        isMoving = true;
    }

    public void StopPlatform()
    {
        isMoving = false;
    }

    public Vector2 GetVelocity()
    {
        if (!isMoving || isWaiting || waypointPositions == null || waypointPositions.Length < 2)
            return Vector2.zero;

        Vector2 currentTarget = waypointPositions[currentWaypointIndex];
        Vector2 direction = (currentTarget - rb.position).normalized;
        return direction * speed;
    }

}
