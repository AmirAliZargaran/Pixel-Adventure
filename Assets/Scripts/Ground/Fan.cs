//using UnityEngine;

//[RequireComponent(typeof(BoxCollider2D))]
//public class Fan : MonoBehaviour
//{
//    [SerializeField] private float fanForce = 15f;

//    private BoxCollider2D fanCollider;

//    private void Awake()
//    {
//        fanCollider = GetComponent<BoxCollider2D>();
//        fanCollider.isTrigger = true;
//    }

//    private void OnTriggerStay2D(Collider2D other)
//    {
//        if (!other.CompareTag("Player"))
//            return;

//        Rigidbody2D rb = other.attachedRigidbody;
//        if (rb == null)
//            return;

//        // بالاترین نقطه‌ی collider
//        float topY = fanCollider.bounds.max.y;

//        // اگر پلیر هنوز داخل محدوده‌ی عمودی فن است
//        if (rb.position.y < topY)
//        {
//            rb.AddForce(Vector2.up * fanForce, ForceMode2D.Force);
//        }
//    }
//}

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Fan : MonoBehaviour
{
    [Header("Fan Force")]
    [SerializeField] private float upwardForce = 60f;

    [Tooltip("حداکثر سرعت عمودی پلیر هنگام قرار داشتن در باد فن")]
    [SerializeField] private float maxUpwardSpeed = 8f;

    [Header("Force Falloff")]
    [Tooltip("فعال باشد: هرچه پلیر به بالای Trigger نزدیک‌تر شود، نیروی باد کمتر می‌شود.")]
    [SerializeField] private bool useSoftFalloff = true;

    [Tooltip("نحوه کم شدن قدرت باد از پایین به بالا. سمت چپ = پایین فن، سمت راست = بالای فن.")]
    [SerializeField]
    private AnimationCurve forceByHeight =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private float velocityMultiplier = 0.01f; // ضریبی برای تبدیل نیرو به سرعت بصری
    [SerializeField] private float driftAmount = 0.2f;        // میزان انحراف افقی
    [SerializeField] private float upwardForceSamplesMultiplier = 0.1f;        // نسبت نیروی عمودی به سرعت انیمیشن

    private BoxCollider2D fanCollider;

    private void Start()
    {
        // تنظیم سرعت انیمیشن بر اساس قدرت پرتاب
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            float baseSamples = 12f; // مقداری که در پنجره انیمیشن ست شده
            float targetSamples = upwardForce * upwardForceSamplesMultiplier; // هدفی که شما تعیین کردید

            // تنظیم سرعت پخش انیماتور
            anim.speed = targetSamples / baseSamples;
        }

        if (windParticles == null)
            return;

        var main = windParticles.main;
        var shape = windParticles.shape;
        var velocity = windParticles.velocityOverLifetime;

        BoxCollider2D fanCollider = GetComponent<BoxCollider2D>();

        // -----------------------------
        // تنظیمات اصلی Particle System
        // -----------------------------

        // سرعت اولیه را صفر می‌کنیم تا حرکت فقط از Velocity over Lifetime باشد
        main.startSpeed = 0f;

        // گرانش نباید روی ذرات اثر بگذارد
        main.gravityModifier = 0f;

        // ذرات در فضای محلی Particle System حرکت کنند
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // -----------------------------
        // تنظیم محدوده تولید ذرات
        // -----------------------------

        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;

        // تولید ذرات در یک نوار باریک در پایین فن
        shape.scale = new Vector3(
            fanCollider.size.x,
            0.1f,
            0.1f
        );

        // قرار دادن محل تولید در پایین محدوده کولایدر
        windParticles.transform.localPosition = new Vector3(
            fanCollider.offset.x,
            fanCollider.offset.y - (fanCollider.size.y / 2f),
            0f
        );

        // -----------------------------
        // محاسبه سرعت عمودی ذرات
        // -----------------------------

        // pushForce باید همان نیروی پرتاب فن باشد
        float verticalSpeed = Mathf.Max(
            0.01f,
            upwardForce * velocityMultiplier
        );

        // -----------------------------
        // تنظیم Velocity over Lifetime
        // -----------------------------

        velocity.enabled = true;

        // مهم:
        // هر سه محور باید از یک حالت استفاده کنند.
        // بنابراین X، Y و Z همگی Two Constants هستند.

        // انحراف تصادفی کم در محور افقی
        velocity.x = new ParticleSystem.MinMaxCurve(
            -driftAmount,
            driftAmount
        );

        // حرکت رو به بالا
        // هر دو مقدار برابرند، پس سرعت Y ثابت باقی می‌ماند،
        // ولی Mode همچنان Two Constants است.
        velocity.y = new ParticleSystem.MinMaxCurve(
            verticalSpeed,
            verticalSpeed
        );

        // حرکت در محور Z نداریم
        velocity.z = new ParticleSystem.MinMaxCurve(
            0f,
            0f
        );

        // -----------------------------
        // تنظیم طول عمر ذرات
        // -----------------------------

        // ذره تقریباً در ارتفاع کولایدر به انتهای عمر خود می‌رسد
        main.startLifetime = fanCollider.size.y / verticalSpeed;
    }

    private void Awake()
    {
        fanCollider = GetComponent<BoxCollider2D>();
        fanCollider.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = other.attachedRigidbody;
        if (playerRb == null)
            return;

        Vector2 up = transform.up;   // جهت بالای فن (شامل چرخش)

        // موقعیت پلیر در فضای محلی فن
        Vector2 localPos = transform.InverseTransformPoint(playerRb.position);
        Bounds localBounds = new Bounds(fanCollider.offset, fanCollider.size);

        // 0 = پایین فن (پنکه)، 1 = بالای فن (دهانه باد)
        float normalizedHeight = Mathf.InverseLerp(
            localBounds.min.y, localBounds.max.y, localPos.y
        );

        float forceMultiplier = useSoftFalloff
            ? forceByHeight.Evaluate(normalizedHeight)
            : 1f;

        if (normalizedHeight >= 1f || forceMultiplier <= 0f)
            return;

        // فقط تا سرعت سقف در راستای فن شتاب بده
        float speedAlongFan = Vector2.Dot(playerRb.velocity, up);
        if (speedAlongFan < maxUpwardSpeed)
        {
            playerRb.AddForce(up * upwardForce * forceMultiplier, ForceMode2D.Force);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
            return;

        Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.25f);

        // نمایش محدوده واقعی باد در Scene View
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawCube(box.offset, box.size);

        Gizmos.matrix = previousMatrix;
    }
}
