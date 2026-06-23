using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    [Header("Detection Parameters")]
    [SerializeField] private float viewDistance = 25f;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private float rateOfFire = 1f;
    [SerializeField] public Transform eyePoint;
    [SerializeField] LayerMask detectionMask = ~0;

    [Header("Detection Tuning")]
    [SerializeField] private float detectionAngle = 85f;
    [SerializeField] private float detectionSphereRadius = 0.35f;
    [SerializeField] private float shootAimAngle = 12f;
    
    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private Transform firePoint;

    [Header("Vision Cone")]
    [SerializeField] private bool showVisionCone = true;
    [SerializeField] Material visionConeMaterial;
    [SerializeField] int coneResolution = 30;
    [SerializeField] float coneHeightOffset = -0.05f;

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
    [SerializeField] private float turnSpeed = 360f;

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

    private Mesh visionMesh;
    private MeshFilter visionMeshFilter;
    private MeshRenderer visionMeshRenderer;


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

        CreateVisionCone();
    }



    void Update()
    {
        if (isDead) return;
        
        HandleDetection();
        HandleScanning();
        HandleAiming();
        HandleShoot();
        HandleAnimate();
        UpdateVisionCone();

    }

    private void HandleDetection()
    {
        if (player == null || eyePoint == null || head == null)
            return;

        playerDetected = false;

        Vector3 targetPoint = player.position + Vector3.up * 0.6f;
        Vector3 directionToPlayer = targetPoint - eyePoint.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
        {
            detectionSoundPlayed = false;
            return;
        }

        float angle = Vector3.Angle(head.forward, directionToPlayer.normalized);

        if (angle > viewAngle * 0.5f)
        {
            detectionSoundPlayed = false;
            return;
        }

        RaycastHit[] hits = Physics.SphereCastAll(eyePoint.position, detectionSphereRadius, directionToPlayer.normalized, viewDistance, detectionMask, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        foreach (RaycastHit hit in hits) 
        {
            if (hit.transform.root == transform) continue;
            
            if (hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
            {
                playerDetected = true;

                if (!detectionSound && detectionSound != null && audioSource != null)
                        audioSource.PlayOneShot(detectionSound);

                detectionSoundPlayed = true;
                return;

            }
                         
            detectionSoundPlayed = false;
            return;
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
        if (!playerDetected || player == null) return;

        Vector3 bodyDirection = player.position - transform.position;
        bodyDirection.y = 0f;

       if (bodyDirection != Vector3.zero)
        {
            Quaternion bodyRotation = Quaternion.LookRotation(bodyDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, bodyRotation, turnSpeed * Time.deltaTime);
        }

       Vector3 headDirection = player.position + Vector3.up * 1f - head.position;

       if (headDirection != Vector3.zero)
        {
            Quaternion headRotation = Quaternion.LookRotation(headDirection);
            head.rotation = Quaternion.RotateTowards(head.rotation, headRotation, turnSpeed * Time.deltaTime);
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
        if (player == null || firePoint == null || bulletPrefab == null) return;

        if (Gunshot != null && audioSource != null)
            audioSource.PlayOneShot(Gunshot);

        if (animator != null)
            animator.SetTrigger("Shoot");

        Vector3 direction = firePoint.forward;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
            bulletScript.Initailize(direction, false);

        Destroy(bullet, bulletLifetime);

    }

    private void HandleAnimate()
    {
        if (animator == null) return;

        animator.SetBool("isPlayerDetected", playerDetected);
    }

    private void CreateVisionCone()
    {
        if (!showVisionCone || eyePoint == null) return;

        GameObject coneObj = new GameObject("Vision Cone");
        coneObj.transform.SetParent(eyePoint, false);
        coneObj.transform.localPosition = new Vector3(0f, coneHeightOffset, 0f);
        coneObj.transform.localRotation = Quaternion.identity;

        visionMeshFilter = coneObj.AddComponent<MeshFilter>();
        visionMeshRenderer = coneObj.AddComponent<MeshRenderer>();

        visionMesh = new Mesh();
        visionMesh.name = "Enemy Vision Cone";
        visionMeshFilter.mesh = visionMesh;

        if (visionConeMaterial != null)
            visionMeshRenderer.material = visionConeMaterial;
    }

    private void UpdateVisionCone()
    {
        if (!showVisionCone || visionMesh == null || eyePoint == null) return;

        int circlePoints = coneResolution;
        
        Vector3[] vertices = new Vector3[coneResolution + 2];
        int[] triangles = new int[coneResolution * 3];

        vertices[0] = Vector3.zero;

        float radius = Mathf.Tan(viewAngle * 0.5f * Mathf.Deg2Rad) * viewDistance;

        for (int i = 0; i < circlePoints; i++)
        {
            float angle = ((float)i / circlePoints) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            vertices[i + 1] = new Vector3(x, y, viewDistance);
        }

        vertices [circlePoints + 1] = Vector3.zero;

        for (int i = 0; i < coneResolution; i++)
        {
            int next = i + 1;

            if (next >= circlePoints)
                next = 0;
            
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = next + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();

        if (visionMeshRenderer != null)
        {
            visionMeshRenderer.enabled = showVisionCone; 
        }
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

        if (visionMeshRenderer != null)
        {
            visionMeshRenderer.gameObject.SetActive(false);
        }

    }
}
