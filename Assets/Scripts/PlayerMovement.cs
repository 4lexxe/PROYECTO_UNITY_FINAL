using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float jumpDuration = 1.0f; // Duración máxima del salto
    public float attackSpeedMultiplier = 1.6f;
    public string attackLeftStateName = "attack_0";
    public string attackRightStateName = "attack_1";
    public bool debugAttack = true;
    public Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);
    public Vector2 attackOffset = new Vector2(0.8f, 0.3f);
    public int attackDamage = 1;
    public bool attackDamageOnStart = true;
    public string slideStateName = "slide";
    public string slideTrigger = "Slide";
    public float slideDuration = 0.5f;
    public float slideSpeed = 9f;
    public bool debugSlide = true;
    public float slideSpeedMultiplier = 1.8f;
    public AudioSource sfxSource;
    public AudioClip sfxAttack0;
    public AudioClip sfxAttackAlt;
    public float sfxVolume = 1f;
    public AudioClip sfxSlide;
    public float sfxSlideVolume = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpLock;
    private float lastJumpTime; // Tiempo del último salto
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public float groundCheckDistance = 0.3f;
    public float airControlFactor = 0.7f;
    public bool runFullSpeedInAir = true;

    private float moveX;
    private bool attackLock;
    private bool isAttacking;
    private string currentAttackStateName;
    private float attackEndTime;
    public float attackMaxDuration = 0.6f;
    private bool attackDurationFromStateSet;
    private int attackIndex;
    private bool slideLock;
    private bool isSliding;
    private float slideEndTime;
    private int slideDirection;
    private float slideBoost;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = GetComponentInChildren<AudioSource>();
        }
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        lastJumpTime = -jumpDuration; // Inicializar para permitir salto inmediato
    }

    void Update()
    {

        moveX = Input.GetAxisRaw("Horizontal");
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;
        
        bool jumpDown = Input.GetButtonDown("Jump");
        bool jumpUp = Input.GetButtonUp("Jump");
        bool leftDown = Input.GetMouseButtonDown(0);
        bool rightDown = Input.GetMouseButtonDown(1);
        bool shiftDown = Input.GetKeyDown(KeyCode.LeftShift);
        
        if (shiftDown && !slideLock && isGrounded && !isAttacking)
        {
            int dir = 0;
            if (Mathf.Abs(moveX) > 0.01f) dir = moveX > 0 ? 1 : -1;
            else dir = sr != null && sr.flipX ? -1 : 1;
            StartSlide(dir);
        }
        // Detectar si está en el suelo
        Vector3 gcPos = groundCheck != null ? groundCheck.position : transform.position;
        bool circle = Physics2D.OverlapCircle(gcPos, groundCheckRadius, groundLayer) != null;
        bool ray = Physics2D.Raycast(gcPos, Vector2.down, groundCheckDistance, groundLayer).collider != null;
        isGrounded = circle || ray;

        // Verificar si ha pasado el tiempo suficiente desde el último salto
        bool canJump = (Time.time - lastJumpTime) >= jumpDuration;

        // Lógica de salto mejorada
        if (jumpDown && isGrounded && !jumpLock && canJump)
        {
            jumpPressed = true;
            lastJumpTime = Time.time; // Registrar el tiempo del salto
        }

        // Resetear el bloqueo cuando toca el suelo y no está presionando salto
        if (isGrounded && !Input.GetButton("Jump")) 
        {
            jumpLock = false;
        }

        // Si no está en el suelo, activar bloqueo para prevenir doble salto
        if (!isGrounded) 
        {
            jumpLock = true;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveX));
            anim.SetBool("isJumping", !isGrounded);
        }

        if (leftDown && !attackLock)
        {
            string chosen = (attackIndex % 2 == 0) ? attackLeftStateName : attackRightStateName;
            attackIndex++;
            if (debugAttack) Debug.Log($"Click Left | chosen:{chosen} idx:{attackIndex-1} anim:{(anim!=null)} ctrl:{(anim!=null && anim.runtimeAnimatorController!=null)}");
            StartAttack(chosen);
        }

        if (anim != null)
        {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            bool isAttackState = st.IsName(attackLeftStateName) || st.IsName(attackRightStateName) || (!string.IsNullOrEmpty(currentAttackStateName) && st.IsName(currentAttackStateName));
            bool isSlideState = st.IsName(slideStateName);
            if (isAttacking && !isAttackState)
            {
                if (debugAttack) Debug.Log($"Attack End | by:leave name:{currentAttackStateName} norm:{st.normalizedTime:F2}");
                isAttacking = false;
                attackLock = false;
                currentAttackStateName = null;
                anim.speed = 1f;
                attackDurationFromStateSet = false;
            }
            if (isAttacking && isAttackState && !attackDurationFromStateSet)
            {
                attackEndTime = Time.time + (st.length / Mathf.Max(0.0001f, anim.speed));
                attackDurationFromStateSet = true;
                if (debugAttack) Debug.Log($"Attack Duration | from state length:{st.length:F2} speed:{anim.speed:F2}");
            }
            if (isAttacking && ((isAttackState && st.normalizedTime >= 1f) || Time.time >= attackEndTime))
            {
                if (debugAttack) Debug.Log($"Attack End | by:{(isAttackState && st.normalizedTime >= 1f ? "state" : "timeout")} name:{currentAttackStateName} norm:{st.normalizedTime:F2}");
                isAttacking = false;
                attackLock = false;
                currentAttackStateName = null;
                anim.speed = 1f;
                attackDurationFromStateSet = false;
            }

            if (isSliding)
            {
                if (isSlideState && st.normalizedTime >= 1f)
                {
                    if (debugSlide) Debug.Log($"Slide End | by:state norm:{st.normalizedTime:F2}");
                    isSliding = false;
                    slideLock = false;
                    slideEndTime = 0f;
                }
                else if (!isSlideState || Time.time >= slideEndTime)
                {
                    if (debugSlide) Debug.Log($"Slide End | by:{(!isSlideState ? "leave" : "timeout")} norm:{st.normalizedTime:F2}");
                    isSliding = false;
                    slideLock = false;
                    slideEndTime = 0f;
                }
            }
        }

    }

    void FixedUpdate()
    {
        float targetSpeed = (isGrounded || runFullSpeedInAir) ? speed : speed * airControlFactor;
        float moveH;
        if (attackLock)
        {
            moveH = 0f;
        }
        else if (isSliding)
        {
            moveH = slideDirection * slideBoost;
        }
        else
        {
            moveH = moveX * targetSpeed;
        }
        rb.linearVelocity = new Vector2(moveH, rb.linearVelocity.y);
        
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        
        jumpPressed = false;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
    }

    void StartAttack(string stateName)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
        {
            if (debugAttack) Debug.Log("StartAttack abort | Animator o Controller nulo");
            attackLock = false;
            isAttacking = false;
            currentAttackStateName = null;
            return;
        }
        attackLock = true;
        isAttacking = true;
        anim.speed = attackSpeedMultiplier;
        if (TryCrossFade(stateName))
        {
            currentAttackStateName = stateName;
            attackEndTime = Time.time + attackMaxDuration;
            if (debugAttack) Debug.Log($"CrossFade OK | state:{stateName}");
            attackDurationFromStateSet = false;
            PlayAttackSound(stateName);
            if (attackDamageOnStart) ApplyMeleeDamage();
            return;
        }
        string trigger = stateName == attackLeftStateName ? "AttackLeft" : "AttackRight";
        if (HasAnimatorParameter(trigger))
        {
            anim.ResetTrigger(trigger);
            anim.SetTrigger(trigger);
            attackEndTime = Time.time + attackMaxDuration;
            currentAttackStateName = stateName;
            if (debugAttack) Debug.Log($"Trigger OK | trigger:{trigger} -> state:{stateName}");
            attackDurationFromStateSet = false;
            PlayAttackSound(stateName);
            if (attackDamageOnStart) ApplyMeleeDamage();
        }
        else
        {
            if (debugAttack) Debug.Log($"StartAttack FAIL | missing trigger:{trigger} and state:{stateName}");
            attackLock = false;
            isAttacking = false;
            currentAttackStateName = null;
            anim.speed = 1f;
            attackDurationFromStateSet = false;
        }
    }

    void ApplyMeleeDamage()
    {
        Vector3 origin = transform.position + new Vector3((sr != null && sr.flipX) ? -attackOffset.x : attackOffset.x, attackOffset.y);
        var hits = Physics2D.OverlapBoxAll(origin, attackBoxSize, 0f);
        if (hits == null || hits.Length == 0) return;
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h == null) continue;
            if (h.gameObject == gameObject) continue;
            var es = h.GetComponent<EnemySimple>();
            if (es != null) es.TakeDamage(attackDamage);
        }
    }

    bool TryCrossFade(string stateName)
    {
        if (anim.runtimeAnimatorController == null || anim.layerCount <= 0) return false;
        int hash = Animator.StringToHash(stateName);
        if (anim.HasState(0, hash))
        {
            anim.CrossFadeInFixedTime(stateName, 0f);
            return true;
        }
        if (debugAttack) Debug.Log($"CrossFade FAIL | state not found:{stateName}");
        return false;
    }

    void StartSlide(int dir)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
        {
            slideLock = false;
            isSliding = false;
            slideEndTime = 0f;
            return;
        }
        slideDirection = dir;
        slideLock = true;
        isSliding = true;
        slideEndTime = Time.time + slideDuration;
        float boosted = Mathf.Max(slideSpeed, Mathf.Abs(rb.linearVelocity.x) * slideSpeedMultiplier);
        slideBoost = boosted;
        rb.linearVelocity = new Vector2(slideDirection * slideBoost, rb.linearVelocity.y);
        PlaySlideSound();
        if (TryCrossFade(slideStateName))
        {
            if (debugSlide) Debug.Log($"Slide CrossFade | state:{slideStateName} dir:{dir}");
            return;
        }
        if (HasAnimatorParameter(slideTrigger))
        {
            anim.ResetTrigger(slideTrigger);
            anim.SetTrigger(slideTrigger);
            if (debugSlide) Debug.Log($"Slide Trigger | trigger:{slideTrigger} dir:{dir}");
        }
        else
        {
            if (debugSlide) Debug.Log("Slide FAIL | missing state and trigger");
            isSliding = false;
            slideLock = false;
            slideEndTime = 0f;
        }
    }

    bool HasAnimatorParameter(string param)
    {
        if (anim == null) return false;
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].name == param) return true;
        }
        return false;
    }

    void PlayAttackSound(string stateName)
    {
        if (sfxSource == null) return;
        AudioClip clip = null;
        if (stateName == attackLeftStateName) clip = sfxAttack0;
        else if (stateName == attackRightStateName) clip = sfxAttackAlt;
        if (clip != null) sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume));
    }

    void PlaySlideSound()
    {
        if (sfxSource == null) return;
        if (sfxSlide != null) sfxSource.PlayOneShot(sfxSlide, Mathf.Clamp01(sfxSlideVolume));
    }

    // Método para debug (opcional)
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        
        GUI.Label(new Rect(10, 10, 300, 30), $"Tiempo desde último salto: {Time.time - lastJumpTime:F2}", style);
        GUI.Label(new Rect(10, 40, 300, 30), $"Puede saltar: {(Time.time - lastJumpTime) >= jumpDuration}", style);
        GUI.Label(new Rect(10, 70, 300, 30), $"En suelo: {isGrounded}", style);
        GUI.Label(new Rect(10, 100, 380, 30), $"Ataque: lock={attackLock} atk={isAttacking} state={currentAttackStateName}", style);
        GUI.Label(new Rect(10, 130, 380, 30), $"Animator: {(anim!=null)} ctrl:{(anim!=null && anim.runtimeAnimatorController!=null)}", style);
    }
}
