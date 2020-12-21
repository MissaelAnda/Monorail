using System;
using ImGuiNET;
using Monorail.Debug;
using Monorail.Renderer;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Monorail.Layers.ImGUI
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
        private VertexArray _vao;
        private int _vertexBufferSize;
        private int _indexBufferSize;
        private ShaderProgram _shaderProgram;
        private Texture2D _texture;

        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        readonly List<char> PressedChars = new List<char>();

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
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

        public void CreateDeviceResources()
        {
            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;
            
            var vertexBuffer = new VertexBuffer();
            vertexBuffer.SetElementSize(Unsafe.SizeOf<ImDrawVert>());
            vertexBuffer.AllocateEmpty(_vertexBufferSize, BufferUsageHint.DynamicDraw);

            var indexBuffer = new IndexBuffer();
            indexBuffer.SetElementSize(sizeof(ushort));
            indexBuffer.ElementsType = DrawElementsType.UnsignedShort;
            indexBuffer.AllocateEmpty(_indexBufferSize, BufferUsageHint.DynamicDraw);

            var imguiVertexLayout = new VertexLayout();
            imguiVertexLayout.AddAttrib(new VertexAttrib("aPosition", VertexAttribDataType.Float2))
                .AddAttrib(new VertexAttrib("aUV", VertexAttribDataType.Float2))
                .AddAttrib(new VertexAttrib("aColor", VertexAttribDataType.UnsignedByte4, true));

            VertexArray.AddVertexLayout("ImGUI_Layout", imguiVertexLayout);

            _vao = new VertexArray("ImGUI_Layout", vertexBuffer, indexBuffer);

            RecreateFontDeviceTexture();

            // Compile Shaders
            var vertexShader = Shader.FromSource(VertexSource, ShaderType.VertexShader);
            var fragmentShader = Shader.FromSource(FragmentSource, ShaderType.FragmentShader);

            _shaderProgram = new ShaderProgram();
            _shaderProgram.AttachShader(vertexShader);
            _shaderProgram.AttachShader(fragmentShader);

            _shaderProgram.LinkProgram();

            _shaderProgram.DetachAllShaders();

            vertexShader.Dispose();
            fragmentShader.Dispose();

            _shaderProgram.SetUniform1("in_fontTexture", 0);
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int _);

            var textureBuilder = new TextureBuilder()
            {
                InternalFormat = PixelInternalFormat.Srgb8Alpha8,
                PixelFormat = PixelFormat.Bgra
            };
            _texture = new Texture2D(width, height, textureBuilder);
            _texture.SetPixels(pixels, PixelFormat.Bgra, PixelType.UnsignedByte);

            io.Fonts.SetTexID((IntPtr)_texture.ID);

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
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                if (cmd_list.VtxBuffer.Size > _vao.VertexBuffer.DataLength)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, cmd_list.VtxBuffer.Size);
                    _vao.VertexBuffer.AllocateEmpty(newSize, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Log.Core.Info($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                if (cmd_list.IdxBuffer.Size > _vao.IndexBuffer.DataLength)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, cmd_list.IdxBuffer.Size);
                    _vao.IndexBuffer.AllocateEmpty(newSize, BufferUsageHint.DynamicDraw);
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

            _shaderProgram.SetUniformMat4("projection_matrix", false, ref mvp);
            _shaderProgram.Bind();
            _vao.Bind();

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            var oldTexture = Texture2D.BindedTextures[0];
            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                _vao.VertexBuffer.SetSubData(cmd_list.VtxBuffer.Size, cmd_list.VtxBuffer.Data);

                _vao.IndexBuffer.SetSubData(cmd_list.IdxBuffer.Size, cmd_list.IdxBuffer.Data);

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
            GL.BindTexture(TextureTarget.Texture2D, oldTexture);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        public void Dispose()
        {
            _vao.Dispose();
            _texture.Dispose();
            _shaderProgram.Dispose();
        }
    }
}
