using ImGuiNET;
using Monorail.Layers.ImGUI;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Monorail.Layers
{
    public class ImGuiLayer : Layer
    {
        ImGuiController _controller;
        GameWindow _window;

        public ImGuiLayer(GameWindow window) : base("ImGuiLayer")
        {
            _window = window;
            _controller = new ImGuiController(_window.Size.X, _window.Size.Y);
        }

        public override void OnUpdate(double delta)
        {
            _controller.Update(_window, delta);
        }

        public override void OnRender(double delta)
        {
            //Layout specification here
            Editor.Start();

            _controller.Render();
        }

        public override void OnRegisterEvents(ref EventList events)
        {
            events.Resize = OnResize;
            events.TextInput = OnTextInput;
        }

        public void OnResize(ResizeEventArgs args) => _controller.WindowResized(args.Width, args.Height);

        public void OnTextInput(TextInputEventArgs args) => _controller.PressChar((char)args.Unicode);
    }
}
