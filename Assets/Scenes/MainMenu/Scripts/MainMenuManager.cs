using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MainR
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Main Interface")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_InputField ipInput;
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private GameObject playPanel;

        [Header("Settings Interface")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private AudioMixer masterMixer;

        [Header("Settings")]
        [SerializeField] private ushort defaultPort;
        [SerializeField] private string defaultScene;

        private List<Resolution> availableResolutions;
        private List<Resolution> selectableResolutions;
        private bool initializingSettings = false;

        #region Initialization/Teardown
        void Start()
        {
            // Get all available resolutions
            Resolution[] resolutions = Screen.resolutions;

            // Get the highest available resolution
            Resolution maxResolution = resolutions[^1];

            // Set the screen to the highest resolution
            Screen.SetResolution(maxResolution.width, maxResolution.height, Screen.fullScreenMode, maxResolution.refreshRateRatio);

            Debug.Log("Resolution automatically set to: " + maxResolution.width + "x" + maxResolution.height + " @ " + maxResolution.refreshRateRatio + "Hz");

            // Set callbacks
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionStateChanged;
            InstanceFinder.ClientManager.OnAuthenticated += OnClientAuthenticated;

            // Set cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnDestroy()
        {
            // Set callbacks
            if (InstanceFinder.ClientManager && InstanceFinder.ServerManager)
            {
                InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionStateChanged;
                InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionStateChanged;
                InstanceFinder.ClientManager.OnAuthenticated -= OnClientAuthenticated;
            }
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Code intended for execution when the HOST button is pressed
        /// </summary>
        public void OnHost()
        {
            if (InstanceFinder.IsHostStarted) { Debug.LogWarning("Attempted to start server while server is started!"); return; }
            if (InstanceFinder.IsClientStarted) { Debug.LogWarning("Attempted to start server while client is started!"); return; }

            InstanceFinder.ServerManager.StartConnection(defaultPort);
            InstanceFinder.ClientManager.StartConnection();
        }

        /// <summary>
        /// Code intended for execution when the JOIN button is pressed
        /// </summary>
        public void OnJoin()
        {
            if (InstanceFinder.IsClientStarted) { Debug.LogWarning("Attempted to start client while client is started!"); return; }

            // Empty IP defaults to loopback
            string joinAddress = ipInput.text == "" ? "127.0.0.1" : ipInput.text;
            InstanceFinder.ClientManager.StartConnection(joinAddress, defaultPort);
        }

        /// <summary>
        /// Code intended for execution when the QUIT button is pressed
        /// </summary>
        public void OnQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Code intended for execution when the settings menu is opened
        /// </summary>
        public void OnSettingsOpened()
        {
            initializingSettings = true;

            // Clear variables
            availableResolutions = new List<Resolution>(Screen.resolutions);
            selectableResolutions = new();

            // Clear dropdowns
            resolutionDropdown.ClearOptions();

            // Option holders
            List<string> resolutionOptions = new();

            // Add available 16:9 resolutions and refresh rates
            foreach (var res in availableResolutions)
            {
                float aspectRatio = (float)res.width / res.height;
                if (Mathf.Approximately(aspectRatio, 16f / 9f)) // Check if the aspect ratio is approximately 16:9
                {
                    string option = res.width + "x" + res.height + " @ " + Mathf.Round((float)res.refreshRateRatio.value) + "Hz";
                    if (!resolutionOptions.Contains(option))
                    {
                        resolutionOptions.Add(option);
                        selectableResolutions.Add(res);
                    }
                }
            }

            // Add to drop down
            resolutionDropdown.AddOptions(resolutionOptions);

            // Set current resolution
            string currentOption = Screen.currentResolution.width + "x" + Screen.currentResolution.height + " @ " + Mathf.Round((float)Screen.currentResolution.refreshRateRatio.value) + "Hz";
            for (int i = 0; i < resolutionOptions.Count; i++)
            {
                if (resolutionOptions[i] == currentOption)
                {
                    resolutionDropdown.value = i;
                }
            }

            // Set toggle
            fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;

            // Set volume slider
            masterMixer.GetFloat("MasterVolume", out float currentVolume);
            volumeSlider.value = Mathf.Pow(10, currentVolume / 20); // Convert from dB to linear

            initializingSettings = false;
        }

        /// <summary>
        /// Code intended for execution when a screen option is chosen
        /// </summary>
        public void OnSetResolution()
        {
            if (initializingSettings) { return; }

            // Retrieve the selected resolution from the selectableResolutions list
            Resolution selectedResolution = selectableResolutions[resolutionDropdown.value];

            FullScreenMode mode = fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, mode, selectedResolution.refreshRateRatio);

            Debug.Log("Resolution set to " + (fullscreenToggle.isOn ? "fullscreen" : "windowed") + " " + $"{selectedResolution.width}x{selectedResolution.height} at {selectedResolution.refreshRateRatio}Hz");
        }

        /// <summary>
        /// Code intended for execution when the volume slider is shifted
        /// </summary>
        public void OnSetVolume()
        {
            float volumeInDb = Mathf.Log10(volumeSlider.value) * 20; // Convert from linear to dB
            masterMixer.SetFloat("MasterVolume", volumeInDb);
            Debug.Log($"Volume set to {volumeInDb} dB");
        }

        /// <summary>
        /// Code intended for execution when the client is turned on or off
        /// </summary>
        private void OnClientConnectionStateChanged(ClientConnectionStateArgs args)
        {
            joinButton.interactable = args.ConnectionState == LocalConnectionState.Stopped;
            hostButton.interactable = args.ConnectionState == LocalConnectionState.Stopped;
        }

        /// <summary>
        /// Code intended for execution when the server is turned on or off
        /// </summary>
        private void OnServerConnectionStateChanged(ServerConnectionStateArgs args)
        {
            joinButton.interactable = args.ConnectionState == LocalConnectionState.Stopped;
            hostButton.interactable = args.ConnectionState == LocalConnectionState.Stopped;
        }

        /// <summary>
        /// Code intended for execution when the client authenticates with a server
        /// </summary>
        private void OnClientAuthenticated()
        {
            InstanceFinder.ClientManager.Broadcast(new LocalNetworkManager.JoinBroadcast(usernameInput.text));
        }
        #endregion
    }
}
