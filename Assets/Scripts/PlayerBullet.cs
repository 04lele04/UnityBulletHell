using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    Vector2 dir;

    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        
        // Ruota il proiettile nella direzione di movimento
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        // Usa transform.right invece di dir perché il proiettile è già ruotato
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            col.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}