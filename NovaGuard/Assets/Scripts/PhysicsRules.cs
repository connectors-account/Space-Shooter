using UnityEngine;

namespace NovaGuard
{
    [DefaultExecutionOrder(-500)]
    public sealed class PhysicsRules : MonoBehaviour
    {
        public const int PlayerLayer = 8, EnemyLayer = 9, FriendlyLayer = 10, HostileLayer = 11, PickupLayer = 12;
        private void Awake()
        {
            for (int a = PlayerLayer; a <= PickupLayer; a++)
                for (int b = a; b <= PickupLayer; b++) Physics2D.IgnoreLayerCollision(a, b, true);
            Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, false);
            Physics2D.IgnoreLayerCollision(PlayerLayer, HostileLayer, false);
            Physics2D.IgnoreLayerCollision(PlayerLayer, PickupLayer, false);
            Physics2D.IgnoreLayerCollision(EnemyLayer, FriendlyLayer, false);
            Time.fixedDeltaTime = .02f;
            Time.maximumDeltaTime = .1f;
        }
    }
}
