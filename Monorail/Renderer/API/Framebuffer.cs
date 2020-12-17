using Monorail.Debug;
using OpenTK.Graphics.OpenGL4;
using System;

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

        public readonly FramebufferTarget Target;

        public FramebufferAttachements Attachements;

        //public Texture Color { get; protected set; }
        //public Texture DepthStencil { get; protected set; }

        public int Color { get; protected set; }
        public int DepthStencil { get; protected set; }

        int _width, _height;

        public Framebuffer(int width, int height, FramebufferAttachements attachements = FramebufferAttachements.Color)
        {
            _width = width;
            _height = height;
            Attachements = attachements;

            Target = FramebufferTarget.Framebuffer;

            //GL.CreateFramebuffers(1, out _id);
            _id = GL.GenFramebuffer();
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Framebuffer);

            GL.BindFramebuffer(Target, _id);

            int oldText = Texture.BindedTextures[0];
            GL.ActiveTexture(TextureUnit.Texture0);
            if (attachements.HasFlag(FramebufferAttachements.Color))
            {
                Color = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, Color);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.NamedFramebufferTexture(_id, FramebufferAttachment.ColorAttachment0, Color, 0);
            }
            else
            {
                GL.NamedFramebufferDrawBuffer(_id, DrawBufferMode.None);
                GL.NamedFramebufferReadBuffer(_id, ReadBufferMode.None);
            }

            if (attachements.HasFlag(FramebufferAttachements.DepthStencil))
            {
                DepthStencil = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, DepthStencil);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.NamedFramebufferTexture(_id, FramebufferAttachment.DepthStencilAttachment, DepthStencil, 0);
            }
            else if (attachements.HasFlag(FramebufferAttachements.Depth))
            {
                DepthStencil = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, DepthStencil);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.NamedFramebufferTexture(_id, FramebufferAttachment.DepthAttachment, DepthStencil, 0);
            }
            GL.BindTexture(TextureTarget.Texture2D, oldText);

            FramebufferStatus status = GL.CheckNamedFramebufferStatus(_id, Target);
            Insist.AssertEq(status, FramebufferStatus.FramebufferComplete, "Failed to create framebuffer: {0}", status);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            if(CurrentlyBinded != _id)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _id);
                GL.Viewport(0, 0, _width, _height);
                CurrentlyBinded = _id;
            }
        }

        public void Unbind()
        {
            if (CurrentlyBinded == _id)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, App.Width, App.Height);
                CurrentlyBinded = 0;
            }
        }

        public static void BindMain()
        {
            if (CurrentlyBinded != 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, App.Width, App.Height);
                CurrentlyBinded = 0;
            }
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                Unbind();

                if (Attachements.HasFlag(FramebufferAttachements.Color))
                    GL.DeleteTexture(Color);
                
                if (Attachements.HasFlag(FramebufferAttachements.Depth) || Attachements.HasFlag(FramebufferAttachements.DepthStencil))
                    GL.DeleteTexture(DepthStencil);

                GL.DeleteFramebuffer(_id);
                base.Dispose();
            }
        }
    }
}
