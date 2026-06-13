using UnityEngine.Events;
using UnityEngine;
using Unity.VisualScripting;

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

    public bool IsADSing => Input.GetMouseButton(1);

    private Camera playerCamera;

    void Start()
    {
        CurrentCooldown = FireCooldown;

        playerCamera = Camera.main;
        playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleInput();
        HandleADS(); 
        CurrentCooldown -= Time.deltaTime;

        Vector3 targetPosition = Input.GetMouseButton(1) ? adsPosition : hipPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, adsMoveSpeed *  Time.deltaTime);
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
        if (CurrentCooldown > 0f)
            return;

        Shoot();

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
}
