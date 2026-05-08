using Unity.Netcode.Components;

namespace BattleSword.Networking
{
    public sealed class OwnerNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            // For this LAN prototype, player movement is owner-authoritative so the
            // controlling client can move responsively. Server-authoritative actions
            // are still demonstrated by projectile spawning.
            return false;
        }
    }
}
