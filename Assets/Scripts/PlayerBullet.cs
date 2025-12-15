using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;

    Vector2 dir;

    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);
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
