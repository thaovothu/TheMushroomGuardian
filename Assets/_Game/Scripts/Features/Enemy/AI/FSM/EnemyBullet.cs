using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 3f;
    public float trackingDuration = 0.5f; // Thời gian dí theo player

    private Rigidbody rb;
    private Transform target;
    private float trackingTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetTarget(Transform player)
    {
        target = player;
    }

    private void Start()
    {
        trackingTimer = trackingDuration;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (trackingTimer > 0f && target != null)
        {
            // Giai đoạn dí theo player
            trackingTimer -= Time.fixedDeltaTime;
            Vector3 direction = (target.position + Vector3.up - transform.position).normalized;
            rb.velocity = direction * speed;
        }
        // Hết trackingDuration → velocity giữ nguyên → bay thẳng
    }

    private void OnTriggerEnter(Collider other)
    {   
        Debug.Log("hihihihi" + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}