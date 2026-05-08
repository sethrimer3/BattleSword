using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSword.Networking
{
    public sealed class LanConnectionUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private InputField ipAddressInput;
        [SerializeField] private InputField portInput;
        [SerializeField] private Text statusText;
        [SerializeField] private Text localIpText;

        private const ushort DefaultPort = 7777;

        private void Awake()
        {
            hostButton.onClick.AddListener(StartHost);
            joinButton.onClick.AddListener(StartClient);

            if (portInput != null && string.IsNullOrWhiteSpace(portInput.text))
            {
                portInput.text = DefaultPort.ToString();
            }
        }

        private void Start()
        {
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

            SetStatus("Choose Host or enter a LAN IPv4 address and Join.");
        }

        private void OnDestroy()
        {
            if (hostButton != null)
            {
                hostButton.onClick.RemoveListener(StartHost);
            }

            if (joinButton != null)
            {
                joinButton.onClick.RemoveListener(StartClient);
            }
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

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"[LAN] {message}");
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
