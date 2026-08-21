using UnityEngine;
using TMPro;

public class GunFlashlight : MonoBehaviour
{
    [Header("Flashlight")]
    [SerializeField] private GameObject flashlightObject;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    private bool flashlightOn = false;

    private void Start()
    {
        flashlightOn = false;

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            flashlightOn = !flashlightOn;
            flashlightObject.SetActive(flashlightOn);

            Debug.Log("Flashlight: " + flashlightOn);
        }
    }
}