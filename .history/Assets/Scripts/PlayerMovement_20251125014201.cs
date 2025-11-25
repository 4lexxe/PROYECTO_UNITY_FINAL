using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float jumpDuration = 1.0f; // Duración máxima del salto
    public float attackSpeedMultiplier = 1.6f;
    public string attackLeftStateName = "attack_0";
    public string attackRightStateName = "attack_2";
    public bool debugAttack = true;

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
    public float groundCheckDistance = 0.2f;
    public float airControlFactor = 0.7f;

    private float moveX;
    private bool attackLock;
    private bool isAttacking;
    private string currentAttackStateName;
    private float attackEndTime;
    public float attackMaxDuration = 0.6f;
    private bool attackDurationFromStateSet;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
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
        
        // Detectar si está en el suelo
        Vector3 gcPos = groundCheck != null ? groundCheck.position : transform.position;
        var hit = Physics2D.Raycast(gcPos, Vector2.down, groundCheckDistance, groundLayer);
        if (col != null)
        {
            isGrounded = col.IsTouchingLayers(groundLayer);
            if (!isGrounded)
            {
                isGrounded = hit.collider != null || Physics2D.OverlapCircle(gcPos, groundCheckRadius, groundLayer);
            }
        }
        else
        {
            isGrounded = hit.collider != null || Physics2D.OverlapCircle(gcPos, groundCheckRadius, groundLayer);
        }

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
            if (debugAttack) Debug.Log($"Click Left | state:{attackLeftStateName} anim:{(anim!=null)} ctrl:{(anim!=null && anim.runtimeAnimatorController!=null)} paramLeft:{HasAnimatorParameter("AttackLeft")}");
            StartAttack(attackLeftStateName);
        }
        if (rightDown && !attackLock)
        {
            if (debugAttack) Debug.Log($"Click Right | state:{attackRightStateName} anim:{(anim!=null)} ctrl:{(anim!=null && anim.runtimeAnimatorController!=null)} paramRight:{HasAnimatorParameter("AttackRight")}");
            StartAttack(attackRightStateName);
        }

        if (anim != null)
        {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            bool isAttackState = st.IsName(attackLeftStateName) || st.IsName(attackRightStateName) || (!string.IsNullOrEmpty(currentAttackStateName) && st.IsName(currentAttackStateName));
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
        }

    }

    void FixedUpdate()
    {
        float targetSpeed = isGrounded ? speed : speed * airControlFactor;
        float moveH = attackLock ? 0f : moveX * targetSpeed;
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
