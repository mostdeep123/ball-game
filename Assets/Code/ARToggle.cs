using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARToggle : MonoBehaviour
{
    public Camera regularCamera;  // Assign Main Camera (Regular Mode)
    public GameObject arSessionOrigin; // Assign AR Session Origin (AR Mode)
    public Button toggleButton;  // Assign UI Button

    private bool isARActive = false; // Default: Regular Mode


    // Start is called before the first frame update
    void Start()
    {
        // Set initial state
        regularCamera.gameObject.SetActive(true);
        arSessionOrigin.SetActive(false);

        // Add listener for button click
        toggleButton.onClick.AddListener(ToggleCameraMode);
    }

    void ToggleCameraMode()
    {
        isARActive = !isARActive; // Toggle state

        // Enable/Disable cameras accordingly
        regularCamera.gameObject.SetActive(!isARActive);
        arSessionOrigin.SetActive(isARActive);
    }

}
