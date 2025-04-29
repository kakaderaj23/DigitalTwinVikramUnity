using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FPSUIClicker : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayDistance = 10f;
    public LayerMask uiLayerMask; // Create a "UI" Layer and assign

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRaycast();
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, uiLayerMask))
        {
            // Optional: you can highlight button on hover if you want

            if (Input.GetMouseButtonDown(0)) // Left click
            {
                Button button = hit.collider.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke(); // Simulate button click
                }
            }
        }
    }
}
