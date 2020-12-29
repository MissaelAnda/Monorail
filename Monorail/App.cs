using System;
using Monorail.ECS;
using Monorail.Layers;
using Monorail.Renderer;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;
        Color4 ClearColor = Color4.CornflowerBlue;

        public static Framebuffer Framebuffer;

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
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);

            // Create framebuffer
            Framebuffer = new Framebuffer(800, 600, FramebufferAttachements.All);

            parent = new Transform2D();
            quad = new Transform2D();

            quad.Position = new Vector2(-200, 0);
            quad.Scale = new Vector2(150, 100);
            quad.Parent = parent;

            camera = new Camera2D(parent);

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

            // Should be processed in the ecs system
            // Shouldn't resize to 0,0 when minimized
            if (Framebuffer.Size != Editor.Viewport && !WindowState.HasFlag(WindowState.Minimized))
                Framebuffer.Resize((int)Editor.Viewport.X, (int)Editor.Viewport.Y);
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

            Framebuffer.Bind();

            RenderCommand.SetClearColor(ClearColor);
            RenderCommand.Clear();
            RenderCommand.SetCaps(EnableCap.Blend);

            Renderer2D.Begin(camera);

            Renderer2D.DrawElipse(parent, Color4.White, 200, Test);
            Renderer2D.DrawQuad(quad, new Color4[] { Color4.White, Color4.White, Color4.White, Color4.White }, Background);
            Renderer2D.DrawTriangle(
                new Vector2[] { new Vector2(50, -150), new Vector2(200, 150), new Vector2(350, -150) },
                new Color4[] { Color4.Red, Color4.Green, Color4.Blue }
            );

            Renderer2D.End();

            Framebuffer.Unbind();

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
