using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    [Header("Detection Parameters")]
    [SerializeField] private float viewDistance = 25f;
    [SerializeField] private float viewAngle= 60f;
    [SerializeField] private float rateOfFire = 1f;
    [SerializeField] public Transform eyePoint;
    
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

    [Header("Scanning")]
    [SerializeField] private Transform head;
    [SerializeField] private float scanAngle = 45f;
    [SerializeField] private float scanSpeed = 2f;

    private float scanTimer;
    private Quaternion baseHeadRotation;

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

        if (head != null)
        {
            baseHeadRotation = head.localRotation;
        }
    }



    void Update()
    {
        HandleDetection();
        HandleScanning();
        HandleAiming();
        HandleShoot();
        HandleAnimate();

        Debug.DrawRay(eyePoint.position, (player != null ? (player.position - eyePoint.position).normalized : transform.forward) * 5f, Color.green);
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

        if (Physics.Raycast(eyePoint.position, directionToPlayer.normalized, out hit, viewDistance))
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
    private void HandleScanning()
    {
        if (playerDetected || head == null) return;

        scanTimer += Time.deltaTime * scanSpeed;

        float angle = Mathf.PingPong(scanTimer, scanAngle * 2f) - scanAngle;

        head.localRotation = baseHeadRotation * Quaternion.Euler(0f, angle, 0f);
    }
    private void HandleAiming()
    {
        if (playerDetected || head == null || player  == null) return; 

        Vector3 direction = player.position - head.position;
        Quaternion tragetRotation = Quaternion.LookRotation(direction);

        head.rotation = Quaternion.Slerp(head.rotation, tragetRotation, Time.deltaTime * 5f);
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

        Vector3 direction = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Initailize(direction, false);
        }
    }

    private void HandleAnimate()
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Transform origin = eyePoint != null ? eyePoint : transform;

        Vector3 forward = transform.forward;

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(origin.position, rightBoundary * viewDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin.position, forward * viewDistance);
    }
}
