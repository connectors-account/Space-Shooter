using System.Numerics;

namespace SpaceShooter;

public interface ICollidable
{
    Vector2 Position { get; }
    float Radius { get; }
}
