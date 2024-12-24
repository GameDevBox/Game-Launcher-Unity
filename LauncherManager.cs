using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine.Rendering;
using TMPro;

namespace LuncherGameDevBox
{
    public class LauncherManager : MonoBehaviour
    {
        #region Fields

        public Camera mainCamera;

        [Header("UI Elements")]
        public GameObject launcherCanvas;
        public TMP_Dropdown screenDropdown;
        public Toggle windowedToggle;
        public TMP_Dropdown graphicsAPIDropdown;

        [Header("Game Settings")]
        public string mainGameScene;

        private static Resolution[] resolutions;

        #endregion

        #region Initialization

        private void Start()
        {
            InitializeLauncher();
            HandleLauncherSettings();
        }

        private void InitializeLauncher()
        {
            launcherCanvas.SetActive(false);
            mainCamera.gameObject.SetActive(false);

            if (!PlayerPrefs.HasKey("Resolution"))
                SetDefaultResolution();
            else
                UpdateResolutionDropdown(PlayerPrefs.GetString("Resolution"));

            InitializeFullScreenMode();
            InitializeGraphicsAPI();
        }

        private void InitializeFullScreenMode()
        {
            bool isFullScreen = !PlayerPrefs.HasKey("FullScreenMode") || PlayerPrefs.GetInt("FullScreenMode") == 1;
            PlayerPrefs.SetInt("FullScreenMode", isFullScreen ? 1 : 0);
            windowedToggle.isOn = !isFullScreen;
        }

        private void InitializeGraphicsAPI()
        {
            if (!PlayerPrefs.HasKey("GraphicsAPI"))
            {
                PlayerPrefs.SetString("GraphicsAPI", "DirectX11");
                graphicsAPIDropdown.value = 0;

                if (!IsDirectX12Compatible())
                    graphicsAPIDropdown.options.RemoveAt(1);
            }
            else
            {
                string graphicsAPI = PlayerPrefs.GetString("GraphicsAPI");

                if (!IsDirectX12Compatible())
                    graphicsAPIDropdown.options.RemoveAt(1);

                graphicsAPIDropdown.value = graphicsAPI == "DirectX11" ? 0 : 1;
            }
        }


        #endregion

        #region Functions

        private void HandleLauncherSettings()
        {
            if (PlayerPrefs.GetInt("Luncher", 1) == 0)
            {
                // Start The Game!
                ApplySettingsAndStartGame();
            }
            else
            {
                // Run Luncher, let player change settings and hit the play button!
                ShowLauncherUI();
            }
        }

        private void SetDefaultResolution()
        {
            string currentMonitorResolution = null;
            SetupResolution(out currentMonitorResolution);

            PlayerPrefs.SetString("Resolution", currentMonitorResolution);
        }

        private void UpdateResolutionDropdown(string resolution)
        {
            string currentMonitorResolution = null;
            SetupResolution(out currentMonitorResolution);

            for (int i = 0; i < screenDropdown.options.Count; i++)
            {
                if (screenDropdown.options[i].text == resolution)
                {
                    screenDropdown.value = i;
                    break;
                }
            }
        }

        private void SetupResolution(out string currentMonitorRes)
        {
            // Clear All Resolutions Options
            screenDropdown.options.Clear();

            currentMonitorRes = null;

            // Setup other resolutions

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (i == 0)
                {
                    // Set first current Monitor Resolution
                    Resolution resolution = Screen.currentResolution;
                    currentMonitorRes = $"{resolution.width} x {resolution.height} (Monitor)";
                    screenDropdown.options.Add(new TMP_Dropdown.OptionData(currentMonitorRes));
                    screenDropdown.value = 0;
                    //screenDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentMonitorRes; // Fixing Unknown Bug
                    screenDropdown.RefreshShownValue();
                }

                screenDropdown.options.Add(new TMP_Dropdown.OptionData($"{resolutions[i].width} x {resolutions[i].height}"));
            }
        }



        private bool IsDirectX12Compatible()
        {
            // DirectX 12 generally requires Shader Model 4.5 or higher
            bool supportsShaderModel45 = SystemInfo.graphicsShaderLevel >= 45; 

            // Check if the current device type is Direct3D12
            bool isDirectX12Available = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12;

            return supportsShaderModel45 && isDirectX12Available;
        }

        private void LaunchGameWithGraphicsAPI(string graphicsAPIArgument)
        {

#if UNITY_EDITOR
            ApplySettingsAndStartGame();
            return;
#endif

                        string executablePath = System.IO.Path.Combine(
    System.IO.Path.GetDirectoryName(Application.dataPath),
    System.IO.Path.GetFileNameWithoutExtension(Application.dataPath)?.Replace("_Data", "") + ".exe"
);

            if (!File.Exists(executablePath))
            {
                UnityEngine.Debug.LogError("Executable not found: " + executablePath);
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = graphicsAPIArgument,
                UseShellExecute = true,
            };

            Process.Start(startInfo);
            Application.Quit();
        }

        private void ShowLauncherUI()
        {
            launcherCanvas.SetActive(true);
            mainCamera.gameObject.SetActive(true);
        }

        private void ApplySettingsAndStartGame()
        {
            // Get the resolution string from PlayerPrefs
            string resolution = PlayerPrefs.GetString("Resolution");

            // Remove "Monitor" if it's part of the string
            resolution = resolution.Replace("Monitor", "").Trim();

            string[] resolutionParts = resolution.Split(new[] { 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);// 1920 x 1080 = [1920] [1080]
            int width = int.Parse(resolutionParts[0]);
            int height = int.Parse(resolutionParts[1]);

            Screen.SetResolution(width, height,
                PlayerPrefs.GetInt("FullScreenMode") == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            PlayerPrefs.SetInt("Luncher", 1);

            SceneManager.LoadScene(mainGameScene);
        }

        #endregion

        #region Button Functions

        public void SetResolution()
        {
            PlayerPrefs.SetString("Resolution", screenDropdown.value == 0 ?
                $"{Screen.currentResolution.width} x {Screen.currentResolution.height} (Monitor)" :
                $"{resolutions[screenDropdown.value - 1].width} x {resolutions[screenDropdown.value - 1].height}");
        }

        public void SetFullScreenMode()
        {
            PlayerPrefs.SetInt("FullScreenMode", windowedToggle.isOn ? 0 : 1);
        }

        public void SetGraphicQuality()
        {
            // Write your Own Logic
            // Add A Player.Prefs here and save the Quality
            // then when it goes to main level read the data and set the quality there.
        }

        public void SetGraphicsAPI()
        {
            string selectedAPI = graphicsAPIDropdown.value == 0 ? "DirectX11" : "DirectX12";
            PlayerPrefs.SetString("GraphicsAPI", selectedAPI);
        }

        public void PlayGame()
        {
            PlayerPrefs.SetInt("Luncher", 0);

            string graphicsAPIArgument = PlayerPrefs.GetString("GraphicsAPI") == "DirectX11" ? "-force-d3d11" : "-force-d3d12";
            LaunchGameWithGraphicsAPI(graphicsAPIArgument);
        }

        public void QuitButton()
        {
            Application.Quit();
        }

        #endregion

        #region Utility Functions

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadConfig()
        {
            Screen.SetResolution(640, 480, false);
            resolutions = Screen.resolutions;
        }

        #endregion
    }
}
