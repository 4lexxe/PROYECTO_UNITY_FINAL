using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float jumpDuration = 1.0f; // Duración máxima del salto
    public float attackSpeedMultiplier = 1.6f;
    public string attackLeftStateName = "attack_0";
    public string attackRightStateName = "attack_2";

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
            StartAttack(attackLeftStateName);
        }
        if (rightDown && !attackLock)
        {
            StartAttack(attackRightStateName);
        }

        if (anim != null)
        {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            if (isAttacking && !string.IsNullOrEmpty(currentAttackStateName) && st.IsName(currentAttackStateName) && st.normalizedTime >= 1f)
            {
                isAttacking = false;
                attackLock = false;
                currentAttackStateName = null;
                anim.speed = 1f;
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
        if (anim == null)
        {
            attackLock = false;
            isAttacking = false;
            currentAttackStateName = null;
            return;
        }
        attackLock = true;
        isAttacking = true;
        currentAttackStateName = stateName;
        anim.speed = attackSpeedMultiplier;
        anim.CrossFadeInFixedTime(stateName, 0f);
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
    }
}
