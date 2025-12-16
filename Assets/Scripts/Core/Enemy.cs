using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static int enemiesKilled = 0;

    [Header("Statistiche")]
    public int hp = 2;
    public float speed = 2f;
    public int xpReward = 1;

    [Header("Opzionale - Effetti")]
    public GameObject deathEffectPrefab;

    private Transform player;
    private Rigidbody2D rb;
    private bool isDead = false;

    // Event per notificare la morte
    public System.Action OnDeath;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Enemy richiede un Rigidbody2D!");
            enabled = false;
            return;
        }

        // Cache del player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player non trovato! Assicurati che abbia il tag 'Player'");
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        // Movimento verso il player
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Incrementa statistiche globali
        enemiesKilled++;

        // Dai XP al player
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.GainXP(xpReward);
            }
        }

        // Notifica death event PRIMA di distruggere l'oggetto
        OnDeath?.Invoke();

        // Effetto morte opzionale
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Distruggi nemico
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage();
            }

            // Il nemico si sacrifica colpendo il player
            Die();
        }
    }

    // Cleanup per evitare memory leak
    void OnDestroy()
    {
        // Rimuovi tutti i subscriber dall'evento
        OnDeath = null;
    }
}