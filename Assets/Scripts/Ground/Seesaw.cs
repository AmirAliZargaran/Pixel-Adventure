using UnityEngine;

public class Seesaw : MonoBehaviour
{
    [Header("فیزیک الاکلنگ")]
    [Tooltip("سفتی: 0 = نرم، 1 = کاملاً سفت (نمی‌چرخد)")]
    [SerializeField, Range(0f, 1f)] private float stiffness = 0.2f;

    [Tooltip("حساسیت به وزن پلیر")]
    [SerializeField] private float weightForce = 1000f;

    [Tooltip("ضریب ضربه هنگام فرود پلیر از بالا")]
    [SerializeField] private float impactForce = 6f;

    [Tooltip("لختی چرخش: بیشتر = واکنش کندتر")]
    [SerializeField] private float inertia = 1f;

    [Tooltip("میراکنندگی چرخش (اصطکاک محور)")]
    [SerializeField] private float angularDrag = 1.5f;

    [Tooltip("سقف سرعت چرخش (درجه بر ثانیه)")]
    [SerializeField] private float maxAngularSpeed = 240f;

    [Tooltip("سقف زاویه کج شدن (مثلاً 45 درجه)")]
    [SerializeField] private float maxTiltAngle = 450f;

    [Tooltip("فنر بازگشت به حالت افقی (0 = خاموش)")]
    [SerializeField] private float returnSpring = 0f;

    [Header("ارجاع‌ها")]
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;
    private float angularVelocity;   // درجه بر ثانیه (مثبت = خلاف عقربه‌های ساعت)
    private bool playerOnPlank;      // آیا پلیر واقعاً روی میله است؟

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // ---------- گشتاور فقط وقتی پلیر روی میله است ----------
        if (playerOnPlank)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                float offsetX = playerObj.transform.position.x - transform.position.x;

                if (Mathf.Abs(offsetX) > 0.05f)
                {
                    // τ = r × F : دورتر بودن = گشتاور بیشتر
                    float torque = weightForce * Mathf.Abs(offsetX) * (1f - stiffness);

                    // راست → ساعت‌گرد (منفی) ، چپ → خلاف ساعت‌گرد (مثبت)
                    float direction = -Mathf.Sign(offsetX);

                    // شتاب زاویه‌ای = گشتاور ÷ لختی
                    angularVelocity += direction * torque * Time.fixedDeltaTime / inertia;
                }

                // (اختیاری) فنر بازگشت — فقط وقتی پلیر روی میله است
                if (returnSpring > 0f)
                {
                    float angle = NormalizeAngle(rb.rotation);
                    angularVelocity += -angle * returnSpring * Time.fixedDeltaTime;
                }
            }
        }

        // میراکنندگی
        angularVelocity *= Mathf.Max(0f, 1f - angularDrag * Time.fixedDeltaTime);

        // سقف سرعت چرخش
        angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularSpeed, maxAngularSpeed);

        // اعمال چرخش (روش درست برای Kinematic)
        rb.MoveRotation(rb.rotation + angularVelocity * Time.fixedDeltaTime);

        // سقف زاویه کج شدن
        float currentAngle = NormalizeAngle(rb.rotation);
        if (Mathf.Abs(currentAngle) > maxTiltAngle)
        {
            rb.MoveRotation(Mathf.Sign(currentAngle) * maxTiltAngle);
            angularVelocity = 0f;
        }
    }

    // ---------- حمل پلیر با سطح میله (مثل پلتفرم متحرک خودت) ----------
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            return;

        Rigidbody2D playerRb = collision.rigidbody;
        if (playerRb == null)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            // فقط سطح بالایی میله (نرمال رو به بالا)
            if (contact.normal.y > 0.5f)
            {
                float surfaceVelY = GetSurfaceVelocityY(contact.point.x);

                // اگر پلیر در حال پرش نیست، سرعت عمودی‌اش را با سطح هماهنگ کن
                if (playerRb.velocity.y <= surfaceVelY + 0.01f)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, surfaceVelY);
                }
                break;
            }
        }
    }

    // ---------- ضربه فرود: پلیر از بالا بیفتد → چرخش ناگهانی ----------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            return;

        playerOnPlank = true;

        float fallSpeed = Mathf.Max(0f, -collision.relativeVelocity.y);
        if (fallSpeed < 0.5f)
            return; // فقط وقتی واقعاً با سرعت فرود آمده

        ContactPoint2D contact = collision.GetContact(0);
        float offsetX = contact.point.x - transform.position.x;
        if (Mathf.Abs(offsetX) < 0.05f)
            return;

        float impact = -Mathf.Sign(offsetX) * fallSpeed * impactForce * (1f - stiffness);
        angularVelocity += impact / inertia;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
            playerOnPlank = false;
    }

    // سرعت عمودی نقطه‌ای از میله که در worldX قرار دارد (فرمول v = ω × r)
    private float GetSurfaceVelocityY(float worldX)
    {
        float r = worldX - transform.position.x;
        float omega = angularVelocity * Mathf.Deg2Rad;
        return omega * r;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
        return angle;
    }
}
