using System.IO;
using System.Collections.Generic;
using BattleSword.Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BattleSword.Editor
{
    [InitializeOnLoad]
    public static class LanMultiplayerSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Dev/LAN_Multiplayer_Test.unity";
        private const string PlayerPrefabPath = "Assets/Prefabs/Networking/NetworkPlayer.prefab";
        private const string ProjectilePrefabPath = "Assets/Prefabs/Networking/NetworkTestProjectile.prefab";
        private const string NetworkPrefabsListPath = "Assets/Prefabs/Networking/LAN_NetworkPrefabs.asset";

        static LanMultiplayerSceneBuilder()
        {
            EditorApplication.delayCall += BuildMissingAssets;
        }

        private static void BuildMissingAssets()
        {
            if (!File.Exists(ScenePath) || !File.Exists(PlayerPrefabPath) || !File.Exists(ProjectilePrefabPath))
            {
                Build();
            }
        }

        [MenuItem("BattleSword/Build LAN Multiplayer Test Scene")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes/Dev");
            Directory.CreateDirectory("Assets/Prefabs/Networking");

            var projectilePrefab = BuildProjectilePrefab();
            var playerPrefab = BuildPlayerPrefab();

            var previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            RenderSettings.skybox = null;

            CreateLighting();
            CreateGround();
            CreateSpawnMarkers();
            CreateNetworkManager(playerPrefab, projectilePrefab);
            CreateCanvas();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            if (previousScene.IsValid())
            {
                SceneManager.SetActiveScene(previousScene);
            }

            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Built LAN multiplayer test scene at {ScenePath}");
        }

        private static GameObject BuildPlayerPrefab()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "NetworkPlayer";
            Object.DestroyImmediate(root.GetComponent<Collider>());
            root.transform.position = Vector3.zero;

            var networkObject = root.AddComponent<NetworkObject>();
            root.AddComponent<CharacterController>().height = 2f;
            root.AddComponent<OwnerNetworkTransform>();

            var bodyRenderer = root.GetComponent<MeshRenderer>();
            bodyRenderer.sharedMaterial = CreateMaterial("NetworkPlayer_Default_Mat", Color.cyan);

            var pivot = new GameObject("CameraPivot").transform;
            pivot.SetParent(root.transform);
            pivot.localPosition = new Vector3(0f, 0.65f, 0f);

            var cameraObject = new GameObject("OwnerCamera");
            cameraObject.transform.SetParent(pivot);
            cameraObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            cameraObject.transform.localRotation = Quaternion.identity;
            var camera = cameraObject.AddComponent<Camera>();
            var listener = cameraObject.AddComponent<AudioListener>();
            camera.enabled = false;
            listener.enabled = false;

            var controller = root.AddComponent<NetworkPlayerController>();
            var projectilePrefab = AssetDatabase.LoadAssetAtPath<NetworkProjectile>(ProjectilePrefabPath);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("playerCamera").objectReferenceValue = camera;
            serialized.FindProperty("audioListener").objectReferenceValue = listener;
            serialized.FindProperty("bodyRenderer").objectReferenceValue = bodyRenderer;
            serialized.FindProperty("cameraPivot").objectReferenceValue = pivot;
            serialized.FindProperty("testProjectilePrefab").objectReferenceValue = projectilePrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildProjectilePrefab()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "NetworkTestProjectile";
            root.transform.localScale = Vector3.one * 0.25f;

            root.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial("NetworkProjectile_Mat", Color.yellow);
            root.AddComponent<NetworkObject>();
            root.AddComponent<NetworkTransform>();

            var body = root.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            root.AddComponent<NetworkProjectile>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateNetworkManager(GameObject playerPrefab, GameObject projectilePrefab)
        {
            var prefabsList = BuildNetworkPrefabsList(playerPrefab, projectilePrefab);
            var networkManagerObject = new GameObject("NetworkManager");
            var networkManager = networkManagerObject.AddComponent<NetworkManager>();
            networkManagerObject.AddComponent<UnityTransport>();

            var config = new NetworkConfig
            {
                PlayerPrefab = playerPrefab,
                NetworkTransport = networkManagerObject.GetComponent<UnityTransport>()
            };

            config.Prefabs.NetworkPrefabsLists ??= new List<NetworkPrefabsList>();
            config.Prefabs.NetworkPrefabsLists.Add(prefabsList);
            networkManager.NetworkConfig = config;
        }

        private static NetworkPrefabsList BuildNetworkPrefabsList(GameObject playerPrefab, GameObject projectilePrefab)
        {
            var prefabsList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkPrefabsListPath);
            if (prefabsList == null)
            {
                prefabsList = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(prefabsList, NetworkPrefabsListPath);
            }

            var playerNetworkPrefab = new NetworkPrefab { Prefab = playerPrefab };
            var projectileNetworkPrefab = new NetworkPrefab { Prefab = projectilePrefab };

            if (!prefabsList.Contains(playerPrefab))
            {
                prefabsList.Add(playerNetworkPrefab);
            }

            if (!prefabsList.Contains(projectilePrefab))
            {
                prefabsList.Add(projectileNetworkPrefab);
            }

            EditorUtility.SetDirty(prefabsList);
            return prefabsList;
        }

        private static void CreateCanvas()
        {
            var canvasObject = new GameObject("LAN Connection Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var panel = CreatePanel(canvasObject.transform);
            var localIp = CreateText(panel, "Local IP", new Vector2(0f, -20f), "Local IP: detecting...");
            var ipInput = CreateInput(panel, "IP Address Input", new Vector2(0f, -65f), "Host IPv4");
            var portInput = CreateInput(panel, "Port Input", new Vector2(0f, -110f), "7777");
            var hostButton = CreateButton(panel, "Host Button", new Vector2(-80f, -160f), "Host");
            var joinButton = CreateButton(panel, "Join Button", new Vector2(80f, -160f), "Join");
            var status = CreateText(panel, "Status", new Vector2(0f, -215f), "Choose Host or Join.");

            var ui = canvasObject.AddComponent<LanConnectionUI>();
            var serialized = new SerializedObject(ui);
            serialized.FindProperty("hostButton").objectReferenceValue = hostButton;
            serialized.FindProperty("joinButton").objectReferenceValue = joinButton;
            serialized.FindProperty("ipAddressInput").objectReferenceValue = ipInput;
            serialized.FindProperty("portInput").objectReferenceValue = portInput;
            serialized.FindProperty("statusText").objectReferenceValue = status;
            serialized.FindProperty("localIpText").objectReferenceValue = localIp;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform CreatePanel(Transform parent)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f, -16f);
            rect.sizeDelta = new Vector2(360f, 260f);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
            return panel.transform;
        }

        private static Text CreateText(Transform parent, string name, Vector2 position, string value)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 32f);
            rect.anchoredPosition = position;

            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 16;
            return text;
        }

        private static InputField CreateInput(Transform parent, string name, Vector2 position, string placeholder)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 34f);
            rect.anchoredPosition = position;
            root.GetComponent<Image>().color = Color.white;

            var text = CreateText(root.transform, "Text", Vector2.zero, string.Empty);
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(10f, 0f);
            text.rectTransform.offsetMax = new Vector2(-10f, 0f);

            var placeholderText = CreateText(root.transform, "Placeholder", Vector2.zero, placeholder);
            placeholderText.color = new Color(0f, 0f, 0f, 0.45f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.rectTransform.anchorMin = Vector2.zero;
            placeholderText.rectTransform.anchorMax = Vector2.one;
            placeholderText.rectTransform.offsetMin = new Vector2(10f, 0f);
            placeholderText.rectTransform.offsetMax = new Vector2(-10f, 0f);

            var input = root.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholderText;
            if (placeholder == "7777")
            {
                input.text = "7777";
            }
            return input;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position, string label)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(130f, 38f);
            rect.anchoredPosition = position;
            root.GetComponent<Image>().color = new Color(0.18f, 0.43f, 0.9f, 1f);

            var buttonText = CreateText(root.transform, "Text", Vector2.zero, label);
            buttonText.rectTransform.anchorMin = Vector2.zero;
            buttonText.rectTransform.anchorMax = Vector2.one;
            buttonText.rectTransform.offsetMin = Vector2.zero;
            buttonText.rectTransform.offsetMax = Vector2.zero;
            return root.GetComponent<Button>();
        }

        private static void CreateLighting()
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "LAN Test Ground";
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial("LAN_Ground_Mat", new Color(0.22f, 0.24f, 0.25f));
        }

        private static void CreateSpawnMarkers()
        {
            for (var i = 0; i < 4; i++)
            {
                var marker = new GameObject($"Spawn Reference {i + 1}");
                marker.transform.position = new Vector3((i - 1.5f) * 3f, 0.05f, 0f);
            }
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Directory.CreateDirectory("Assets/Art/Materials");
            var path = $"Assets/Art/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
