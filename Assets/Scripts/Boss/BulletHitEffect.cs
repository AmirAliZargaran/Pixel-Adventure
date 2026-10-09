using System.Collections;
using UnityEngine;

public class BulletHitEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.6f;
    [SerializeField] private bool fadeOut = true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if(fadeOut && spriteRenderer != null)
        {
            StartCoroutine(FadeOutRoutine());
        }
        else
        {
            Destroy(gameObject, lifetime);
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        float timer = 0f;
        Color initialColor = spriteRenderer.color;

        while(timer < lifetime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, timer / lifetime);
            spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
