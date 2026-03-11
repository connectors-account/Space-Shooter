using System.Numerics;

namespace SpaceShooter;

public static class CollisionSystem
{
    public static bool CheckCollision(ICollidable a, ICollidable b)
    {
        float distance = Vector2.Distance(a.Position, b.Position);
        return distance < (a.Radius + b.Radius);
    }
}
