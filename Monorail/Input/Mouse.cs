using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Monorail.Input
{
    public static class Mouse
    {
        internal static GameWindow _window;

        public static void Update()
        {

        }

        /// <summary>
        /// Whether the button is currently down
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsButtonDown(MouseButton button) => _window.MouseState.IsButtonDown(button);
        
        /// <summary>
        /// Whether the button is currently up
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsButtonUp(MouseButton button) => !_window.MouseState.IsButtonDown(button);

        /// <summary>
        /// Whether the button has just been pressed this frame
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsButtonPressed(MouseButton button)
            => _window.MouseState.IsButtonDown(button) && !_window.MouseState.WasButtonDown(button);

        /// <summary>
        /// Whether the button has just been released this frame
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsButtonReleased(MouseButton button)
            => !_window.MouseState.IsButtonDown(button) && _window.MouseState.WasButtonDown(button);

        public static float X
        {
            get => Position.X;
            set => Position = new Vector2(value, Position.Y);
        }

        public static float Y
        {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }

        public static Vector2 Position
        {
            get => _window.MouseState.Position;
            set => _window.MousePosition = value;
        }

        public static Vector2 PreviousPosition => _window.MouseState.PreviousPosition;

        public static Vector2 PositionDelta => Position - PreviousPosition;

        public static float ScrollDeltaY => _window.MouseState.ScrollDelta.Y;
    }
}
