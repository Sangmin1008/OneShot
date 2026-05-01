using Fusion;

namespace OneShot
{
    public interface IDamageable
    {
        void TakeDamage(float damage, PlayerRef shooter);
    }
}