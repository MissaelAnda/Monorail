using System;
using Monorail.Util;
using System.Drawing;
using Monorail.Debug;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace Monorail.Renderer
{
    public enum TextureCoordinate
    {
        S = TextureParameterName.TextureWrapS,
        T = TextureParameterName.TextureWrapT,
        R = TextureParameterName.TextureWrapR
    }


    public class TextureBuilder
    {
        public static TextureBuilder Default => new TextureBuilder();

        public bool GenerateMipmaps = false;
        public PixelInternalFormat InternalFormat = PixelInternalFormat.Rgba8;
        public PixelFormat PixelFormat = PixelFormat.Bgra;
        public PixelType PixelType = PixelType.UnsignedByte;

        public TextureWrapMode WrapR = TextureWrapMode.Repeat;
        public TextureWrapMode WrapS = TextureWrapMode.Repeat;
        public TextureWrapMode WrapT = TextureWrapMode.Repeat;

        public TextureMagFilter MagFilter = TextureMagFilter.Linear;
        public TextureMinFilter MinFilter = TextureMinFilter.Linear;

        public Color4 BorderColor = Color4.White;
    }


    public class Texture2D : OpenGLResource
    {
        public const int MAX_TEXTURE_POSITIONS = 32;
        public const SizedInternalFormat SRGB8ALPHA8 = (SizedInternalFormat)All.Srgb8Alpha8;
        public const TextureTarget TARGET = TextureTarget.Texture2D;

        public static int[] BindedTextures { get; private set; } = new int[MAX_TEXTURE_POSITIONS];

        public const GetPName MAX_TEXTURE_MAX_ANISOTROPIC = (GetPName)0x84FF;

        public static readonly float MaxAnisotropic;

        static Texture2D()
        {
            MaxAnisotropic = GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPIC);
        }

        public readonly int MipmapLevels;


        #region getters and setters

        public int Width
        {
            get => _width;
            set => SetWidth(value);
        }

        public int Height
        {
            get => _height;
            set => SetHeight(value);
        }

        public TextureWrapMode WrapR
        {
            get => _wrapR;
            set => SetWrapMode(TextureCoordinate.R, value);
        }

        public TextureWrapMode WrapS
        {
            get => _wrapS;
            set => SetWrapMode(TextureCoordinate.S, value);
        }

        public TextureWrapMode WrapT
        {
            get => _wrapT;
            set => SetWrapMode(TextureCoordinate.T, value);
        }

        public TextureMagFilter MagFilter
        {
            get => _magFilter;
            set => SetMagFilter(value);
        }

        public TextureMinFilter MinFilter
        {
            get => _minFilter;
            set => SetMinFilter(value);
        }

        public float Anisotropic
        {
            get => _anisotropic;
            set => SetAnisotropic(value);
        }

        public Color4 BorderColor
        {
            get => _borderColor;
            set => SetBorderColor(value);
        }

        #endregion


        int _bindedPositions = 0;
        float _anisotropic = 0f;

        TextureWrapMode _wrapR;
        TextureWrapMode _wrapS;
        TextureWrapMode _wrapT;

        TextureMagFilter _magFilter;
        TextureMinFilter _minFilter;

        int _width, _height;

        Color4 _borderColor;

        PixelInternalFormat _internalFormat;
        PixelFormat _pixelFormat;
        PixelType _pixelType;

        public static Texture2D FromPath(string path, TextureBuilder builder)
        {
            Bitmap image;
            try { image = new Bitmap(path); }
            catch (Exception e)
            {
                Insist.Fail(e.ToString());
                throw e;
            }

            return Texture2D.FromBitmap(image, builder);
        }

        public static Texture2D FromBitmap(Bitmap image, TextureBuilder builder)
        {
            var texture = new Texture2D(image.Width, image.Height, builder);

            image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            texture.SetPixels(data.Scan0);
            image.UnlockBits(data);

            return texture;
        }

        public Texture2D(int width, int height, TextureBuilder builder)
        {
            Insist.Assert(width > 0);
            Insist.Assert(height > 0);
            _width = width;
            _height = height;
            _internalFormat = builder.InternalFormat;
            _pixelFormat = builder.PixelFormat;
            _pixelType = builder.PixelType;

            GL.CreateTextures(TARGET, 1, out _id);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Texture);

            if (builder.GenerateMipmaps)
                MipmapLevels = (int)MathHelper.Floor(MathHelper.Log(MathHelper.Max(width, height), 2));
            else MipmapLevels = 1;

            var old = BindedTextures[0];
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TARGET, _id);
            GL.TexImage2D(TARGET, 0, _internalFormat, _width, _height, 0, _pixelFormat, _pixelType, (IntPtr)null);
            GL.BindTexture(TARGET, old);

            GL.TextureParameter(_id, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

            SetDefaults(builder);
        }

        public void SetPixels(IntPtr pixels, PixelFormat? pixelFormat = null, PixelType? pixelType = null)
        {
            if (pixelFormat.HasValue) _pixelFormat = pixelFormat.Value;
            if (pixelType.HasValue) _pixelType = pixelType.Value;

            GL.TextureSubImage2D(_id, 0, 0, 0, Width, Height, _pixelFormat, _pixelType, pixels);

            if (MipmapLevels > 1) GL.GenerateTextureMipmap(_id);
        }

        public void SetPixelsRectangle(IntPtr pixels, Rectangle target, PixelFormat? pixelFormat = null, PixelType? pixelType = null)
        {
            Insist.Assert(target.X > 0);
            Insist.Assert(target.Y > 0);
            Insist.Assert(target.Width <= Width);
            Insist.Assert(target.Height <= Height);

            var format = pixelFormat.HasValue ? pixelFormat.Value : _pixelFormat;
            var type = pixelType.HasValue ? pixelType.Value : _pixelType;

            GL.TextureSubImage2D(_id, 0, target.X, target.Y, target.Width, target.Height, format, type, pixels);

            if (MipmapLevels > 1) GL.GenerateTextureMipmap(_id);
        }

        public void Resize(int width, int height)
        {
            Insist.Assert(width > 0);
            Insist.Assert(height > 0);
            _width = width;
            _height = height;

            var old = BindedTextures[0];
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TARGET, _id);
            GL.TexImage2D(TARGET, 0, PixelInternalFormat.Rgba8, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            GL.BindTexture(TARGET, old);
        }

        void SetDefaults(TextureBuilder builder)
        {
            WrapR = builder.WrapR;
            WrapS = builder.WrapS;
            WrapT = builder.WrapT;

            MagFilter = builder.MagFilter;
            MinFilter = builder.MinFilter;

            BorderColor = builder.BorderColor;
        }


        #region Bindings

        public void Bind(TextureUnit position)
        {
            int pos = position - TextureUnit.Texture0;
            if (BindedTextures[pos] != _id)
                SetPosition(pos, true);
        }

        public void Unbind(TextureUnit position)
        {
            int pos = position - TextureUnit.Texture0;
            if (BindedTextures[pos] == _id)
                SetPosition(pos, false);
        }

        public void Unbind()
        {
            for (int i = 0; i < MAX_TEXTURE_POSITIONS; i++)
            {
                if (Flags.GetBit(_bindedPositions, i))
                    SetPosition(i, false);
            }
        }

        public static void UnbindPosition(TextureUnit position)
        {
            int pos = position - TextureUnit.Texture0;
            if (BindedTextures[pos] != 0)
            {
                GL.ActiveTexture(position);
                GL.BindTexture(TARGET, 0);
                BindedTextures[pos] = 0;
            }
        }

        public static void UnbindAll()
        {
            for (int i = 0; i < 32; i++)
            {
                if (BindedTextures[i] != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    GL.BindTexture(TARGET, 0);
                    BindedTextures[i] = 0;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetPosition(int pos, bool binded)
        {
            var id = binded ? _id : 0;
            BindedTextures[pos] = id;
            Flags.SetBit(ref _bindedPositions, pos, binded);

            GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + pos));
            GL.BindTexture(TARGET, id);
        }

        #endregion


        #region Fluent Setters

        public Texture2D SetWrapMode(TextureCoordinate coord, TextureWrapMode mode)
        {
            switch (coord)
            {
                case TextureCoordinate.S:
                    _wrapS = mode;
                    break;
                case TextureCoordinate.R:
                    _wrapR = mode;
                    break;
                case TextureCoordinate.T:
                    _wrapT = mode;
                    break;
            }

            GL.TextureParameter(_id, (TextureParameterName)coord, (int)mode);
            return this;
        }

        public Texture2D SetMagFilter(TextureMagFilter filter)
        {
            _magFilter = filter;
            GL.TextureParameter(_id, TextureParameterName.TextureMagFilter, (int)filter);
            return this;
        }

        public Texture2D SetMinFilter(TextureMinFilter filter)
        {
            if (MipmapLevels > 1)
            {
                if (filter == TextureMinFilter.Linear)
                    filter = TextureMinFilter.LinearMipmapLinear;
                else if (filter == TextureMinFilter.Nearest)
                    filter = TextureMinFilter.NearestMipmapNearest;
            }

            _minFilter = filter;
            GL.TextureParameter(_id, TextureParameterName.TextureMinFilter, (int)filter);
            return this;
        }

        public Texture2D SetAnisotropic(float level)
        {
            const TextureParameterName TEXTURE_MAX_ANISOTROPIC = (TextureParameterName)0x84FE;
            _anisotropic = MathHelper.Clamp(level, 1, MaxAnisotropic);
            GL.TextureParameter(_id, TEXTURE_MAX_ANISOTROPIC, _anisotropic);
            return this;
        }

        public Texture2D SetLod(int @base, int min, int max)
        {
            GL.TextureParameter(_id, TextureParameterName.TextureLodBias, @base);
            GL.TextureParameter(_id, TextureParameterName.TextureMinLod, min);
            GL.TextureParameter(_id, TextureParameterName.TextureMaxLod, max);
            return this;
        }

        public Texture2D SetBorderColor(Color4 color)
        {
            _borderColor = color;
            GL.TextureParameter(_id, TextureParameterName.TextureBorderColor, color.ToArray());
            return this;
        }

        public Texture2D SetWidth(int width)
        {
            Resize(width, _height);
            return this;
        }

        public Texture2D SetHeight(int height)
        {
            Resize(_width, height);
            return this;
        }

        #endregion


        public override void Dispose()
        {
            if (!_disposed)
            {
                Unbind();

                GL.DeleteTexture(_id);
                base.Dispose();
            }
        }


        public static implicit operator IntPtr(Texture2D tex) => (IntPtr)tex.ID;

        public static implicit operator int(Texture2D tex) => tex.ID;
    }
}
