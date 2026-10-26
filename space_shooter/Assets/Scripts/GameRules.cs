using System;

namespace SpaceShooter
{
    // Pure C# rules shared by the Unity game and the small command-line test suite.
    public static class GameRules
    {
        public const float ArenaHalfWidth = 8f;
        public const float ArenaHalfHeight = 5f;
        public const float PlayerHalfWidth = 0.55f;
        public const float PlayerHalfHeight = 0.5f;
        public const float EnemyHalfWidth = 0.5f;
        public const float EnemyHalfHeight = 0.4f;
        public const float BulletHalfWidth = 0.09f;
        public const float BulletHalfHeight = 0.22f;
        public const float PlayerSpeed = 6f;
        public const float EnemySpeed = 2.8f;
        public const float BulletSpeed = 13f;
        public const float FireInterval = 0.18f;
        public const float SpawnInterval = 0.8f;
        public const int PointsPerEnemy = 10;

        public static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        public static void MovePlayer(ref float x, ref float y, float inputX, float inputY, float dt)
        {
            float length = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
            if (length > 1f)
            {
                inputX /= length;
                inputY /= length;
            }
            x = Clamp(x + inputX * PlayerSpeed * dt,
                -ArenaHalfWidth + PlayerHalfWidth, ArenaHalfWidth - PlayerHalfWidth);
            y = Clamp(y + inputY * PlayerSpeed * dt,
                -ArenaHalfHeight + PlayerHalfHeight, ArenaHalfHeight - PlayerHalfHeight);
        }

        // Segment versus expanded AABB in relative coordinates. Both objects may move;
        // testing their whole relative path avoids missing fast bullets between frames.
        public static bool SweptOverlap(float startX, float startY, float endX, float endY,
            float combinedHalfWidth, float combinedHalfHeight, out float hitTime)
        {
            float enter = 0f;
            float exit = 1f;
            bool hit = Clip(startX, endX - startX, combinedHalfWidth, ref enter, ref exit)
                && Clip(startY, endY - startY, combinedHalfHeight, ref enter, ref exit);
            hitTime = enter;
            return hit;
        }

        private static bool Clip(float start, float delta, float radius, ref float enter, ref float exit)
        {
            if (Math.Abs(delta) < 0.000001f)
                return Math.Abs(start) <= radius;
            float first = (-radius - start) / delta;
            float second = (radius - start) / delta;
            if (first > second)
            {
                float temporary = first;
                first = second;
                second = temporary;
            }
            enter = Math.Max(enter, first);
            exit = Math.Min(exit, second);
            return enter <= exit;
        }
    }
}
