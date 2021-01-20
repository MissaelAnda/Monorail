using System.Drawing;
using Monorail.Renderer;
using OpenTK.Mathematics;

namespace Monorail.ECS
{
    public class Sprite
    {
        public readonly Texture2D Texture;

        public Rectangle? Source
        {
            get => _source;
            set => SetSource(value);
        }

        public RectangleF? UniformSource
        {
            get => _uniformSource;
            set => SetUniformSource(value);
        }

        Rectangle? _source;
        RectangleF? _uniformSource;

        public Sprite(Texture2D texture)
        {
            Texture = texture;
            SetUniformSource(null);
        }

        /// <summary>
        /// Sets the OpenGL uniform texture coordinates 0-1 (correspindily setting the pixels source)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Sprite SetUniformSource(RectangleF? source)
        {
            _uniformSource = source;

            if (source.HasValue)
            {
                var s = source.Value;

                int x = (int)(s.X * Texture.Width);
                // Inverts the Y coordinate from opengl to image
                int y = Texture.Height - (int)((s.Y + s.Height) * Texture.Height);
                int width = (int)(s.Width * Texture.Width);
                int height = (int)(s.Height * Texture.Height);
                _source = new Rectangle(x, y, width, height);
            }
            else _source = new Rectangle(0, 0, Texture.Width, Texture.Height);

            return this;
        }

        /// <summary>
        /// Sets the texture pixels source (correspondily setting the uniform OpenGL coordinates)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Sprite SetSource(Rectangle? source)
        {
            if (source.HasValue)
            {
                var s = source.Value;

                float x = (float)s.X / Texture.Width;
                // Inverts the Y coordinate from image to opengl
                float y = 1f - (float)(s.Y + s.Height) / Texture.Height;
                float width = (float)s.Width / Texture.Width;
                float height = (float)s.Height / Texture.Height;
                _uniformSource = new RectangleF(x, y, width, height);
                _source = source;
            }
            else
            {
                _uniformSource = null;
                _source = new Rectangle(0, 0, Texture.Width, Texture.Height);
            }

            return this;
        }
    }
}