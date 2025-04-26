using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KSP;
using KSP.UI.Screens;

namespace WasdMainMenu
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class WasdMainMenuCamera : MonoBehaviour
    {
        private const float defaultMoveSpeed = 25f;
        private const float defaultRotationSpeed = 100f;
        private const float defaultZoomSpeed = 10f;
        private const float defaultMinFOV = 5f;
        private const float defaultMaxFOV = 80f;

        private float moveSpeed = defaultMoveSpeed;
        private float rotationSpeed = defaultRotationSpeed;
        private float zoomSpeed = defaultZoomSpeed;
        private float minFOV = defaultMinFOV;
        private float maxFOV = defaultMaxFOV;

        private string moveSpeedString;
        private string rotationSpeedString;
        private string zoomSpeedString;
        private string minFOVString;
        private string maxFOVString;

        private Camera stockMainMenuCamera;
        private Camera customCamera;
        private Transform camTransform;

        private ApplicationLauncherButton appButton;
        private Texture2D cameraEnabledIcon;
        private Texture2D cameraDisabledIcon;
        private bool buttonAdded = false;
        private bool modEnabled = false;

        private bool controlsWindowOpen = false;
        private Rect controlsWindowRect = new Rect(10, 50, 250, 300);
        private const int controlsWindowID = 123456;
        private Vector2 controlsScrollPos = Vector2.zero;

        private bool settingsWindowOpen = false;
        private Rect settingsWindowRect = new Rect(270, 50, 320, 300);
        private const int settingsWindowID = 123457;
        private Vector2 settingsScrollPos = Vector2.zero;

        private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>()
        {
            { "Forward",   KeyCode.W },
            { "Backward",  KeyCode.S },
            { "Left",      KeyCode.A },
            { "Right",     KeyCode.D },
            { "Up",        KeyCode.Space },
            { "Down",      KeyCode.LeftControl },
            { "BankLeft",  KeyCode.Q },
            { "BankRight", KeyCode.E },
            { "SpeedUp",   KeyCode.R },
            { "SlowDown",  KeyCode.F }
        };

        private string waitingForKey = null;
        private bool waitingForKeyWindowOpen = false;
        private Rect waitingForKeyWindowRect = new Rect(400, 200, 200, 100);

        private string keybindsPath;
        private bool validInstall = false;

        private void Start()
        {
            stockMainMenuCamera = Camera.main;
            if (stockMainMenuCamera == null && Camera.allCameras.Length > 0)
            {
                stockMainMenuCamera = Camera.allCameras[0];
            }

            moveSpeedString = moveSpeed.ToString();
            rotationSpeedString = rotationSpeed.ToString();
            zoomSpeedString = zoomSpeed.ToString();
            minFOVString = minFOV.ToString();
            maxFOVString = maxFOV.ToString();

            FindOrCreateKeybindFile();
            LoadKeybindsFromFile();
            CheckModFolder();
        }

        private void FindOrCreateKeybindFile()
        {
            string root = KSPUtil.ApplicationRootPath + "GameData/";
            string[] possibleFolders =
            {
                "WasdMainMenu/Plugin",
                "WasdMainMenu/WasdMainMenu/Plugin",
                "WasdMainMenu-1.2/WasdMainMenu/Plugin"
            };

            bool foundFolder = false;
            foreach (var folder in possibleFolders)
            {
                string fullDir = Path.Combine(root, folder);
                if (Directory.Exists(fullDir))
                {
                    keybindsPath = Path.Combine(fullDir, "WasdKeybinds.txt");
                    foundFolder = true;
                    break;
                }
            }

            if (!foundFolder)
            {
                string fallback = Path.Combine(root, "WasdMainMenu/Plugin");
                if (!Directory.Exists(fallback))
                {
                    Directory.CreateDirectory(fallback);
                }
                keybindsPath = Path.Combine(fallback, "WasdKeybinds.txt");
                ScreenMessages.PostScreenMessage("Created WasdKeybinds.txt in WasdMainMenu/Plugin", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                if (!File.Exists(keybindsPath))
                {
                    File.WriteAllText(keybindsPath, "");
                    ScreenMessages.PostScreenMessage("Created WasdKeybinds.txt at: " + keybindsPath, 3f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }

        private void CheckModFolder()
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(keybindsPath)));
            if (folderName == "WasdMainMenu" || folderName == "WasdMainMenu-1.2")
            {
                validInstall = true;
            }
            else
            {
                ScreenMessages.PostScreenMessage("WasdMainMenu not installed in a valid folder!", 4f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void Update()
        {
            if (!buttonAdded && ApplicationLauncher.Instance != null)
            {
                LoadIcons();

                if (cameraEnabledIcon != null && cameraDisabledIcon != null)
                {
                    appButton = ApplicationLauncher.Instance.AddModApplication(
                        OnAppButtonToggleOn, OnAppButtonToggleOff,
                        null, null, null, null,
                        ApplicationLauncher.AppScenes.MAINMENU,
                        cameraEnabledIcon); 
                    buttonAdded = true;
                }
                else
                {
                    Debug.LogError("[WasdMainMenu] Failed to load toolbar icons.");
                }
            }

            if (modEnabled) 
            {
                if (customCamera != null)
                {
                    HandleMovement();
                    HandleRotation();
                    HandleZoom();
                }
            }
        }

        private void LoadIcons()
        {
            if (cameraEnabledIcon == null)
            {
                cameraEnabledIcon = TryLoadTexture("Camera_Enabled");
            }

            if (cameraDisabledIcon == null)
            {
                cameraDisabledIcon = TryLoadTexture("Camera_Disabled");
            }
        }

        private Texture2D TryLoadTexture(string iconName)
        {
            string[] possiblePaths = new string[]
            {
            "WasdMainMenu-1.3/WasdMainMenu/Textures/" + iconName,
            "WasdMainMenu/Textures/" + iconName,
            "WasdMainMenu-1.3/Textures/" + iconName,
            "WasdMainMenu/WasdMainMenu/Textures/" + iconName
            };

            foreach (var path in possiblePaths)
            {
                var texture = GameDatabase.Instance.GetTexture(path, false);
                if (texture != null)
                {
                    Debug.Log("[WasdMainMenu] Loaded texture from: " + path);
                    return texture;
                }
            }

            Debug.LogError("[WasdMainMenu] Could not find texture: " + iconName);
            return null;
        }

        private void OnDestroy()
        {
            SaveKeybindsToFile();
            if (ApplicationLauncher.Instance != null && appButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
            }
        }

        private void OnAppButtonToggleOn()
        {
            modEnabled = true;

            if (!validInstall)
            {
                ScreenMessages.PostScreenMessage("Invalid WasdMainMenu folder!", 3f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            ScreenMessages.PostScreenMessage(
                "WasdMainMenu enabled",
                3f, ScreenMessageStyle.UPPER_CENTER
            );

            if (appButton != null && cameraEnabledIcon != null)
            {
                appButton.SetTexture(cameraEnabledIcon);
            }

            if (stockMainMenuCamera != null)
                stockMainMenuCamera.gameObject.SetActive(false);

            if (customCamera == null)
                CreateCustomCamera();
        }

        private void OnAppButtonToggleOff()
        {
            modEnabled = false;

            if (customCamera != null)
            {
                Destroy(customCamera.gameObject);
                customCamera = null;
                camTransform = null;
            }

            if (appButton != null && cameraDisabledIcon != null)
            {
                appButton.SetTexture(cameraDisabledIcon);
            }

            if (stockMainMenuCamera != null)
            {
                stockMainMenuCamera.gameObject.SetActive(true);
                stockMainMenuCamera.clearFlags = CameraClearFlags.Skybox;
            }

            controlsWindowOpen = false;
            settingsWindowOpen = false;
            waitingForKeyWindowOpen = false;

            ScreenMessages.PostScreenMessage(
                "Don't worry! The Skybox will fix itself once the MainMenu is refreshed.",
                5f, ScreenMessageStyle.UPPER_CENTER
            );
        }

        private void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (modEnabled)
            {
                if (GUI.Button(new Rect(10, 10, 100, 30), "Controls"))
                    controlsWindowOpen = !controlsWindowOpen;

                if (GUI.Button(new Rect(120, 10, 100, 30), "Settings"))
                    settingsWindowOpen = !settingsWindowOpen;
            }

            if (controlsWindowOpen)
                controlsWindowRect = GUI.Window(controlsWindowID, controlsWindowRect, DrawControlsWindow, "Controls");

            if (settingsWindowOpen)
                settingsWindowRect = GUI.Window(settingsWindowID, settingsWindowRect, DrawSettingsWindow, "Settings");

            if (waitingForKeyWindowOpen)
                waitingForKeyWindowRect = GUI.Window(999999, waitingForKeyWindowRect, DrawWaitingForKeyWindow, "Waiting for Input");

            OnGUI_PostCapture();
        }

        private void DrawControlsWindow(int windowID)
        {
            GUILayout.BeginVertical();
            controlsScrollPos = GUILayout.BeginScrollView(controlsScrollPos, GUILayout.Width(230), GUILayout.Height(180));

            GUILayout.Label("Movement / Rotation / Zoom Settings", HighLogic.Skin.label);

            GUILayout.Label("Move Speed:");
            moveSpeedString = GUILayout.TextField(moveSpeedString, 10);

            GUILayout.Label("Rotation Speed:");
            rotationSpeedString = GUILayout.TextField(rotationSpeedString, 10);

            GUILayout.Label("Zoom Speed:");
            zoomSpeedString = GUILayout.TextField(zoomSpeedString, 10);

            GUILayout.Label("Min FOV:");
            minFOVString = GUILayout.TextField(minFOVString, 10);

            GUILayout.Label("Max FOV:");
            maxFOVString = GUILayout.TextField(maxFOVString, 10);

            GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Apply", GUILayout.Height(25)))
            {
                float temp;
                if (float.TryParse(moveSpeedString, out temp)) moveSpeed = temp;
                if (float.TryParse(rotationSpeedString, out temp)) rotationSpeed = temp;
                if (float.TryParse(zoomSpeedString, out temp)) zoomSpeed = temp;
                if (float.TryParse(minFOVString, out temp)) minFOV = temp;
                if (float.TryParse(maxFOVString, out temp)) maxFOV = temp;
            }

            if (GUILayout.Button("Reset", GUILayout.Height(25)))
            {
                moveSpeed = defaultMoveSpeed;
                rotationSpeed = defaultRotationSpeed;
                zoomSpeed = defaultZoomSpeed;
                minFOV = defaultMinFOV;
                maxFOV = defaultMaxFOV;

                moveSpeedString = moveSpeed.ToString();
                rotationSpeedString = rotationSpeed.ToString();
                zoomSpeedString = zoomSpeed.ToString();
                minFOVString = minFOV.ToString();
                maxFOVString = maxFOV.ToString();
            }

            GUI.DragWindow();
            GUILayout.EndVertical();
        }

        private void DrawSettingsWindow(int windowID)
        {
            GUILayout.BeginVertical();
            settingsScrollPos = GUILayout.BeginScrollView(settingsScrollPos, GUILayout.Width(300), GUILayout.Height(180));

            GUILayout.Label("Keybinds:", HighLogic.Skin.label);

            foreach (var kvp in keyBindings)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(kvp.Key + ": ", GUILayout.Width(120));
                if (GUILayout.Button(kvp.Value.ToString(), GUILayout.Width(100)))
                {
                    waitingForKey = kvp.Key;
                    waitingForKeyWindowOpen = true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Reset to Default", GUILayout.Height(25)))
            {
                ResetKeybindsToDefault();
                SaveKeybindsToFile();
            }

            GUI.DragWindow();
            GUILayout.EndVertical();
        }

        private void DrawWaitingForKeyWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Press any key...");
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void CreateCustomCamera()
        {
            GameObject camObj = new GameObject("MyCustomMainMenuCam");
            customCamera = camObj.AddComponent<Camera>();
            camTransform = camObj.transform;

            camTransform.position = new Vector3(0f, 10f, -25f);
            camTransform.rotation = Quaternion.Euler(15f, 0f, 0f);

            customCamera.farClipPlane = 1e7f;
            customCamera.fieldOfView = 60f;
            customCamera.clearFlags = CameraClearFlags.SolidColor;
            customCamera.backgroundColor = Color.black;
        }

        private void HandleMovement()
        {
            Vector3 translation = Vector3.zero;

            if (Input.GetKey(keyBindings["Forward"])) translation += camTransform.forward;
            if (Input.GetKey(keyBindings["Backward"])) translation -= camTransform.forward;
            if (Input.GetKey(keyBindings["Left"])) translation -= camTransform.right;
            if (Input.GetKey(keyBindings["Right"])) translation += camTransform.right;
            if (Input.GetKey(keyBindings["Up"])) translation += camTransform.up;
            if (Input.GetKey(keyBindings["Down"])) translation -= camTransform.up;

            if (Input.GetKey(keyBindings["SpeedUp"])) translation *= 4f;
            if (Input.GetKey(keyBindings["SlowDown"])) translation *= 0.25f;

            camTransform.position += translation * (moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
                camTransform.Rotate(-mouseY, mouseX, 0f, Space.Self);
            }

            if (Input.GetKey(keyBindings["BankLeft"]))
                camTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
            if (Input.GetKey(keyBindings["BankRight"]))
                camTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                float newFOV = customCamera.fieldOfView - (scroll * zoomSpeed);
                customCamera.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
            }
        }

        private void OnGUI_PostCapture()
        {
            if (!waitingForKeyWindowOpen) return;

            Event e = Event.current;
            if (e.isKey && e.type == EventType.KeyDown)
            {
                keyBindings[waitingForKey] = e.keyCode;
                waitingForKey = null;
                waitingForKeyWindowOpen = false;
                SaveKeybindsToFile();
            }
            else if (e.type == EventType.MouseDown)
            {
                KeyCode newKey = KeyCode.None;
                switch (e.button)
                {
                    case 0: newKey = KeyCode.Mouse0; break;
                    case 1: newKey = KeyCode.Mouse1; break;
                    case 2: newKey = KeyCode.Mouse2; break;
                }
                if (newKey != KeyCode.None)
                {
                    keyBindings[waitingForKey] = newKey;
                    waitingForKey = null;
                    waitingForKeyWindowOpen = false;
                    SaveKeybindsToFile();
                }
            }
        }

        private void ResetKeybindsToDefault()
        {
            keyBindings["Forward"] = KeyCode.W;
            keyBindings["Backward"] = KeyCode.S;
            keyBindings["Left"] = KeyCode.A;
            keyBindings["Right"] = KeyCode.D;
            keyBindings["Up"] = KeyCode.Space;
            keyBindings["Down"] = KeyCode.LeftControl;
            keyBindings["BankLeft"] = KeyCode.Q;
            keyBindings["BankRight"] = KeyCode.E;
            keyBindings["SpeedUp"] = KeyCode.R;
            keyBindings["SlowDown"] = KeyCode.F;
        }

        private void SaveKeybindsToFile()
        {
            try
            {
                var dir = Path.GetDirectoryName(keybindsPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var writer = new StreamWriter(keybindsPath, false))
                {
                    foreach (var kvp in keyBindings)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }
                Debug.Log("[WasdMainMenu] Keybinds saved to: " + keybindsPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WasdMainMenu] Error saving keybinds: " + ex.Message);
            }
        }

        private void LoadKeybindsFromFile()
        {
            if (!File.Exists(keybindsPath)) return;

            try
            {
                var lines = File.ReadAllLines(keybindsPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('=');
                    if (parts.Length < 2) continue;
                    var action = parts[0].Trim();
                    var keyStr = parts[1].Trim();
                    if (keyBindings.ContainsKey(action))
                    {
                        if (Enum.TryParse(keyStr, out KeyCode code))
                        {
                            keyBindings[action] = code;
                        }
                    }
                }
                Debug.Log("[WasdMainMenu] Keybinds loaded from: " + keybindsPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WasdMainMenu] Error loading keybinds: " + ex.Message);
            }
        }
    }
}
