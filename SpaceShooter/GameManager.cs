namespace SpaceShooter
{
    /// <summary>Tracks game state, score, lives, and wave number.</summary>
    public enum GameState { Menu, Playing, GameOver }

    public class GameManager
    {
        public int    Score { get; private set; }
        public int    Wave  { get; private set; }
        public int    Lives { get; private set; }
        public int    HighScore { get; private set; }
        public GameState State { get; set; }

        public GameManager() => Reset();

        public void Reset()
        {
            if (Score > HighScore) HighScore = Score;
            Score = 0;
            Wave  = 1;
            Lives = 3;
            State = GameState.Menu;
        }

        public void AddScore(int points)
        {
            Score += points;
            if (Score > HighScore) HighScore = Score;
        }

        public void LoseLife()   => Lives--;
        public bool IsGameOver   => Lives <= 0;
        public void NextWave()   => Wave++;
    }
}
