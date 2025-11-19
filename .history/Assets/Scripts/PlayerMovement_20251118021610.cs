using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
   void Startt  sr void Update()
    {
        // Movimiento horizontal
        moveX = Input.GetAxisRaw("Horizontal");

        // Flip
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = // Movitientr horizontal
        moue;

        // Salto sin checkear sue
lo
     // Fl p
        i  if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocit
y = new // Salto sVn checkear suelo
        iector2(rb.liKeylocitKeyCode.S aceum
p       { anim.Setmi  rb.l n   Ve
  itynmacew VRutor2(rb
linearVel c  y.x, jumpF rce);
          aiSeiatSetBo"l("psJumpe"g", tahs);
        }

        // AnimvcX }es de I
l /Runid FixedUpdate()
    {
        rb.linearVelocit y);
    }
}
