using Monorail.Renderer;
using OpenTK.Mathematics;
using System;

namespace Monorail.ECS
{
    public class SpriteRenderer
    {
        public Sprite Sprite;

        public float Depth
        {
            get => _depth;
            set => SetDepth(value);
        }

        public Color4 Modulate = Color4.White;

        float _depth = 0.0f;

        public SpriteRenderer()
        { }

        public SpriteRenderer(Sprite sprite)
        {
            Sprite = sprite;
        }

        public SpriteRenderer(Texture2D texture)
        {
            Sprite = new Sprite(texture);
        }

        public void Render(Transform2D transform)
        {
            if (Sprite == null)
                return;

            Renderer2D.DrawQuad(transform, Modulate, Depth, Sprite.Texture, Sprite.UniformSource);
        }

        public SpriteRenderer SetDepth(float depth)
        {
            if (depth >= -1 && depth <= 1)
                _depth = depth;

            return this;
        }
    }
}
