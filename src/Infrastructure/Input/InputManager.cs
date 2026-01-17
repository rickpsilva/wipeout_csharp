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

        public static bool IsActionPressed(GameAction action, KeyboardState? keyboard)
        {
            if (keyboard == null) return false;
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

        /// <summary>
        /// Maps OpenTK Keys to wipeout-rewrite button codes (matching input.h enum values).
        /// Used for control remapping in "AWAITING INPUT" screen.
        /// </summary>
        public static uint MapKeyToButtonCode(Keys key)
        {
            return key switch
            {
                // Letters A-Z (INPUT_KEY_A = 4 ... INPUT_KEY_Z = 29)
                Keys.A => 4, Keys.B => 5, Keys.C => 6, Keys.D => 7, Keys.E => 8,
                Keys.F => 9, Keys.G => 10, Keys.H => 11, Keys.I => 12, Keys.J => 13,
                Keys.K => 14, Keys.L => 15, Keys.M => 16, Keys.N => 17, Keys.O => 18,
                Keys.P => 19, Keys.Q => 20, Keys.R => 21, Keys.S => 22, Keys.T => 23,
                Keys.U => 24, Keys.V => 25, Keys.W => 26, Keys.X => 27, Keys.Y => 28,
                Keys.Z => 29,
                
                // Numbers 1-0 (INPUT_KEY_1 = 30 ... INPUT_KEY_0 = 39)
                Keys.D1 => 30, Keys.D2 => 31, Keys.D3 => 32, Keys.D4 => 33, Keys.D5 => 34,
                Keys.D6 => 35, Keys.D7 => 36, Keys.D8 => 37, Keys.D9 => 38, Keys.D0 => 39,
                
                // Special keys
                Keys.Enter => 40,       // INPUT_KEY_RETURN
                Keys.Escape => 41,      // INPUT_KEY_ESCAPE
                Keys.Backspace => 42,   // INPUT_KEY_BACKSPACE
                Keys.Tab => 43,         // INPUT_KEY_TAB
                Keys.Space => 44,       // INPUT_KEY_SPACE
                Keys.Minus => 45,       // INPUT_KEY_MINUS
                Keys.Equal => 46,       // INPUT_KEY_EQUALS
                Keys.LeftBracket => 47, // INPUT_KEY_LEFTBRACKET
                Keys.RightBracket => 48,// INPUT_KEY_RIGHTBRACKET
                Keys.Backslash => 49,   // INPUT_KEY_BACKSLASH
                Keys.Semicolon => 51,   // INPUT_KEY_SEMICOLON
                Keys.Apostrophe => 52,  // INPUT_KEY_APOSTROPHE
                Keys.GraveAccent => 53, // INPUT_KEY_TILDE
                Keys.Comma => 54,       // INPUT_KEY_COMMA
                Keys.Period => 55,      // INPUT_KEY_PERIOD
                Keys.Slash => 56,       // INPUT_KEY_SLASH
                Keys.CapsLock => 57,    // INPUT_KEY_CAPSLOCK
                
                // Function keys
                Keys.F1 => 58, Keys.F2 => 59, Keys.F3 => 60, Keys.F4 => 61,
                Keys.F5 => 62, Keys.F6 => 63, Keys.F7 => 64, Keys.F8 => 65,
                Keys.F9 => 66, Keys.F10 => 67, Keys.F11 => 68, Keys.F12 => 69,
                
                // Navigation
                Keys.PrintScreen => 70, Keys.ScrollLock => 71, Keys.Pause => 72,
                Keys.Insert => 73, Keys.Home => 74, Keys.PageUp => 75,
                Keys.Delete => 76, Keys.End => 77, Keys.PageDown => 78,
                Keys.Right => 79, Keys.Left => 80, Keys.Down => 81, Keys.Up => 82,
                
                // Numpad
                Keys.NumLock => 83,
                Keys.KeyPadDivide => 84, Keys.KeyPadMultiply => 85,
                Keys.KeyPadSubtract => 86, Keys.KeyPadAdd => 87, Keys.KeyPadEnter => 88,
                Keys.KeyPad1 => 89, Keys.KeyPad2 => 90, Keys.KeyPad3 => 91,
                Keys.KeyPad4 => 92, Keys.KeyPad5 => 93, Keys.KeyPad6 => 94,
                Keys.KeyPad7 => 95, Keys.KeyPad8 => 96, Keys.KeyPad9 => 97,
                Keys.KeyPad0 => 98, Keys.KeyPadDecimal => 99,
                
                // Modifiers
                Keys.LeftControl => 100, Keys.LeftShift => 101, Keys.LeftAlt => 102,
                Keys.LeftSuper => 103, Keys.RightControl => 104, Keys.RightShift => 105,
                Keys.RightAlt => 106,
                
                _ => 0 // INPUT_INVALID
            };
        }
    }
}
