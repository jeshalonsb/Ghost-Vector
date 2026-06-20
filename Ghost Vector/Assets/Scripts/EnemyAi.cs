using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    [Header("Detection Parameters")]
    [SerializeField] private float viewDistance = 25f;
    [SerializeField] private float viewAngle= 60f;
    [SerializeField] private float rateOfFire = 1f;
    [SerializeField] public Transform eyePoint;
    
    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private Transform firePoint;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLifetime = 3f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Scanning")]
    [SerializeField] private Transform head;
    [SerializeField] private float leftScanLimit = -60f;
    [SerializeField] private float rightScanLimit = 60f;
    [SerializeField] private float scanSpeed = 45f;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip Gunshot;
    [SerializeField] AudioClip detectionSound;

    private Transform player;

    private bool playerDetected;
    private bool detectionSoundPlayed;

    private float shootTimer;

    private Quaternion baseHeadRotation;

    private float currentScanAngle;
    private bool scanningRight = true;

    private bool isDead;


    private void Start()
    {
        animator = GetComponent<Animator>();
        
        GameObject playerobj = GameObject.FindGameObjectWithTag("Player");

        if (playerobj != null)
        {
            player = playerobj.transform;
        }

        shootTimer = rateOfFire;

        if (head != null)
        {
            baseHeadRotation = head.localRotation;
            currentScanAngle = leftScanLimit;
        }
    }



    void Update()
    {
        if (isDead) return;
        
        HandleDetection();
        HandleScanning();
        HandleAiming();
        HandleShoot();
        HandleAnimate();

        if (eyePoint != null && head != null)
        {
            Debug.DrawRay(eyePoint.position, head.forward * viewDistance, playerDetected ? Color.red : Color.green);
        }
    }

    private void HandleDetection()
    {
        if (player == null || eyePoint == null || head == null)
            return;

        playerDetected = false;

        Vector3 directionToPlayer = player.position - eyePoint.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)                                    //initial detection
        {
            detectionSoundPlayed = false;
                return;
        }
        
        float angle = Vector3.Angle(head.forward, directionToPlayer.normalized);      //check FOV

        if (angle > viewAngle * 0.5f)
        {
            detectionSoundPlayed = false;
                return;
        }

        if (Physics.Raycast(eyePoint.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer))
        {
            Debug.Log("Hit: " + hit.transform.name);
            Debug.Log("Tag: " + hit.transform.tag);

            if (hit.transform.CompareTag("Player") ||
                hit.transform.root.CompareTag("Player"))
            {
                Debug.Log("PLAYER DETECTED");
                playerDetected = true;
            }
            if (hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
            {
                playerDetected = true;

                if (!detectionSoundPlayed)
                {
                    if (detectionSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(detectionSound);
                    }

                    detectionSoundPlayed = true;
                }
            }
        }
    }
    private void HandleScanning()
    {
        if (playerDetected) return;
        if (head == null) return;

        if (scanningRight)
        {
            currentScanAngle += scanSpeed * Time.deltaTime;

            if (currentScanAngle >= rightScanLimit)
            {
                currentScanAngle = rightScanLimit;
                scanningRight = false;
            }
        }
        else
        {
            currentScanAngle -= scanSpeed * Time.deltaTime;

            if(currentScanAngle <= leftScanLimit)
            {
                currentScanAngle = leftScanLimit;
                scanningRight = true;
            }
        }

        head.localRotation = baseHeadRotation * Quaternion.Euler(0f, currentScanAngle, 0f);
    }
    private void HandleAiming()
    {
        if (!playerDetected) return;

        if(player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        head.rotation = Quaternion.RotateTowards(head.rotation, targetRotation, scanSpeed * 4f * Time.deltaTime );

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
        if (player == null || firePoint == null || bulletPrefab == null) return;

        if (Gunshot != null)
        {
            audioSource.PlayOneShot(Gunshot);
        }

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

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
        if (animator == null) return;

        animator.SetBool("isPlayerDetected", playerDetected);
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null || head == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * head.forward;

        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle * 0.5f, 0) * head.forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(eyePoint.position, leftBoundary * viewDistance);

        Gizmos.DrawRay(eyePoint.position, rightBoundary * viewDistance);

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(eyePoint.position, head.forward * viewDistance);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        playerDetected = false;
        detectionSoundPlayed = false;

        if (OnScreenUI.Instance != null)
        {
            OnScreenUI.Instance.EnemyKilled();
        }

        if (animator  != null)
        {
            animator.SetBool("isPlayerDetected", false);
            animator.ResetTrigger("Shoot");
            animator.SetTrigger("Die");
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider col  in colliders)
        {
            col.enabled = false;
        }

    }
}
