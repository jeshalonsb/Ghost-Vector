using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDamage : MonoBehaviour
{
    [Header("Gun Properties")]
    public float Damage;
    public float BulletRange;
    private Transform PlayerCamera;

    private void Start()
    {
        PlayerCamera = Camera.main.transform;
    }

    public void Shoot()
    {
        Ray gunRay = new Ray(PlayerCamera.position, PlayerCamera.forward);
        if (Physics.Raycast(gunRay, out RaycastHit hitInfo, BulletRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.Health -= Damage;
            }
        }
    }
}
