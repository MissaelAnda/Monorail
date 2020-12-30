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

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;
        Color4 ClearColor = Color4.CornflowerBlue;

        public static int Width { get; protected set; }
        public static int Height { get; protected set; }

        Camera2D camera;

        Transform2D parent;
        Transform2D quad;

        Texture2D Test;
        Texture2D Background;

        public App() : base(GameWindowSettings.Default, new NativeWindowSettings() { APIVersion = new Version(4, 5) })
        {
            Title = "Monorail";
            Size = new Vector2i(1600, 900);
            layerStack = new LayerStack(this);

            UpdateFrame += OnUpdate;

            Mouse._window = this;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);

            parent = new Transform2D();
            quad = new Transform2D();

            quad.Position = new Vector2(-200, 0);
            quad.Scale = new Vector2(150, 100);
            quad.Parent = parent;

            camera = new Camera2D(new Transform2D());
            camera.MaxZoom = 5;
            camera.MinZoom = 0.001f;
            camera.Zoom = 0;

            EditorManager.CurrentScene = new Scene2D(camera);

            Test = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\blender2.83.png", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });
            Background = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\charliebrown.jpg", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);

            parent.Rotation += (float)args.Time;
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            RenderCommand.SetClearMasks(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            RenderCommand.SetClearColor(new Color4(0.1f, 0.1f, 0.1f, 1.0f));
            RenderCommand.Clear();

            EditorManager.CurrentScene.RenderTarget.Bind();

            RenderCommand.SetClearColor(ClearColor);
            RenderCommand.Clear();
            RenderCommand.SetCaps(EnableCap.Blend);
            RenderCommand.ResetDrawCalls();

            Renderer2D.Begin(camera);

            Renderer2D.DrawElipse(parent, Color4.White, 200, Test);
            Renderer2D.DrawQuad(quad, new Color4[] { Color4.White, Color4.White, Color4.White, Color4.White }, Background);
            Renderer2D.DrawTriangle(
                new Vector2[] { new Vector2(50, -150), new Vector2(200, 150), new Vector2(350, -150) },
                new Color4[] { Color4.Red, Color4.Green, Color4.Blue }
            );

            Renderer2D.End();

            EditorManager.CurrentScene.RenderTarget.Unbind();

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
