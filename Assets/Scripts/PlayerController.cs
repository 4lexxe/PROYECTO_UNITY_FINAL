using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float vidaMaxima = 5f;
    public float vida;
    public AudioClip damageSfx;
    public float damageSfxVolume = 1f;
    public bool blinkOnDamage = true;
    public float blinkDuration = 0.6f;
    public float blinkFrequency = 10f;
    private bool _blinking;
    private float _blinkEnd;

    void Start()
    {
        vida = vidaMaxima;
    }

    public void RecibirDanio(float cantidad)
    {
        vida -= cantidad;
        vida = Mathf.Clamp(vida, 0, vidaMaxima);
        if (damageSfx != null)
        {
            AudioSource.PlayClipAtPoint(damageSfx, transform.position, Mathf.Clamp01(damageSfxVolume));
        }
        if (blinkOnDamage)
        {
            _blinkEnd = Time.time + Mathf.Max(0.05f, blinkDuration);
            if (!_blinking) StartCoroutine(Blink());
        }
    }

    System.Collections.IEnumerator Blink()
    {
        _blinking = true;
        var srs = GetComponentsInChildren<SpriteRenderer>(true);
        float halfPeriod = 0.5f / Mathf.Max(1f, blinkFrequency);
        bool state = false;
        while (Time.time < _blinkEnd)
        {
            state = !state;
            for (int i = 0; i < srs.Length; i++)
            {
                if (srs[i] != null) srs[i].enabled = state;
            }
            yield return new WaitForSeconds(halfPeriod);
        }
        for (int i = 0; i < srs.Length; i++)
        {
            if (srs[i] != null) srs[i].enabled = true;
        }
        _blinking = false;
    }
}
