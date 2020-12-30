using Monorail.Renderer;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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

        public Scene()
        {
            RenderTarget = new Framebuffer(App.Width, App.Height, FramebufferAttachements.All);
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
