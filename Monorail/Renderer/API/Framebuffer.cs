using System;
using Monorail.Debug;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Monorail.Renderer
{
    public enum AttachmentType
    {
        DepthAttachment = FramebufferAttachment.DepthAttachment,
        DepthStencilAttachment = FramebufferAttachment.DepthStencilAttachment,
        StencilAttachment = FramebufferAttachment.StencilAttachment,
        Color0 = FramebufferAttachment.ColorAttachment0,
        Color1 = FramebufferAttachment.ColorAttachment1,
        Color2 = FramebufferAttachment.ColorAttachment2,
        Color3 = FramebufferAttachment.ColorAttachment3,
        Color4 = FramebufferAttachment.ColorAttachment4,
        Color5 = FramebufferAttachment.ColorAttachment5,
        Color6 = FramebufferAttachment.ColorAttachment6,
        Color7 = FramebufferAttachment.ColorAttachment7,
        Color8 = FramebufferAttachment.ColorAttachment8,
        Color9 = FramebufferAttachment.ColorAttachment9,
        Color10 = FramebufferAttachment.ColorAttachment10,
        Color11 = FramebufferAttachment.ColorAttachment11,
        Color12 = FramebufferAttachment.ColorAttachment12,
        Color13 = FramebufferAttachment.ColorAttachment13,
        Color14 = FramebufferAttachment.ColorAttachment14,
        Color15 = FramebufferAttachment.ColorAttachment15,
    }

    public class FramebufferAttachmentTexture
    {
        readonly AttachmentType _attachment;

        Texture2D _texture;

        public int ID => _texture.ID;

        TextureBuilder _builder;

        public FramebufferAttachmentTexture(int width, int height, AttachmentType attachment, TextureBuilder builder = null)
        {
            _attachment = attachment;

            if (builder == null)
            {
                _builder = _attachment switch
                {
                    AttachmentType.DepthStencilAttachment => new TextureBuilder()
                    {
                        InternalFormat = PixelInternalFormat.Depth24Stencil8,
                        PixelFormat = PixelFormat.DepthStencil,
                        PixelType = PixelType.UnsignedInt248,
                    },
                    AttachmentType.DepthAttachment => new TextureBuilder()
                    {
                        InternalFormat = PixelInternalFormat.DepthComponent,
                        PixelFormat = PixelFormat.DepthComponent,
                        PixelType = PixelType.Float,
                    },
                    AttachmentType.StencilAttachment => new TextureBuilder()
                    {
                        InternalFormat = PixelInternalFormat.R8,
                        PixelFormat = PixelFormat.StencilIndex,
                        PixelType = PixelType.Byte,
                    },
                    _ => new TextureBuilder(),
                };
            }
            else _builder = builder;

            Invalidate(width, height);
        }

        public void Invalidate(int width, int height)
        {
            _texture?.Dispose();

            _texture = new Texture2D(width, height, _builder);
        }

        public void Link(Framebuffer framebuffer)
        {
            GL.NamedFramebufferTexture(framebuffer.ID, (FramebufferAttachment)_attachment, _texture.ID, 0);
        }

        public void Dispose()
        {
            _texture?.Dispose();
            _texture = null;
        }
    }

    public class Framebuffer : OpenGLResource
    {
        public static int CurrentlyBinded { get; protected set; } = 0;

        public static FramebufferTarget CurrentTarget { get; protected set; } = FramebufferTarget.Framebuffer;

        private Dictionary<AttachmentType, FramebufferAttachmentTexture> _attachments = new Dictionary<AttachmentType, FramebufferAttachmentTexture>();

        public FramebufferTarget Target
        {
            get => _target;
            // TODO: when changing target only recreate framebuffer and not all attachments
            //set
            //{
            //    if (_target != value)
            //    {
            //        _target = value;
            //        Invalidate();
            //    }
            //}
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

        public Vector2i Size
        {
            get => new Vector2i(Width, Height);
            set => Resize(value.X, value.Y);
        }

        FramebufferTarget _target = FramebufferTarget.Framebuffer;

        int _width, _height;
        List<DrawBuffersEnum> _colors = new List<DrawBuffersEnum>();

        public Framebuffer(int width, int height, params AttachmentType[] attachments)
        {
            Insist.Assert(width > 0);
            Insist.Assert(height > 0);

            _width = width;
            _height = height;

            _target = FramebufferTarget.Framebuffer;

            if (attachments.Length == 0)
                _attachments.Add(AttachmentType.Color0, null);
            else
                foreach (var attachment in attachments)
                    _attachments.Add(attachment, null);

            CheckIfHasColor();
            Invalidate();
        }

        void CheckIfHasColor()
        {
            _colors.Clear();
            for (int i = 0; i < 16; ++i)
            {
                var color = FramebufferAttachment.ColorAttachment0 + i;
                if (_attachments.ContainsKey((AttachmentType)color))
                {
                    _colors.Add(DrawBuffersEnum.ColorAttachment0 + i);
                }
            }
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
                GL.DeleteFramebuffer(_id);
            }

            GL.CreateFramebuffers(1, out _id);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Framebuffer);

            foreach (var key in _attachments.Keys)
            {
                if (_attachments[key] == null)
                    _attachments[key] = new FramebufferAttachmentTexture(_width, _height, key);
                else
                    _attachments[key].Invalidate(_width, _height);
                _attachments[key].Link(this);
            }

            SetDrawBuffers();

            FramebufferStatus status = GL.CheckNamedFramebufferStatus(_id, Target);
            Insist.AssertEq(status, FramebufferStatus.FramebufferComplete, "Failed to create framebuffer: {0}", status);

            if (shouldBind) GL.BindFramebuffer(Target, _id);
        }

        public void Resize(int width, int height)
        {
            Insist.Assert(width > 0, "Framebuffer must be wider than 0px.");
            Insist.Assert(height > 0, "Framebuffer must be larger than 0px.");

            if (_width != width || _height != height)
            {
                _width = width;
                _height = height;

                Invalidate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(Vector2 size) => Resize((int)size.X, (int)size.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(Vector2i size) => Resize(size.X, size.Y);

        public void AddAttachment(AttachmentType attachment)
        {
            if (!_attachments.ContainsKey(attachment))
            {
                var attach= new FramebufferAttachmentTexture(_width, _height, attachment);
                attach.Link(this);
                _attachments.Add(attachment, attach);
                CheckIfHasColor();
                SetDrawBuffers();
            }
        }

        public void SetAttachmentTextureBuilder(AttachmentType attachment, TextureBuilder builder)
        {
            if (attachment == AttachmentType.DepthAttachment ||
                attachment == AttachmentType.DepthStencilAttachment ||
                attachment == AttachmentType.StencilAttachment)
                return;

            if (_attachments.ContainsKey(attachment))
                _attachments[attachment].Dispose();
            _attachments[attachment] = new FramebufferAttachmentTexture(_width, _height, attachment, builder);
        }

        public void RemoveAttachment(AttachmentType attachment)
        {
            // The framebuffer must have at least 1 attachment
            if (_attachments.Count == 1 && _attachments.TryGetValue(attachment, out var value))
            {
                value.Dispose();
                _attachments.Remove(attachment);
                CheckIfHasColor();
                SetDrawBuffers();
            }
        }

        private void SetDrawBuffers()
        {
            if (_colors.Count == 0)
            {
                GL.NamedFramebufferDrawBuffer(_id, DrawBufferMode.None);
                GL.NamedFramebufferReadBuffer(_id, ReadBufferMode.None);
            }
            else
            {
                GL.NamedFramebufferDrawBuffers(_id, _colors.Count, _colors.ToArray());
            }
        }

        public int? AttachmentTextureID(AttachmentType attachment)
        {
            if (_attachments.TryGetValue(attachment, out var value)) return value.ID;
            return null;
        }

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

                foreach (var attachment in _attachments)
                    attachment.Value?.Dispose();

                GL.DeleteFramebuffer(_id);
                base.Dispose();
            }
        }
    }
}
