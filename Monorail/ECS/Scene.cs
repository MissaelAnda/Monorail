using Necs;
using Monorail.Renderer;
using OpenTK.Mathematics;

namespace Monorail.ECS
{
    public abstract class Scene
    {
        public Vector2 Resolution
        {
            get => _resolution;
            set => SetResolution(value);
        }

        // TODO: Create and change to RenderTarget class if needed
        public Framebuffer RenderTarget { get; protected set; }

        public ICamera Camera { get; protected set; }

        public Color4 ClearColor = Color4.DarkGray;

        protected Vector2 _resolution;

        internal Registry _registry = new Registry();

        public Scene()
        {
            RenderTarget = new Framebuffer(App.Width, App.Height, ColorAttachements.All);
            _resolution = new Vector2(App.Width, App.Height);
        }

        public abstract Entity CreateEntity(Entity? parent = null);

        public abstract void DeleteEntity(Entity entity);

        public abstract void Update(float delta);

        public abstract void Render(float delta);

        public virtual Scene SetResolution(Vector2 resolution)
        {
            if (_resolution != resolution && resolution != Vector2.Zero)
            {
                _resolution = resolution;
                Camera.Resolution = resolution;
                RenderTarget.Resize(resolution);
            }

            return this;
        }

        #region Scene Lifecycle

        public virtual void OnLoad()
        { }

        public virtual void OnUpdate(float delta)
        { }

        public virtual void BeforeRender(float delta)
        { }

        public virtual void AfterRender(float delta)
        { }

        public virtual void OnUnload()
        { }

        #endregion
    }
}
