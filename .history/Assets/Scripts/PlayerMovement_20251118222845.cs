using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float jumpDuration = 1.0f; // Duración máxima del salto
    public float attackDuration = 0.35f;
    public float attackCooldown = 0.15f;
    public float attackSpeedMultiplier = 1.6f;
    public string attackStateName = "Attack";

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpLock;
    private float lastJumpTime; // Tiempo del último salto
    private bool isJumping; // Si está actualmente saltando
    private bool attackLock;
    private float lastAttackTime;
    private bool isAttacking;
    
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public float groundCheckDistance = 0.2f;
    public float airControlFactor = 0.7f;

    private float moveX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
        bool attackDown = Input.GetMouseButtonDown(0);
        
        // Detectar si está en el suelo
        CheckGrounded();

        // Verificar si ha pasado el tiempo suficiente desde el último salto
        bool canJump = (Time.time - lastJumpTime) >= jumpDuration;

        // Lógica de ataque
        if (attackDown && !attackLock && !isAttacking)
        {
            StartAttack();
        }

        // Lógica de salto mejorada
        if (jumpDown && isGrounded && !jumpLock && canJump && !isAttacking)
        {
            jumpPressed = true;
            lastJumpTime = Time.time; // Registrar el tiempo del salto
            isJumping = true;
        }

        // Resetear el bloqueo cuando toca el suelo y no está presionando salto
        if (isGrounded && !Input.GetButton("Jump")) 
        {
            jumpLock = false;
            isJumping = false;
        }

        // Si no está en el suelo, activar bloqueo para prevenir doble salto
        if (!isGrounded) 
        {
            jumpLock = true;
        }

        // Actualizar animaciones
        UpdateAnimations();

        // Verificar fin del ataque
        CheckAttackEnd();
    }

    void CheckGrounded()
    {
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
    }

    void StartAttack()
    {
        attackLock = true;
        isAttacking = true;
        lastAttackTime = Time.time;
        anim.speed = attackSpeedMultiplier;
        
        if (HasAnimatorParameter("Attack"))
        {
            anim.SetTrigger("Attack");
        }
        else if (!string.IsNullOrEmpty(attackStateName))
        {
            anim.Play(attackStateName, 0, 0f);
        }
    }

    void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("isJumping", !isGrounded);
    }

    void CheckAttackEnd()
    {
        if (isAttacking)
        {
            // Verificar por tiempo en lugar de normalizedTime (más confiable)
            if (Time.time - lastAttackTime >= attackDuration)
            {
                EndAttack();
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        anim.speed = 1f;
        
        // Usar cooldown antes de poder atacar de nuevo
        if (Time.time - lastAttackTime >= attackDuration + attackCooldown)
        {
            attackLock = false;
        }
        else
        {
            // Programar el desbloqueo para después del cooldown
            Invoke("UnlockAttack", attackCooldown);
        }
    }

    void UnlockAttack()
    {
        attackLock = false;
    }

    void FixedUpdate()
    {
        // Movimiento horizontal (reducido durante ataque)
        float currentSpeed = isGrounded ? speed : speed * airControlFactor;
        float moveH = moveX * currentSpeed;
        
        if (isAttacking)
        {
            // Reducir velocidad durante el ataque pero no detener completamente
            moveH *= 0.3f;
        }
        
        rb.linearVelocity = new Vector2(moveH, rb.linearVelocity.y);

        // Aplicar salto
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        jumpPressed = false;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
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
        GUI.Label(new Rect(10, 100, 300, 30), $"Atacando: {isAttacking}", style);
        GUI.Label(new Rect(10, 130, 300, 30), $"Attack Lock: {attackLock}", style);
    }
}