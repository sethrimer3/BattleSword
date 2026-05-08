using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BattleSword.Networking
{
    public sealed class LanConnectionUI : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button multiplayerButton;

        [Header("LAN Connection")]
        [SerializeField] private GameObject connectionPanel;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button backButton;
        [SerializeField] private InputField ipAddressInput;
        [SerializeField] private InputField portInput;
        [SerializeField] private Text statusText;
        [SerializeField] private Text localIpText;

        private const ushort DefaultPort = 7777;
        private bool menuOpen;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            EnsureInputSystemEventModule();
            EnsureMenuExists();

            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.AddListener(StartSinglePlayer);
            }

            if (multiplayerButton != null)
            {
                multiplayerButton.onClick.AddListener(ShowMultiplayerMenu);
            }

            if (hostButton != null)
            {
                hostButton.onClick.AddListener(StartHost);
            }

            if (joinButton != null)
            {
                joinButton.onClick.AddListener(StartClient);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(ShowMainMenu);
            }

            if (portInput != null && string.IsNullOrWhiteSpace(portInput.text))
            {
                portInput.text = DefaultPort.ToString();
            }
        }

        private void Start()
        {
            previousTimeScale = Time.timeScale;
            var localIp = FindLocalIPv4Address();

            if (localIpText != null)
            {
                localIpText.text = string.IsNullOrWhiteSpace(localIp)
                    ? "Local IP: not found"
                    : $"Local IP: {localIp}";
            }

            if (ipAddressInput != null && string.IsNullOrWhiteSpace(ipAddressInput.text) && !string.IsNullOrWhiteSpace(localIp))
            {
                ipAddressInput.text = localIp;
            }

            ShowMainMenu();
        }

        private void Update()
        {
            if (!menuOpen)
            {
                return;
            }

            // Some FPS controllers lock the cursor every frame. While the menu is open,
            // keep taking control back so uGUI can receive mouse clicks.
            UnlockCursorForMenu();
        }

        private void OnDestroy()
        {
            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.RemoveListener(StartSinglePlayer);
            }

            if (multiplayerButton != null)
            {
                multiplayerButton.onClick.RemoveListener(ShowMultiplayerMenu);
            }

            if (hostButton != null)
            {
                hostButton.onClick.RemoveListener(StartHost);
            }

            if (joinButton != null)
            {
                joinButton.onClick.RemoveListener(StartClient);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(ShowMainMenu);
            }
        }

        private void StartSinglePlayer()
        {
            if (!TryGetTransport(out var networkManager, out var transport))
            {
                // If this scene already has a non-networked FPS controller, this still
                // lets the player proceed instead of being blocked by missing Netcode setup.
                HideAllMenus();
                return;
            }

            if (!TryReadPort(out var port))
            {
                return;
            }

            // Single-player uses a local host session so this prototype can reuse the
            // NetworkPlayer prefab without maintaining a second controller path.
            transport.SetConnectionData("127.0.0.1", port, "127.0.0.1");

            if (networkManager.StartHost())
            {
                SetStatus("Starting single-player.");
                HideAllMenus();
            }
            else
            {
                SetStatus("Could not start single-player session.");
            }
        }

        private void ShowMainMenu()
        {
            menuOpen = true;
            Time.timeScale = 0f;

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            if (connectionPanel != null)
            {
                connectionPanel.SetActive(false);
            }

            SetStatus("Choose Single Player or Multiplayer.");
            UnlockCursorForMenu();
        }

        private void ShowMultiplayerMenu()
        {
            menuOpen = true;
            Time.timeScale = 0f;

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (connectionPanel != null)
            {
                connectionPanel.SetActive(true);
            }

            SetStatus("Host a LAN game or join by IPv4 address.");
            UnlockCursorForMenu();
        }

        private void HideAllMenus()
        {
            menuOpen = false;
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (connectionPanel != null)
            {
                connectionPanel.SetActive(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void StartHost()
        {
            if (!TryGetTransport(out var networkManager, out var transport))
            {
                return;
            }

            if (!TryReadPort(out var port))
            {
                return;
            }

            // Host listens on all local interfaces. Other players connect to the host's LAN IPv4.
            transport.SetConnectionData("0.0.0.0", port, "0.0.0.0");

            var localIp = FindLocalIPv4Address();
            if (networkManager.StartHost())
            {
                SetStatus(string.IsNullOrWhiteSpace(localIp)
                    ? $"Hosting on port {port}. Find this PC's IPv4 with ipconfig."
                    : $"Hosting on {localIp}:{port}");
                HideAllMenus();
            }
            else
            {
                SetStatus("Could not start host. Check the NetworkManager and port.");
            }
        }

        private void StartClient()
        {
            if (!TryGetTransport(out var networkManager, out var transport))
            {
                return;
            }

            if (!TryReadPort(out var port))
            {
                return;
            }

            var address = ipAddressInput != null ? ipAddressInput.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(address))
            {
                SetStatus("Enter the host computer's local IPv4 address.");
                return;
            }

            // Clients connect directly to the host over LAN using Unity Transport UDP.
            transport.SetConnectionData(address, port);

            if (networkManager.StartClient())
            {
                SetStatus($"Joining {address}:{port}");
                HideAllMenus();
            }
            else
            {
                SetStatus("Could not start client. Check the IP address and NetworkManager.");
            }
        }

        private bool TryGetTransport(out NetworkManager networkManager, out UnityTransport transport)
        {
            networkManager = NetworkManager.Singleton;
            transport = null;

            if (networkManager == null)
            {
                SetStatus("No NetworkManager found in the scene.");
                return false;
            }

            transport = networkManager.GetComponent<UnityTransport>();
            if (transport == null)
            {
                SetStatus("NetworkManager is missing UnityTransport.");
                return false;
            }

            return true;
        }

        private bool TryReadPort(out ushort port)
        {
            port = DefaultPort;
            var rawPort = portInput != null ? portInput.text.Trim() : DefaultPort.ToString();

            if (!ushort.TryParse(rawPort, out port) || port == 0)
            {
                SetStatus("Port must be a number from 1 to 65535.");
                return false;
            }

            return true;
        }

        private void EnsureMenuExists()
        {
            if (connectionPanel == null && hostButton != null)
            {
                connectionPanel = hostButton.transform.parent.gameObject;
            }

            if (mainMenuPanel == null)
            {
                mainMenuPanel = CreatePanel("Main Menu Panel", new Vector2(380f, 230f));
                CreateMenuText(mainMenuPanel.transform, "Title", new Vector2(0f, -32f), "BattleSword", 26);
                singlePlayerButton = CreateMenuButton(mainMenuPanel.transform, "Single Player Button", new Vector2(0f, -94f), "Play Single-Player");
                multiplayerButton = CreateMenuButton(mainMenuPanel.transform, "Multiplayer Button", new Vector2(0f, -148f), "Play Multi-Player");
            }

            if (connectionPanel != null && backButton == null)
            {
                backButton = CreateMenuButton(connectionPanel.transform, "Back Button", new Vector2(0f, -250f), "Back");
            }
        }

        private static void EnsureInputSystemEventModule()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }

            // The generated scene originally used StandaloneInputModule, which depends
            // on legacy Input Manager axes like "Submit". This project uses the new
            // Input System, so replace it before the EventSystem starts processing UI.
            var standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule != null)
            {
                Destroy(standaloneInputModule);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        private GameObject CreatePanel(string name, Vector2 size)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(transform, false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            return panel;
        }

        private Text CreateMenuText(Transform parent, string name, Vector2 position, string value, int fontSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 40f);
            rect.anchoredPosition = position;

            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = fontSize;
            return text;
        }

        private Button CreateMenuButton(Transform parent, string name, Vector2 position, string label)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 42f);
            rect.anchoredPosition = position;

            buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.43f, 0.9f, 1f);

            var labelText = CreateMenuText(buttonObject.transform, "Text", Vector2.zero, label, 16);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;

            return buttonObject.GetComponent<Button>();
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"[LAN] {message}");
        }

        private static void UnlockCursorForMenu()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static string FindLocalIPv4Address()
        {
            try
            {
                // This can return several addresses on machines with VPNs or virtual adapters.
                // The first non-loopback IPv4 is usually the right private LAN address for home testing.
                return Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(address => address.ToString())
                    .FirstOrDefault(address => !IPAddress.IsLoopback(IPAddress.Parse(address)));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not detect local IPv4 address: {exception.Message}");
                return string.Empty;
            }
        }
    }
}
