using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float jumpDuration = 1.0f; // Duración máxima del salto

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer   private bool jumpLock;
    private float lastJumpTime; // Tiempo del último salto
    private bool isJumping; // Si está actualmente saltando
    
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public float groundCheckDistance = 0.2f;
    public float airControlFactor = 0Pressed;
    private bool attack.7f;

    private float moveX;

    voidta  etComt<Rigidbody2D>();
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
        bool jumpUp = Input.GetButtonUp("Jump");
        
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
       isGrounded &&  }

        && (Time.time - lastAttackTime  >= attackCooldown)anim.SetFloat("Speed", Mathf.PX)sSedsJumping", !isGrounded);
    }

    void FixedUpdate()
  o .locity =sed = false;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
    }

    // Método para debug (opcional)
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        
        GUI.Label(new Rect(10, 10, 300, 30), $"Tiempo desde último salto: {Time.time - lastJumpTime:F2}", style);
        GUI.Label(new Rect(10, 40, 300, 30), $"Puede "  tackLock = true;
                isAttacking = true;
                anim.speed = attackSpeedMultiplier;
                anim.SetTrigger("Attack");
            }
            else
            {
                attackLock = false;
            }
        }

        var st = anim.GetCurrentAnimatorStateInfo(0);
        if (isAttacking && (st.IsName(attackStateName) && st.normalizedTime >= 1f))
        {
            isAttacking = false;
            attackLock = false;
            anim.speed = 1f;
        }
        else if (isAttac&T= aalse;
            attackLock = false;
            anim.speed = 1f;
        }

        Pressed = false;
        attack