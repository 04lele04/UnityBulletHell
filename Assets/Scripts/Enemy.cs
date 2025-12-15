using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static int enemiesKilled = 0;

    public int hp = 2;
    public float speed = 2f;
    public int xpReward = 1;

    Transform player;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 dir =
            ((Vector2)player.position - (Vector2)transform.position).normalized;

        rb.velocity = dir * speed;
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;

        if (hp <= 0)
        {
            enemiesKilled++;
            Debug.Log($"ENEMIES KILLED: {enemiesKilled}");

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.GainXP(xpReward);

            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerController>().TakeDamage();
            Destroy(gameObject);
        }
    }
}
