using System;
using Monorail.Debug;
using System.Drawing;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Monorail.Renderer
{
    public static class RenderCommand
    {
        public static int DrawCalls { get; private set; } = 0;

        public static Color4 ClearColor
        {
            get => _clearColor;
            set => SetClearColor(value);
        }

        public static Rectangle Viewport
        {
            get => _viewport;
            set => SetViewport(value);
        }

        static ClearBufferMask _clearMasks = ClearBufferMask.ColorBufferBit;

        static List<EnableCap> _caps = new List<EnableCap>();

        static Color4 _clearColor;

        static Rectangle _viewport = new Rectangle(0, 0, 0, 0);

        static DebugProc _debugCallback;

        static RenderCommand()
        {
            ClearColor = Color4.Magenta;

            _debugCallback = GLDebugCallback;

            Log.Core.Succes("Engine started.");
            Log.Core.Info("Using OpenGL {0}", GL.GetString(StringName.Version));

            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(_debugCallback, (IntPtr)0);

            //GL.Enable(EnableCap.FramebufferSrgb);
        }

        public static void ResetDrawCalls() => DrawCalls = 0;

        public static void GLDebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
#nullable enable
            string? msg = Marshal.PtrToStringAnsi(message, length);
#nullable disable

            if (msg == null)
                msg = "Failed to decode error message.";

            string log = string.Format("OpenGL Message: [Source: - {0}] [Type: - {1}] [ID: - {2}]\n[Message: {3}]", source, type, id, msg);

            switch (severity)
            {
                case DebugSeverity.DebugSeverityHigh:
                    Log.Core.Error(log);
                    break;
                case DebugSeverity.DebugSeverityMedium:
                    Log.Core.Warn(log);
                    break;
                case DebugSeverity.DebugSeverityLow:
                    Log.Core.Notification(log);
                    break;
                default:
                    break;
            }
        }

        public static void SetClearMasks(ClearBufferMask masks) => _clearMasks = masks;

        public static void AddClearMasks(ClearBufferMask masks) => _clearMasks |= masks;

        public static void RemoveClearMasks(ClearBufferMask masks) => _clearMasks &= ~masks;

        public static void SetCaps(params EnableCap[] caps)
        {
            ResetCaps();
            _caps.AddRange(caps);
            EnableCaps();
        }

        public static void EnableCaps(params EnableCap[] caps)
        {
            foreach (var cap in caps)
            {
                if (!_caps.Contains(cap))
                {
                    GL.Enable(cap);
                    _caps.Add(cap);
                }
            }
        }

        public static void DisableCaps(params EnableCap[] caps)
        {
            foreach (var cap in caps)
            {
                GL.Disable(cap);
                _caps.Remove(cap);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetCaps()
        {
            DisableAllCaps();
            _caps.Clear();
        }

        static void DisableAllCaps()
        {
            for (int i = 0; i < _caps.Count; i++)
                GL.Disable(_caps[i]);
        }

        static void EnableCaps()
        {
            for (int i = 0; i < _caps.Count; i++)
                GL.Enable(_caps[i]);
        }

        /// <summary>
        /// Checks if the OpenGL capability is enabled
        /// </summary>
        /// <param name="cap"></param>
        /// <returns></returns>
        public static bool HasCap(EnableCap cap) => _caps.Contains(cap);

        public static void SetClearColor(Color4 color)
        {
            if (_clearColor != color)
            {
                GL.ClearColor(color);
                _clearColor = color;
            }
        }

        public static void Clear() => GL.Clear(_clearMasks);

        public static void DrawIndexed(VertexArray vao, BeginMode mode = BeginMode.Triangles, int? elements = null, int offset = 0)
        {
            Insist.AssertNotNull(vao.IndexBuffer);
            var elemCount = elements.HasValue ? elements.Value : vao.IndexBuffer.DataLength;

            vao.Bind();
            GL.DrawElements(mode, elemCount, vao.IndexBuffer.ElementsType, offset);
            DrawCalls++;
        }

        #region Viewport

        public static void SetViewport(Rectangle viewport)
        {
            _viewport = viewport;
            GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }

        public static void SetViewport(Vector2i position, Vector2i size) =>
            SetViewport(new Rectangle(position.X, position.Y, size.X, size.Y));

        public static void SetViewport(int x, int y, int width, int height) =>
            SetViewport(new Rectangle(x, y, width, height));

        public static void SetViewportPosition(Vector2i position) =>
            SetViewport(new Rectangle(position.X, position.Y, _viewport.Width, _viewport.Height));

        public static void SetViewportPosition(int x, int y) =>
            SetViewport(new Rectangle(x, y, _viewport.Width, _viewport.Height));

        public static void SetViewportSize(Vector2i size) =>
            SetViewport(new Rectangle(_viewport.X, _viewport.Y, size.X, size.Y));

        public static void SetViewportSize(int width, int height) =>
            SetViewport(new Rectangle(_viewport.X, _viewport.Y, width, height));

        #endregion
    }
}
