using System;
using UnityEngine.InputSystem;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Hand-authored wrapper around the New Input System that mirrors the actions defined in
    /// Assets/InputActions/PlayerInputActions.inputactions. Building the actions in code guarantees
    /// the project compiles without relying on Unity's "Generate C# Class" step.
    ///
    /// Actions:
    ///   Move  (Vector2) : WASD + Arrow keys (2D composite)
    ///   Fire  (Button)  : Space + Left Mouse Button
    ///   Pause (Button)  : Escape
    /// </summary>
    public sealed class PlayerInputActions : IDisposable
    {
        private readonly InputActionMap _gameplayMap;

        public GameplayActions Gameplay { get; }

        public PlayerInputActions()
        {
            _gameplayMap = new InputActionMap("Gameplay");

            // Move — 2D Vector composite (WASD + arrow keys).
            var move = _gameplayMap.AddAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            // Fire — button (Space or Left Mouse Button).
            var fire = _gameplayMap.AddAction("Fire", InputActionType.Button);
            fire.AddBinding("<Keyboard>/space");
            fire.AddBinding("<Mouse>/leftButton");

            // Pause — button (Escape).
            var pause = _gameplayMap.AddAction("Pause", InputActionType.Button);
            pause.AddBinding("<Keyboard>/escape");

            Gameplay = new GameplayActions(move, fire, pause);
        }

        public void Enable() => _gameplayMap.Enable();
        public void Disable() => _gameplayMap.Disable();

        public void Dispose()
        {
            _gameplayMap?.Dispose();
        }

        /// <summary>Strongly-typed accessors for the Gameplay action map.</summary>
        public sealed class GameplayActions
        {
            public InputAction Move { get; }
            public InputAction Fire { get; }
            public InputAction Pause { get; }

            public GameplayActions(InputAction move, InputAction fire, InputAction pause)
            {
                Move = move;
                Fire = fire;
                Pause = pause;
            }
        }
    }
}
