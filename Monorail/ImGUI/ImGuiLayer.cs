using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ImGuiNET;

namespace Monorail.ImGUI
{
    public class ImGuiLayer : Layer
    {
        ImGuiController _controller;
        GameWindow _window;

        public ImGuiLayer(GameWindow window) : base("ImGuiLayer")
        {
            _window = window;
            _controller = new ImGuiController(_window.Size.X, _window.Size.Y, _window);
        }

        public override void OnAttached()
        {
            
        }

        public override void OnUpdate(double delta)
        {
            _controller.Update(_window, delta);
            ImGui.ShowDemoWindow();
            _controller.Render();
        }

        public override void OnRegisterEvents(ref EventList events)
        {
            events.Resize = OnResize;
            events.KeyDown = OnKeyPress;
        }

        public void OnResize(ResizeEventArgs args) => _controller.WindowResized(args.Width, args.Height);

        public void OnKeyPress(KeyboardKeyEventArgs args) => _controller.PressChar((char)args.ScanCode);
    }
}
