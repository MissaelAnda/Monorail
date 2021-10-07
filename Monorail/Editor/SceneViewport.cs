using ImGuiNET;
using Monorail.ECS;
using Monorail.Input;
using Monorail.Util;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using ImVec2 = System.Numerics.Vector2;

namespace Monorail.Editor
{
    public static class SceneViewport
    {
        public static bool Focused { get; private set; }

        public static bool WasFocused { get; private set; }

        public static bool JustFocused => Focused && !WasFocused;

        public static bool JustLostFocus => !Focused && WasFocused;

        public static bool MovingCamera { get; private set; }

        /// <summary>
        /// The viewport rendereable size (excludes titlebar)
        /// </summary>
        public static Vector2 Resolution { get; private set; }

        /// <summary>
        /// Window Position
        /// </summary>
        public static Vector2 Position { get; private set; }

        /// <summary>
        /// Max Positions
        /// </summary>
        public static Vector2 MaxPosition => Position + Size;

        /// <summary>
        /// The window size (including titlebar)
        /// </summary>
        public static Vector2 Size { get; private set; }

        static float TitlebarSize => Size.Y - Resolution.Y;

        static Vector2 PreviousMousePosition;

        static bool ShouldRetainCursor = false;

        public static void Process()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new ImVec2(0, 0));
            ImGui.Begin("Scene Viewport");

            Focused = ImGui.IsWindowFocused();
            Resolution = ImGui.GetContentRegionAvail().ToVector2();
            Position = ImGui.GetWindowPos().ToVector2();
            Size = ImGui.GetWindowSize().ToVector2();

            EditorManager.CurrentScene.Resolution = Resolution;

            if (Focused && Keyboard.IsKeyPressed(Keys.Q))
            {
                if (EditorManager.CurrentScene is Scene2D)
                {
                    var camera = (EditorManager.CurrentScene as Scene2D).Camera2D;
                    camera.Transform.Position = Vector2.Zero;
                    camera.Zoom = 0;
                }
                else
                {
                    var camera = (EditorManager.CurrentScene as Scene3D).Camera3D;
                    camera.Transform.Position = Vector3.Zero;
                    camera.Transform.Rotation = new Quaternion();
                    camera.Zoom = 0;
                }
            }

            MouseProcess();

            ImGui.Image((IntPtr)EditorManager.CurrentScene.RenderTarget.AttachmentTextureID(Renderer.AttachmentType.Color0), Resolution.ToImVec2(), new ImVec2(0, 1), new ImVec2(1, 0));
            ImGui.End();
            ImGui.PopStyleVar();

            if (ShouldRetainCursor)
            {
                PreviousMousePosition = Mouse.Position;

                // If the mouse is outside the contentRegion move it in to the other side
                if (Mouse.X > MaxPosition.X)
                {
                    Mouse.X = Position.X;
                    PreviousMousePosition.X = Position.X;
                }
                else if (Mouse.X < Position.X)
                {
                    Mouse.X = MaxPosition.X;
                    PreviousMousePosition.X = MaxPosition.X;
                }

                if (Mouse.Y > MaxPosition.Y)
                {
                    Mouse.Y = Position.Y;
                    PreviousMousePosition.Y = Position.Y;
                }
                else if (Mouse.Y < Position.Y)
                {
                    Mouse.Y = MaxPosition.Y;
                    PreviousMousePosition.Y = MaxPosition.Y;
                }
            }

            ShouldRetainCursor = false;
            WasFocused = Focused;
        }

        static void MouseProcess()
        {
            var contentRegion = new RectangleF(Position.X, Position.Y + TitlebarSize, Resolution.X, Resolution.Y);
            if (contentRegion.IsInside(Mouse.Position) && Mouse.ScrollDeltaY != 0)
                EditorManager.CurrentScene.Camera.RawZoom -= Mouse.ScrollDeltaY / 10;

            // TODO: Editor camera controllers

            if (MovingCamera)
            {
                // TODO: 3D midldle mouse click pans camera
                // Currently moving the camera
                if (Mouse.IsButtonDown(MouseButton.Middle))
                {
                    // The camera only moves if the mouse does
                    if (Mouse.Position != PreviousMousePosition)
                    {
                        // Movement in a 2D scene is in x, y axis
                        if (EditorManager.CurrentScene is Scene2D scene2D)
                        {
                            var transform = scene2D.Camera2D.Transform;
                            transform.Position += new Vector2(PreviousMousePosition.X - Mouse.X, Mouse.Y - PreviousMousePosition.Y) * scene2D.Camera.RawZoom;
                        }
                        // Movement in 3D scene is along camera cross angle
                        //else if (EditorManager.CurrentScene is Scene3D scene3D)

                    }
                }
                else MovingCamera = false;

                // TODO: Right click grants full movement to 3D camera with WASD QE control schema
            }
            else if (Mouse.IsButtonPressed(MouseButton.Middle) && contentRegion.IsInside(Mouse.Position))
            {
                ImGui.SetWindowFocus();
                MovingCamera = true;
            }

            if (MovingCamera)
                ShouldRetainCursor = true;
        }
    }
}
