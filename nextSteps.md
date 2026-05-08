# LAN Multiplayer Prototype Next Steps

## What Works

- The LAN test scene now starts on a simple menu with `Play Single-Player` and `Play Multi-Player`.
- `Play Single-Player` hides the menu and leaves the existing FPS scene player active.
- `Play Multi-Player` opens the LAN host/join panel.
- Host/Join disables the local-only Low Poly Shooter player/camera before Netcode spawns the multiplayer player.
- `Assets/Scenes/Dev/LAN_Multiplayer_Test.unity` is the dedicated LAN multiplayer test scene.
- The scene contains a `NetworkManager` using Unity Transport.
- One player can host a LAN session on a selected UDP port.
- Another player can join by entering the host computer's local IPv4 address and matching port.
- Each connected client spawns as a separate capsule player.
- Only the owning client controls its own player camera, audio listener, movement, and mouse look.
- Remote players are visible and receive replicated position/rotation updates.
- Each player receives a different capsule color from the server.
- Press `F` to request a server-authoritative test cube/projectile spawn. The host/server spawns it as a network object so all clients can see it.

## How To Open The Scene

1. Open this project in Unity 6.
2. If `Assets/Scenes/Dev/LAN_Multiplayer_Test.unity` is not present yet, run `BattleSword > Build LAN Multiplayer Test Scene` from the Unity menu. This generates the scene and the networking prefabs.
3. Open `Assets/Scenes/Dev/LAN_Multiplayer_Test.unity`.
4. Press Play.

## How To Host On LAN

1. Open the test scene.
2. Click `Play Multi-Player`.
3. Leave the port as `7777` unless you need a different port.
4. Click `Host`.
5. Tell the other player the displayed local IPv4 address and port.
6. If Windows Defender Firewall prompts you, allow Unity or the built player on private networks.

## How To Join On LAN

1. Make sure both computers are on the same Wi-Fi/router or wired LAN.
2. Open the test scene or a build that starts in the test scene.
3. Click `Play Multi-Player`.
4. Enter the host computer's local IPv4 address.
5. Enter the same port as the host, usually `7777`.
6. Click `Join`.

## Finding The Host IPv4 Manually On Windows

1. Open Command Prompt or PowerShell on the host computer.
2. Run `ipconfig`.
3. Look for the active Wi-Fi or Ethernet adapter.
4. Use the `IPv4 Address`, commonly something like `192.168.x.x` or `10.0.x.x`.

The in-game local IP display uses the first non-loopback IPv4 address it can find. That is usually correct for simple home LANs, but VPNs, virtual adapters, or multiple network cards can make it pick the wrong address. Use `ipconfig` if joining fails.

## Testing Two Instances On One Computer

- In the Unity Editor, click `Play Multi-Player`, then click `Host`.
- Start a second instance from a standalone build and join `127.0.0.1` with port `7777`.
- Some editor/build combinations can also work with the LAN IPv4 address shown in the UI.
- Running two Unity Editor play sessions from the same project is not recommended.

## Known Limitations

- LAN only. There is no Lobby, Relay, Authentication, matchmaking, Steam networking, or internet NAT traversal.
- Movement is intentionally simple and owner-authoritative for responsiveness in this prototype.
- Projectile spawning is only a proof of networked server-spawned actions, not a weapon system.
- There is no lag compensation, client-side prediction, reconciliation, anti-cheat, enemy networking, save/load networking, or dedicated server flow.
- Spawn positions are still basic and may overlap if many players join at once.
- The UI uses Unity's built-in `uGUI` controls for simplicity.

## Package Assumptions

- Unity Editor: `6000.4.6f1`.
- Netcode for GameObjects: `com.unity.netcode.gameobjects` `2.7.0`.
- Unity Transport: `com.unity.transport` `2.2.1`.
