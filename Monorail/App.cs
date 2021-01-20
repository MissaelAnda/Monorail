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
using Necs;

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;

        public static int Width { get; protected set; }
        public static int Height { get; protected set; }

        Transform2D parent;
        Transform2D quad;

        Texture2D Test;
        Texture2D Background;

        Scene _currentScene;

        public App() : base(GameWindowSettings.Default, new NativeWindowSettings() { APIVersion = new Version(4, 5) })
        {
            Title = "Monorail";
            Size = new Vector2i(1600, 900);
            layerStack = new LayerStack(this);

            UpdateFrame += OnUpdate;

            Mouse._window = this;
            Keyboard._window = this;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);

            Test = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\blender2.83.png", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });
            Background = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\charliebrown.jpg", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });

            _currentScene = new Scene2D();
            _currentScene.ClearColor = Color4.DarkSlateGray;
            EditorManager.CurrentScene = _currentScene;

            Registry registry = EditorManager.CurrentScene._registry;

            var entity = EditorManager.CurrentScene.CreateEntity();
            parent = registry.GetComponent<Transform2D>(entity);
            var sprite = new SpriteRenderer(Test);
            sprite.Sprite.Source = new System.Drawing.Rectangle(200, 200, 1000, 500);
            registry.AddComponent(entity, sprite);
            
            entity = EditorManager.CurrentScene.CreateEntity(entity);
            quad = registry.GetComponent<Transform2D>(entity);
            registry.AddComponent(entity, new SpriteRenderer(Background));

            parent.Scale = new Vector2(500, 400);
            quad.Position = new Vector2(0.8f, 0);
            quad.Scale = new Vector2(.8f, .5f);
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);
            float delta = (float)args.Time;

            _currentScene.Update(delta);

            // TODO: Remove
            parent.Rotation += delta;
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

            float delta = (float)args.Time;
            EditorManager.CurrentScene.Render(delta);

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
