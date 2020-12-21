using System;
using Monorail.ECS;
using Monorail.Layers;
using Monorail.Renderer;
using OpenTK.Mathematics;
using Monorail.Layers.ImGUI;
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
            //quad.Parent = parent;
            parent.LocalScale = new Vector2(300, 250);
            quad.Position = new Vector2(500, 0);
            quad.Scale = new Vector2(200);

            camera = new Camera2D();
            //camera = new Camera2D(new Vector2(2, 2));
            camera.Transform = parent;

            Background = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\charliebrown.jpg", new TextureBuilder()
            {
                InternalFormat = PixelInternalFormat.Rgba,
                PixelFormat = PixelFormat.Bgra
            });
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);

            quad.LocalRotation += (float)args.Time;

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
            Renderer2D.DrawQuad(quad, new Color4[] { Color4.White, Color4.White, Color4.White, Color4.White }, Background);
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
