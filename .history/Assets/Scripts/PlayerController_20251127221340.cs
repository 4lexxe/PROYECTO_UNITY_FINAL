using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float vidaMaxima = 5f;
    public float vida;
    public bool muerto = false;
    public AudioClip damageSfx;
    public float damageSfxVolume = 1f;
    public bool blinkOnDamage = true;
    public float blinkDuration = 0.6f;
    public float blinkFrequency = 10f;
    public bool disableMovementOnDeath = true;
    private bool _blinking;
    private float _blinkEnd;
    private Animator _anim;

    void Start()
    {
        vida = vidaMaxima;
        _anim = GetComponent<Animator>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
    }

    public void RecibirDanio(float cantidad)
    {
        vida -= cantidad;
        vida = Mathf.Clamp(vida, 0, vidaMaxima);
        if (vida <= 0 )
        {
            muerto = true;
            var pm = GetComponent<PlayerMovement>();
            if (pm == null) pm = GetComponentInParent<PlayerMovement>();
            if (pm != null && disableMovementOnDeath) pm.enabled = false;
            if (_anim != null)
            {
                int hash = Animator.StringToHash("Death");
                if (_anim.HasState(0, hash)) _anim.CrossFadeInFixedTime("Death", 0f);
                else
                {
                    for (int i = 0; i < _anim.parameterCount; i++)
                    {
                        var p = _anim.parameters[i];
                        if (p.name == "Death")
                        {
                            _anim.ResetTrigger("Death");
                            _anim.SetTrigger("Death");
                            break;
                        }
                    }
                }
            }

            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }

        }
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
