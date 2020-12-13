using System;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using Monorail.Debug;
using OpenTK.Mathematics;

namespace Monorail.ImGUI
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        const string VertexSource = @"#version 330 core
            uniform mat4 projection_matrix;
            layout(location = 0) in vec2 in_position;
            layout(location = 1) in vec2 in_texCoord;
            layout(location = 2) in vec4 in_color;
            out vec4 color;
            out vec2 texCoord;
            void main()
            {
                gl_Position = projection_matrix * vec4(in_position, 0, 1);
                color = in_color;
                texCoord = in_texCoord;
            }";

        const string FragmentSource = @"#version 330 core
            uniform sampler2D in_fontTexture;
            in vec4 color;
            in vec2 texCoord;
            out vec4 outputColor;
            void main()
            {
                outputColor = color * texture(in_fontTexture, texCoord);
            }";

        private bool _frameBegun;

        // Veldrid objects
        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;
        private int _shaderProgram;
        private int _texture;

        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        readonly List<char> PressedChars = new List<char>();

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height, GameWindow gw)
        {
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            ImGui.StyleColorsDark();
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            GL.CreateVertexArrays(1, out _vertexArray);

            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            GL.CreateBuffers(1, out _vertexBuffer);
            GL.CreateBuffers(1, out _indexBuffer);

            GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            // Compile Shaders
            {
                int vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, VertexSource);
                GL.CompileShader(vertexShader);

                string infoLogVert = GL.GetShaderInfoLog(vertexShader);
                if (infoLogVert != String.Empty)
                    throw new Exception($"Failed to compile Vertex Shader: {infoLogVert}");

                int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, FragmentSource);
                GL.CompileShader(fragmentShader);

                infoLogVert = GL.GetShaderInfoLog(fragmentShader);
                if (infoLogVert != String.Empty)
                    throw new Exception($"Failed to compile Fragment Shader: {infoLogVert}");

                _shaderProgram = GL.CreateProgram();
                GL.AttachShader(_shaderProgram, vertexShader);
                GL.AttachShader(_shaderProgram, fragmentShader);

                GL.LinkProgram(_shaderProgram);

                GL.DetachShader(_shaderProgram, vertexShader);
                GL.DetachShader(_shaderProgram, fragmentShader);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
            }

            // setup Vertex Array Attrib Pointers
            {
                GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
                GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

                GL.EnableVertexArrayAttrib(_vertexArray, 0);
                GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
                GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

                GL.EnableVertexArrayAttrib(_vertexArray, 1);
                GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
                GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

                GL.EnableVertexArrayAttrib(_vertexArray, 2);
                GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
                GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);
            }
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            //InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;
            var MipmapLevels = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            GL.CreateTextures(TextureTarget.Texture2D, 1, out _texture);
            //GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TextureStorage2D(_texture, MipmapLevels, SizedInternalFormat.Rgba8, width, height);

            GL.TextureSubImage2D(_texture, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.TextureParameter(_texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TextureParameter(_texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TextureParameter(_texture, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

            GL.TextureParameter(_texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TextureParameter(_texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //GL.BindTexture(TextureTarget.Texture2D, 0);

            io.Fonts.SetTexID((IntPtr)_texture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, double deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(double deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = (float)deltaSeconds; // DeltaTime is in seconds.
        }

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = wnd.MouseState.GetSnapshot();
            KeyboardState KeyboardState = wnd.KeyboardState.GetSnapshot();

            io.MouseDown[0] = MouseState.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = MouseState.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = MouseState.IsButtonDown(MouseButton.Middle);
            io.MousePos = new System.Numerics.Vector2((int)MouseState.Position.X, (int)MouseState.Position.Y);

            
            io.MouseWheel = MouseState.ScrollDelta.Y;
            io.MouseWheelH = MouseState.ScrollDelta.X;

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown) continue;
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            //uint vertexOffsetInVertices = 0;
            //uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Log.Core.Info($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    Log.Core.Info($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f, -1.0f, 1.0f
            );

            GL.UseProgram(_shaderProgram);

            var loc = GL.GetUniformLocation(_shaderProgram, "projection_matrix");
            if (loc < 0) Log.Core.Error("Uniform 'projection_matrix' not found.");
            GL.UniformMatrix4(loc, false, ref mvp);

            loc = GL.GetUniformLocation(_shaderProgram, "in_fontTexture");
            if (loc < 0) Log.Core.Error("Uniform 'in_fontTexture' not found.");
            GL.Uniform1(loc, 0);

            GL.BindVertexArray(_vertexArray);

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);

                GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

                int vtx_offset = 0;
                int idx_offset = 0;

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteTexture(_texture);
        }
    }
}
