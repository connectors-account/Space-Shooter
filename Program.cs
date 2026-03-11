using Raylib_cs;
using SpaceShooter;

// Initialize window
const int SCREEN_WIDTH = 800;
const int SCREEN_HEIGHT = 600;

Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Space Shooter");
Raylib.SetTargetFPS(60);
Raylib.InitAudioDevice();

// Create game instance
var game = new Game(SCREEN_WIDTH, SCREEN_HEIGHT);

// Main game loop
while (!Raylib.WindowShouldClose())
{
    float deltaTime = Raylib.GetFrameTime();
    
    game.Update(deltaTime);
    
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);
    
    game.Draw();
    
    Raylib.EndDrawing();
}

// Cleanup
game.Unload();
Raylib.CloseAudioDevice();
Raylib.CloseWindow();
