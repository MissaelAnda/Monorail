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

        protected Vector2 _resolution;

        internal Registry _registry = new Registry();

        public Scene()
        {
            RenderTarget = new Framebuffer(App.Width, App.Height, FramebufferAttachements.All);
            _resolution = new Vector2(App.Width, App.Height);
        }

        public abstract Entity CreateEntity(Transform2D parent = null);

        public virtual void DeleteEntity(Entity entity)
        {
            var transform = _registry.GetComponent<Transform2D>(entity);
            transform.Parent = null;

            for (int i = 0; i < transform.Children.Count; i++)
                DeleteEntity(transform.Children[i].Entity);

            _registry.DeleteEntity(entity);
        }

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
    }
}
