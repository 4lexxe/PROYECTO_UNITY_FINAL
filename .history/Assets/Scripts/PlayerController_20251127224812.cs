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
    public string deathStateName = "Death";
    public string deathTriggerName = "Death";
    public bool spawnDeathLight = true;
    public float deathLightIntensity = 4f;
    public float deathLightRange = 10f;
    public float deathLightDuration = 1.5f;
    public float deathGameOverDelay = 5f;
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
                bool done = false;
                if (!string.IsNullOrEmpty(deathStateName))
                {
                    int hash = Animator.StringToHash(deathStateName);
                    if (_anim.HasState(0, hash))
                    {
                        _anim.CrossFadeInFixedTime(deathStateName, 0f);
                        done = true;
                    }
                }
                if (!done && !string.IsNullOrEmpty(deathTriggerName))
                {
                    for (int i = 0; i < _anim.parameterCount; i++)
                    {
                        var p = _anim.parameters[i];
                        if (p.name == deathTriggerName)
                        {
                            _anim.ResetTrigger(deathTriggerName);
                            _anim.SetTrigger(deathTriggerName);
                            done = true;
                            break;
                        }
                    }
                }
            }
            if (spawnDeathLight)
            {
                var go = new GameObject("DeathLight");
                go.transform.position = transform.position;
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Color.white;
                light.intensity = deathLightIntensity;
                light.range = deathLightRange;
                light.shadows = LightShadows.None;
                Destroy(go, Mathf.Max(0.1f, deathLightDuration));
            }

            if (GameManager.instance != null)
            {
                StartCoroutine(GameOverAfterDeath());
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

    System.Collections.IEnumerator GameOverAfterDeath()
    {
        if (_anim == null)
        {
            GameManager.instance.GameOver();
            yield break;
        }
        float waitEnterTimeout = 2.0f;
        float endEnter = Time.time + waitEnterTimeout;
        while (Time.time < endEnter)
        {
            var st = _anim.GetCurrentAnimatorStateInfo(0);
            if (!string.IsNullOrEmpty(deathStateName) && st.IsName(deathStateName)) break;
            yield return null;
        }
        yield return new WaitForSeconds(Mathf.Max(0f, deathGameOverDelay));
        GameManager.instance.GameOver();
    }
}
