using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Monorail.Debug;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Monorail.ImGUI;

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;
        Color4 ClearColor = Color4.CornflowerBlue;

        public App() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Title = "Monorail";
            Size = new OpenTK.Mathematics.Vector2i(1280, 720);
            layerStack = new LayerStack(this);

            UpdateFrame += OnUpdate;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Log.Core.Succes("Engine started.");
            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);

            GL.ClearColor(ClearColor);
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);


            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected new void OnUnload()
        {
            
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}
