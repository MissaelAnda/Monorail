using System;
using Monorail.Debug;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Monorail.Renderer
{
    [Flags]
    public enum FramebufferAttachements
    {
        Color,
        Depth,
        DepthStencil,

        ColorDepth = Color | Depth,

        All = Color | DepthStencil,
    }

    public class Framebuffer : OpenGLResource
    {
        public static int CurrentlyBinded { get; protected set; } = 0;

        public static FramebufferTarget CurrentTarget { get; protected set; } = FramebufferTarget.Framebuffer;

        public static readonly TextureBuilder DepthStencilTextureBuilder;

        public static readonly TextureBuilder DepthTextureBuilder;

        static Framebuffer()
        {
            DepthStencilTextureBuilder = new TextureBuilder()
            {
                InternalFormat = PixelInternalFormat.Depth24Stencil8,
                PixelFormat = PixelFormat.DepthStencil,
                PixelType = PixelType.UnsignedInt248,
            };

            DepthTextureBuilder = new TextureBuilder()
            {
                InternalFormat = PixelInternalFormat.DepthComponent,
                PixelFormat = PixelFormat.DepthComponent,
                PixelType = PixelType.Float,
            };
        }


        public FramebufferTarget Target
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    _target = value;
                    // If currently binded, change bind type too
                    if (CurrentlyBinded == _id)
                    {
                        GL.BindFramebuffer(_target, _id);
                    }
                }
            }
        }

        public int Width
        {
            get => _width;
            set => Resize(value, _height);
        }

        public int Height
        {
            get => _height;
            set => Resize(_width, value);
        }

        public Vector2i Size => new Vector2i(Width, Height);

        public FramebufferAttachements Attachements
        {
            get => _attachements;
        }

        public Texture2D Color { get; protected set; }
        public Texture2D DepthStencil { get; protected set; }
        FramebufferTarget _target = FramebufferTarget.Framebuffer;
        FramebufferAttachements _attachements;

        int _width, _height;

        public Framebuffer(int width, int height, FramebufferAttachements attachements = FramebufferAttachements.Color)
        {
            Insist.Assert(width > 0);
            Insist.Assert(height > 0);

            _width = width;
            _height = height;
            _attachements = attachements;

            Target = FramebufferTarget.Framebuffer;

            Invalidate();
        }

        void Invalidate()
        {
            bool shouldBind = false;
            if (_id > 0)
            {
                if (CurrentlyBinded == _id)
                {
                    GL.BindFramebuffer(Target, 0);
                    shouldBind = true;
                }
                Color?.Dispose();
                DepthStencil?.Dispose();
                GL.DeleteFramebuffer(_id);
            }

            GL.CreateFramebuffers(1, out _id);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Framebuffer);

            if (_attachements.HasFlag(FramebufferAttachements.Color))
            {
                Color = new Texture2D(_width, _height, new TextureBuilder());
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.ColorAttachment0, Color.ID, 0);
            }
            else
            {
                GL.NamedFramebufferDrawBuffer(_id, DrawBufferMode.None);
                GL.NamedFramebufferReadBuffer(_id, ReadBufferMode.None);
            }

            if (_attachements.HasFlag(FramebufferAttachements.DepthStencil))
            {
                DepthStencil = new Texture2D(_width, _height, DepthStencilTextureBuilder);
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.DepthStencilAttachment, DepthStencil.ID, 0);
            }
            else if (_attachements.HasFlag(FramebufferAttachements.Depth))
            {
                DepthStencil = new Texture2D(_width, _height, DepthTextureBuilder);
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.DepthAttachment, DepthStencil.ID, 0);
            }

            FramebufferStatus status = GL.CheckNamedFramebufferStatus(_id, Target);
            Insist.AssertEq(status, FramebufferStatus.FramebufferComplete, "Failed to create framebuffer: {0}", status);

            if (shouldBind)
                GL.BindFramebuffer(Target, _id);
        }

        public void Resize(int width, int height)
        {
            Insist.Assert(width > 0);
            Insist.Assert(height > 0);

            _width = width;
            _height = height;

            Invalidate();
        }

#if TODO
        public void ChangeAttachements(FramebufferAttachements newAttachements)
        {
            // Remove color attachement
            if (_attachements.HasFlag(FramebufferAttachements.Color) && !newAttachements.HasFlag(FramebufferAttachements.Color))
            {
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.ColorAttachment0, 0, 0);
                GL.NamedFramebufferDrawBuffer(_id, DrawBufferMode.None);
                GL.NamedFramebufferReadBuffer(_id, ReadBufferMode.None);
            }
            // Add color attachement
            else if (!_attachements.HasFlag(FramebufferAttachements.Color) && newAttachements.HasFlag(FramebufferAttachements.Color))
            {
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.ColorAttachment0, 0, 0);
                GL.NamedFramebufferDrawBuffer(_id, DrawBufferMode.None);
                GL.NamedFramebufferReadBuffer(_id, ReadBufferMode.None);
            }

            if (_attachements.HasFlag(FramebufferAttachements.DepthStencil))
            {
                _depthStencil = new Texture2D(width, height, DepthStencilTextureBuilder);
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.DepthStencilAttachment, _depthStencil.ID, 0);
            }
            else if (_attachements.HasFlag(FramebufferAttachements.Depth))
            {
                _depthStencil = new Texture2D(width, height, DepthTextureBuilder);
                GL.NamedFramebufferTexture(_id, FramebufferAttachment.DepthAttachment, _depthStencil.ID, 0);
            }
        }
#endif

        public void Bind()
        {
            if(CurrentlyBinded != _id)
            {
                GL.BindFramebuffer(Target, _id);
                RenderCommand.SetViewport(0, 0, _width, _height);
                CurrentlyBinded = _id;
            }
        }

        public void Unbind()
        {
            if (CurrentlyBinded == _id)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                RenderCommand.SetViewportSize(App.Width, App.Height);
                CurrentlyBinded = 0;
            }
        }

        public static void BindMain()
        {
            if (CurrentlyBinded != 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                RenderCommand.SetViewportSize(App.Width, App.Height);
                CurrentlyBinded = 0;
            }
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                Unbind();

                Color?.Dispose();
                DepthStencil?.Dispose();

                GL.DeleteFramebuffer(_id);
                base.Dispose();
            }
        }
    }
}
