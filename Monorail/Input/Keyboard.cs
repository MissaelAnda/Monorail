using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.Input
{
    public static class Keyboard
    {
        internal static GameWindow _window;

        public static bool IsKeyDown(Keys key) =>
            _window.KeyboardState.IsKeyPressed(key);

        public static bool IsKeyPressed(Keys key) =>
            _window.KeyboardState.IsKeyDown(key);

        public static bool IsKeyReleased(Keys key) =>
            _window.KeyboardState.IsKeyReleased(key);
    }
}
