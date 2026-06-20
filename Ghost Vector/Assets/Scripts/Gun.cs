using UnityEngine.Events;
using UnityEngine;
using Unity.VisualScripting;
using TMPro;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnGunShoot;

    [Header("Gun Settings")]
    public float FireCooldown;
    public bool Automatic;

    private float CurrentCooldown;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletLifetime = 3f;

    [Header("ADS")]
    public float normalFOV = 60f;
    public float adsFOV = 35f;
    public float zoomSpeed = 1.0f;
    public Vector3 hipPosition;
    public Vector3 adsPosition;
    public float adsMoveSpeed = 10f;

    [Header("Slide")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private Vector3 slidePosition;
    [SerializeField] private Vector3 slideRotation;
    [SerializeField] private float slideRotateSpeed = 10f;
    private Quaternion startingRotation;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int currentAmmo;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] KeyCode reloadKey = KeyCode.R;
    private bool isReloading;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    public bool IsADSing => Input.GetMouseButton(1);

    private Camera playerCamera;

    void Start()
    {
        CurrentCooldown = FireCooldown;

        playerCamera = Camera.main;
        playerCamera.fieldOfView = normalFOV;

        startingRotation = transform.localRotation;

        currentAmmo = magazineSize;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            StartCoroutine(Reload());
        }
        HandleInput();
        HandleADS(); 
        CurrentCooldown -= Time.deltaTime;

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (Input.GetMouseButton(1))
        {
            targetPosition = adsPosition;
            targetRotation = startingRotation;
        }
        else if (playerController != null && playerController.IsSliding)
        {
            targetPosition = slidePosition;
            targetRotation = startingRotation * Quaternion.Euler(slideRotation);
        }
        else
        {
            targetPosition = hipPosition;
            targetRotation = startingRotation;
        }
        
        transform.localPosition = Vector3.Lerp( transform.localPosition, targetPosition, adsMoveSpeed *  Time.deltaTime );
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, slideRotateSpeed * Time.deltaTime);
    }
    private void HandleInput()
    {
        if (Automatic)
        {
            if (Input.GetMouseButton(0))
            {
                TryShoot();
            }
        }
        else
        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }
    private void TryShoot()
    {
        if (CurrentCooldown > 0f || isReloading)
            return;
        
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Shoot();
        currentAmmo--;
        UpdateAmmoUI();

        OnGunShoot?.Invoke();

        CurrentCooldown = FireCooldown;
    }

    private void Shoot()
    {
        Camera cam = Camera.main;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(1f);
            }
        }
        else
        {
            targetPoint = cam.transform.position + cam.transform.forward * 100f;
        }

        Vector3 direction = (targetPoint - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Initailize(direction, true);
        }

        Destroy(bullet, bulletLifetime);
    }
    private void HandleADS()
    {
        float targetFOV = Input.GetMouseButton(1) ? adsFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, zoomSpeed *  Time.deltaTime);
    }

    private IEnumerator Reload()
    {
        if (ammoText != null)
        {
            ammoText.text = "RELOADING...";
        }
        
        if (isReloading || currentAmmo == magazineSize)
            yield break;

        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        UpdateAmmoUI();
        isReloading = false;
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + magazineSize;
        }
    }
       
}
