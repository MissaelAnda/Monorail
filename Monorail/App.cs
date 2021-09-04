using System;
using Monorail.ECS;
using Monorail.Layers;
using Monorail.Editor;
using Monorail.Renderer;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Monorail.Input;
using Monorail.Examples;

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;

        public static int Width { get; protected set; }
        public static int Height { get; protected set; }

        Scene _currentScene;

        public App() : base(GameWindowSettings.Default, new NativeWindowSettings() { APIVersion = new Version(4, 5) })
        {
            Title = "Monorail";
            Size = new Vector2i(1600, 900);
            layerStack = new LayerStack(this);

            UpdateFrame += OnUpdate;

            Mouse._window = this;
            Keyboard._window = this;

            Renderer3D.Initialize();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _currentScene = new ExampleScene3D();
            //_currentScene = new ExampleScene2D();
            EditorManager.CurrentScene = _currentScene;

            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);
            float delta = (float)args.Time;

            _currentScene.Update(delta);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Clear main Render Target (Screen)
            RenderCommand.SetClearMasks(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            RenderCommand.SetClearColor(new Color4(0.1f, 0.1f, 0.1f, 1.0f));
            RenderCommand.Clear();

            float delta = (float)args.Time;
            _currentScene.Render(delta);

            layerStack.Render(args.Time);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected new void OnUnload()
        {
            
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            RenderCommand.SetViewportSize(e.Width, e.Height);
            Width = e.Width;
            Height = e.Height;
        }
    }
}
