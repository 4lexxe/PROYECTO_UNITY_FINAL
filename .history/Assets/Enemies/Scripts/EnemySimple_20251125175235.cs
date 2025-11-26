using UnityEngine;

public class EnemySimple : MonoBehaviour
{
    public float speed = 2f;
    public float aggroRange = 12f;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.0f;
    public string attackTriggerName = "Attack";
    public string speedParamName = "Speed";
    private Transform player;
    private Animator anim;
    private SpriteRenderer sr;
    private float lastAttack;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        sr = GetComponent<SpriteRenderer>();
        lastAttack = -attackCooldown;
    }

    void Update()
    {
        if (player == null) return;
        Vector3 self = transform.position;
        Vector3 target = new Vector3(player.position.x, self.y, self.z);
        float dist = Vector2.Distance(self, player.position);
        if (dist > aggroRange)
        {
            if (anim != null) anim.SetFloat(speedParamName, 0f);
            return;
        }
        if (dist > attackRange)
        {
            float dir = Mathf.Sign(target.x - self.x);
            if (sr != null) sr.flipX = dir < 0f;
            transform.position = Vector3.MoveTowards(self, new Vector3(target.x, self.y, self.z), speed * Time.deltaTime);
            if (anim != null) anim.SetFloat(speedParamName, Mathf.Abs(dir) * speed);
            return;
        }
        if (Time.time - lastAttack >= attackCooldown)
        {
            if (anim != null) anim.SetTrigger(attackTriggerName);
            lastAttack = Time.time;
            if (anim != null) anim.SetFloat(speedParamName, 0f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null) return;
        if (other.CompareTag("Player"))
        {
            if (Time.time - lastAttack >= attackCooldown)
            {
                if (anim != null) anim.SetTrigger(attackTriggerName);
                lastAttack = Time.time;
            }
        }
    }
}
