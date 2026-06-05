using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    [Header("Detection Parameters")]
    [SerializeField] private float viewDistance = 25f;
    [SerializeField] private float viewAngle= 60f;
    [SerializeField] private float rateOfFire = 1f;
    private float shootTimer;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private Transform firePoint;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLifetime = 3f;
    
    [Header("Animation")]
    public Animation RagDollEffect;

    [Header("Audio")]
    [SerializeField] AudioSource AudioSource;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip Gunshot;
    [SerializeField] AudioClip detectionSound;

    private Transform player;
    private bool playerDetected;
    private bool detectionSoundPlayed;

    private void Start()
    {
        GameObject playerobj = GameObject.FindGameObjectWithTag("Player");

        if (playerobj != null)
        {
            player = playerobj.transform;
        }

        shootTimer = rateOfFire;
    }



    void Update()
    {
        HandleDetection();
        HandleShoot();
        HandleAnimate();
    }

    private void HandleDetection()
    {
        if (player == null)
            return;

        playerDetected = false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)                                    //initial detection
        {
            detectionSoundPlayed = false;
                return;
        }
        
        float angle = Vector3.Angle(transform.forward, directionToPlayer);      //check FOV

        if (angle > viewAngle * 0.5f)
        {
            detectionSoundPlayed = false;
                return;
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, viewDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                playerDetected = true;

                if (!detectionSoundPlayed)
                {
                    AudioSource.PlayOneShot(detectionSound);
                    detectionSoundPlayed = true;
                }
            }
        }
    }

    private void HandleShoot()
    {
        if (!playerDetected) return;

        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = rateOfFire;
        }
    }

    private void Shoot()
    {
        AudioSource.PlayOneShot(Gunshot);

        if (bulletPrefab != null && firePoint != null)
        {
            Vector3 direction = (player.position - firePoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if ( rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }

            Destroy(bullet, bulletLifetime);
        }
    }

    private void HandleAnimate()
    {

    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;

        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);

        Gizmos.color = Color.blue; 
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

}
