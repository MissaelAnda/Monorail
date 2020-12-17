using System;
using Monorail.Util;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using System.Drawing;
using Monorail.Debug;

namespace Monorail.Renderer
{
    public enum TextureCoordinate
    {
        S = TextureParameterName.TextureWrapS,
        T = TextureParameterName.TextureWrapT,
        R = TextureParameterName.TextureWrapR
    }


    public class Texture : OpenGLResource
    {
        public const int MAX_TEXTURE_POSITIONS = 32;
        public const SizedInternalFormat SRGB8ALPHA8 = (SizedInternalFormat)All.Srgb8Alpha8;

        public static int[] BindedTextures { get; private set; } = new int[MAX_TEXTURE_POSITIONS];

        public const GetPName MAX_TEXTURE_MAX_ANISOTROPIC = (GetPName)0x84FF;

        public static readonly float MaxAnisotropic;

        static Texture()
        {
            MaxAnisotropic = GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPIC);
        }


        public readonly TextureTarget Target;
        public readonly int Width;
        public readonly int Height;
        public readonly int MipmapLevels;
        public readonly SizedInternalFormat InternalFormat;

        #region getters and setters

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

        #endregion


        int _bindedPositions = 0;
        float _anisotropic = 0f;

        TextureWrapMode _wrapR = TextureWrapMode.Repeat;
        TextureWrapMode _wrapS = TextureWrapMode.Repeat;
        TextureWrapMode _wrapT = TextureWrapMode.Repeat;

        TextureMagFilter _magFilter = TextureMagFilter.Linear;
        TextureMinFilter _minFilter = TextureMinFilter.LinearMipmapLinear;

        Color4 _borderColor = Color4.Black;

        public static Texture FromPath(string path, TextureTarget target, SizedInternalFormat? internalFormat = null, bool generateMipmaps = false)
        {
            Bitmap image;
            try { image = new Bitmap(path); }
            catch (Exception e)
            {
                Insist.Fail(e.ToString());
                throw e;
            }

            return new Texture(image, target, internalFormat, generateMipmaps);
        }

        public Texture(Bitmap image, TextureTarget target, SizedInternalFormat? internalFormat = null, bool generateMipmaps = false) :
            this(target, image.Width, image.Height, internalFormat, generateMipmaps)
        {
            System.Drawing.Imaging.BitmapData data = image.LockBits(new Rectangle(0, 0, Width, Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            SetPixels(data.Scan0, PixelFormat.Bgra, PixelType.UnsignedByte);

            image.UnlockBits(data);
        }

        public Texture(TextureTarget target, int width, int height, SizedInternalFormat? internalFormat = null, bool generateMipmaps = false)
        {
            Target = target;
            Width = width;
            Height = height;

            GL.CreateTextures(target, 1, out _id);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Texture);

            InternalFormat = internalFormat.HasValue ? internalFormat.Value : SRGB8ALPHA8;

            if (generateMipmaps)
                MipmapLevels = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
            else
                MipmapLevels = 1;

            GL.TextureStorage2D(_id, MipmapLevels, InternalFormat, width, height);
            GL.TextureParameter(_id, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

            SetDefaults();
        }

        public void SetPixels(IntPtr pixels, PixelFormat format, PixelType type)
        {
            GL.TextureSubImage2D(_id, 0, 0, 0, Width, Height, format, type, pixels);

            if (MipmapLevels > 1) GL.GenerateTextureMipmap(_id);
        }

        public void SetPixelsRectangle(IntPtr pixels, PixelFormat format, PixelType type, Rectangle target)
        {
            Insist.Assert(target.X > 0);
            Insist.Assert(target.Y > 0);
            Insist.Assert(target.Width <= Width);
            Insist.Assert(target.Height <= Height);

            GL.TextureSubImage2D(_id, 0, target.X, target.Y, target.Width, target.Height, format, type, pixels);

            if (MipmapLevels > 1) GL.GenerateTextureMipmap(_id);
        }

        void SetDefaults()
        {
            WrapR = TextureWrapMode.Repeat;
            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;

            MagFilter = TextureMagFilter.Linear;
            MinFilter = TextureMinFilter.Linear;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetPosition(int pos, bool binded)
        {
            var id = binded ? _id : 0;
            BindedTextures[pos] = id;
            Flags.SetBit(ref _bindedPositions, pos, binded);

            GL.ActiveTexture(TextureUnit.Texture0 + pos);
            GL.BindTexture(TextureTarget.Texture2D, id);
        }

        #endregion


        #region Fluent Setters

        public Texture SetWrapMode(TextureCoordinate coord, TextureWrapMode mode)
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

        public Texture SetMagFilter(TextureMagFilter filter)
        {
            GL.TextureParameter(_id, TextureParameterName.TextureMagFilter, (int)filter);
            return this;
        }

        public Texture SetMinFilter(TextureMinFilter filter)
        {
            if (MipmapLevels > 1)
            {
                if (filter == TextureMinFilter.Linear)
                    filter = TextureMinFilter.LinearMipmapLinear;
                else if (filter == TextureMinFilter.Nearest)
                    filter = TextureMinFilter.NearestMipmapNearest;
            }

            GL.TextureParameter(_id, TextureParameterName.TextureMinFilter, (int)filter);
            return this;
        }

        public Texture SetAnisotropic(float level)
        {
            const TextureParameterName TEXTURE_MAX_ANISOTROPIC = (TextureParameterName)0x84FE;
            _anisotropic = MathHelper.Clamp(level, 1, MaxAnisotropic);
            GL.TextureParameter(_id, TEXTURE_MAX_ANISOTROPIC, _anisotropic);
            return this;
        }

        public Texture SetLod(int @base, int min, int max)
        {
            GL.TextureParameter(_id, TextureParameterName.TextureLodBias, @base);
            GL.TextureParameter(_id, TextureParameterName.TextureMinLod, min);
            GL.TextureParameter(_id, TextureParameterName.TextureMaxLod, max);
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
    }
}
