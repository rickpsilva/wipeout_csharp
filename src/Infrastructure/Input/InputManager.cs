using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace WipeoutRewrite.Infrastructure.Input
{
    // Enumerate game actions
    public enum GameAction
    {
        Accelerate,
        Brake,
        TurnLeft,
        TurnRight,
        BoostLeft,
        BoostRight,
        WeaponFire,
        WeaponCycle,
        Pause,
        Exit,
        
        // Menu actions
        MenuUp,
        MenuDown,
        MenuLeft,
        MenuRight,
        MenuSelect,
        MenuBack
    }

    public static class InputManager
    {
        private static readonly Dictionary<GameAction, Keys> _keyBindings = new()
        {
            { GameAction.Accelerate, Keys.Up },
            { GameAction.Brake, Keys.Down },
            { GameAction.TurnLeft, Keys.Left },
            { GameAction.TurnRight, Keys.Right },
            { GameAction.BoostLeft, Keys.Z },
            { GameAction.BoostRight, Keys.X },
            { GameAction.WeaponFire, Keys.Space },
            { GameAction.WeaponCycle, Keys.C },
            { GameAction.Pause, Keys.P },
            { GameAction.Exit, Keys.Escape },
            
            // Menu bindings
            { GameAction.MenuUp, Keys.Up },
            { GameAction.MenuDown, Keys.Down },
            { GameAction.MenuLeft, Keys.Left },
            { GameAction.MenuRight, Keys.Right },
            { GameAction.MenuSelect, Keys.Enter },
            { GameAction.MenuBack, Keys.Backspace }
        };

        // Previous state to detect changes (key press vs hold)
        private static readonly Dictionary<GameAction, bool> _previousState = new();

        static InputManager()
        {
            foreach (var action in Enum.GetValues(typeof(GameAction)))
            {
                _previousState[(GameAction)action] = false;
            }
        }

        public static bool IsActionDown(GameAction action, KeyboardState keyboard)
        {
            if (_keyBindings.TryGetValue(action, out var key))
            {
                return keyboard.IsKeyDown(key);
            }
            return false;
        }

        public static bool IsActionPressed(GameAction action, KeyboardState keyboard)
        {
            bool isCurrentlyDown = IsActionDown(action, keyboard);
            bool wasPreviouslyDown = _previousState[action];
            // Don't update here - let Update() handle it
            return isCurrentlyDown && !wasPreviouslyDown;
        }

        public static void Update(KeyboardState keyboard)
        {
            // Update previous state for next frame - this is the ONLY place that updates _previousState
            foreach (var action in Enum.GetValues(typeof(GameAction)))
            {
                GameAction ga = (GameAction)action;
                _previousState[ga] = IsActionDown(ga, keyboard);
            }
        }

        public static void RemapKey(GameAction action, Keys key)
        {
            if (_keyBindings.ContainsKey(action))
            {
                _keyBindings[action] = key;
                Console.WriteLine($"Remapped {action} to {key}");
            }
        }

        public static Keys GetKeyForAction(GameAction action)
        {
            return _keyBindings.TryGetValue(action, out var key) ? key : Keys.Unknown;
        }
    }
}
