using UnityEngine;
using KSP;
using KSP.UI.Screens; // For ApplicationLauncher

namespace KSPCountdown
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class CustomMainMenuCamera : MonoBehaviour
    {
        // Movement settings for the custom camera
        public float moveSpeed = 25f;
        public float rotationSpeed = 100f;
        public float zoomSpeed = 10f;
        public float minFOV = 5f;
        public float maxFOV = 80f;

        // References for the custom camera and its transform
        private Camera customCamera;
        private Transform camTransform;

        // Reference to the original stock Main Menu camera
        private Camera stockMainMenuCamera;

        // State flag for whether the mod (custom camera) is active
        private bool modEnabled = false;

        // ApplicationLauncher button for the sidebar icon
        private ApplicationLauncherButton appButton;
        private Texture2D iconTexture;
        private bool buttonAdded = false;

        private void Start()
        {
            // Locate the stock Main Menu camera (but leave it active for now)
            stockMainMenuCamera = Camera.main;
            if (stockMainMenuCamera == null && Camera.allCameras.Length > 0)
            {
                stockMainMenuCamera = Camera.allCameras[0];
            }

            // Create a dummy icon texture; replace with your own texture as desired.
            iconTexture = new Texture2D(38, 38);
            Color[] pixels = new Color[38 * 38];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.green; // Green indicates "disabled" (normal) state.
            }
            iconTexture.SetPixels(pixels);
            iconTexture.Apply();
        }

        private void Update()
        {
            // Wait until ApplicationLauncher is available and add the button only once.
            if (!buttonAdded && ApplicationLauncher.Instance != null)
            {
                appButton = ApplicationLauncher.Instance.AddModApplication(
                    OnAppButtonToggleOn,  // Called when toggled on
                    OnAppButtonToggleOff, // Called when toggled off
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.MAINMENU,
                    iconTexture);
                buttonAdded = true;
            }

            // Only process custom camera input when enabled
            if (!modEnabled || customCamera == null) return;

            HandleMovement();
            HandleRotation();
            HandleZoom();
        }

        private void OnDestroy()
        {
            // Clean up the sidebar button when the mod is destroyed
            if (ApplicationLauncher.Instance != null && appButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
            }
        }

        // Called when the sidebar icon is toggled ON (mod enabled)
        private void OnAppButtonToggleOn()
        {
            modEnabled = true;
            Debug.Log("[WasdMainMenu] Custom camera enabled.");

            // Update icon color to red to indicate enabled state
            UpdateIconColor(Color.red);

            // Disable the stock camera if it exists
            if (stockMainMenuCamera != null)
            {
                stockMainMenuCamera.gameObject.SetActive(false);
            }

            // Create the custom camera if it doesn't already exist
            if (customCamera == null)
            {
                CreateCustomCamera();
            }
        }

        // Called when the sidebar icon is toggled OFF (mod disabled)
        private void OnAppButtonToggleOff()
        {
            modEnabled = false;
            Debug.Log("[WasdMainMenu] Custom camera disabled; reverting to stock view.");

            // Update icon color back to green to indicate disabled state
            UpdateIconColor(Color.green);

            // If our custom camera exists, destroy it
            if (customCamera != null)
            {
                Destroy(customCamera.gameObject);
                customCamera = null;
                camTransform = null;
            }

            // Re-enable the stock camera so that the Main Menu returns to normal
            if (stockMainMenuCamera != null)
            {
                stockMainMenuCamera.gameObject.SetActive(true);
            }
        }

        // Helper method to update the icon's color
        private void UpdateIconColor(Color newColor)
        {
            if (iconTexture != null)
            {
                Color[] pixels = iconTexture.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = newColor;
                }
                iconTexture.SetPixels(pixels);
                iconTexture.Apply();
            }
        }

        // Draws the title "WasdMainMenu" on screen when the mod is active
        private void OnGUI()
        {
            if (!modEnabled) return;
            GUI.skin = HighLogic.Skin;
            // Draw title at the top center of the screen
            GUI.Label(new Rect(Screen.width / 2 - 50, 10, 1000, 30), "WasdMainMenu Enabled");
        }

        /// <summary>
        /// Creates a custom camera and positions it similar to the stock Main Menu camera.
        /// </summary>
        private void CreateCustomCamera()
        {
            GameObject camObj = new GameObject("MyCustomMainMenuCam");
            customCamera = camObj.AddComponent<Camera>();
            camTransform = camObj.transform;

            // Position and orient the custom camera (adjust as needed)
            camTransform.position = new Vector3(0f, 10f, -25f);
            camTransform.rotation = Quaternion.Euler(15f, 0f, 0f);

            // Set the camera's far clipping plane for a large view range
            customCamera.farClipPlane = 1e7f;
            customCamera.fieldOfView = 60f;

            Debug.Log("[WasdMainMenu] Custom camera created.");
        }

        private void HandleMovement()
        {
            Vector3 translation = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) translation += camTransform.forward;
            if (Input.GetKey(KeyCode.S)) translation -= camTransform.forward;
            if (Input.GetKey(KeyCode.A)) translation -= camTransform.right;
            if (Input.GetKey(KeyCode.D)) translation += camTransform.right;

            // Adjust speed with Shift (faster) or Ctrl (slower)
            if (Input.GetKey(KeyCode.LeftShift))
                translation *= 4f;
            else if (Input.GetKey(KeyCode.LeftControl))
                translation *= 0.25f;

            camTransform.position += translation * (moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            // Rotate by right-click dragging
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
                camTransform.Rotate(-mouseY, mouseX, 0f, Space.Self);
            }

            // Q/E keys for banking
            if (Input.GetKey(KeyCode.Q))
                camTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
            if (Input.GetKey(KeyCode.E))
                camTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                float newFOV = customCamera.fieldOfView - (scroll * zoomSpeed);
                customCamera.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
            }
        }
    }
}
