using System;
using Monorail.Debug;
using Monorail.Layers;
using Monorail.Renderer;
using OpenTK.Mathematics;
using Monorail.Layers.ImGUI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;
        Color4 ClearColor = Color4.CornflowerBlue;

        ShaderProgram shaderProgram, shaderUV;
        VertexArray _vao, _vaoQuad;
        public static Framebuffer Framebuffer;

        public static int Width { get; protected set; }
        public static int Height { get; protected set; }

        float[] VertexData =
        {
            -0.5f, -0.5f, 1.0f, 0.0f, 0.0f,
             0.0f,  0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
        };

        uint[] IndexData =
        {
            0, 1, 2
        };

        float[] VertexDataUV =
        {
            -1.0f, -1.0f, 0.0f, 0.0f,
            -1.0f,  1.0f, 0.0f, 1.0f,
             1.0f,  1.0f, 1.0f, 1.0f,
             1.0f, -1.0f, 1.0f, 0.0f,
        };

        uint[] IndexDataUV =
        {
            0, 1, 2, 2, 3, 0
        };

        string VertexShader = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec3 aColor;

            out vec3 Color;

            void main()
            {
                Color = aColor;
                gl_Position = vec4(aPosition, 0.0, 1.0);
            }";

        string FragmentShader = @"
            #version 330 core
            out vec4 FragColor;

            in vec3 Color;

            void main()
            {
                FragColor = vec4(Color, 0.5);
            }";


        string VertexShaderUV = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aUV;

            out vec2 UV;

            void main()
            {
                UV = aUV;
                gl_Position = vec4(aPosition, 0.0, 1.0);
            }";

        string FragmentShaderUV = @"
            #version 330 core
            out vec4 FragColor;

            in vec2 UV;

            uniform sampler2D tex;

            void main()
            {
                FragColor = texture(tex, UV);
            }";

        DebugProc _debugCallback;

        public App() : base(GameWindowSettings.Default, new NativeWindowSettings() { APIVersion = new Version(4, 5) })
        {
            Title = "Monorail";
            Size = new Vector2i(1600, 900);
            layerStack = new LayerStack(this);

            UpdateFrame += OnUpdate;

            _debugCallback = GLDebugCallback;
        }

        public void GLDebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
#nullable enable
            string? msg = Marshal.PtrToStringAnsi(message, length);
#nullable disable

            if (msg == null)
                msg = "Failed to decode error message.";

            string log = string.Format("OpenGL Message: [Source: - {0}] [Type: - {1}] [ID: - {2}]\n[Message: {3}]", source, type, id, msg);

            switch(severity)
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

        protected override void OnLoad()
        {
            base.OnLoad();

            Log.Core.Succes("Engine started.");
            Log.Core.Info("Using OpenGL {0}", GL.GetString(StringName.Version));

            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(_debugCallback, (IntPtr)0);

            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);

            var layout = new VertexLayout();
            layout.AddAttrib(new VertexAttrib("aPosition", VertexAttribDataType.Float2))
                .AddAttrib(new VertexAttrib("aColor", VertexAttribDataType.Float3));

            var uvLayout = new VertexLayout();
            uvLayout.AddAttrib(new VertexAttrib("aPosition", VertexAttribDataType.Float2))
                .AddAttrib(new VertexAttrib("aUV", VertexAttribDataType.Float2));

            VertexArray.AddVertexLayout("Simple2D", layout);
            VertexArray.AddVertexLayout("Quad", uvLayout);

            var vertexBuffer = new VertexBuffer();
            vertexBuffer.SetData(VertexData, BufferUsageHint.StaticDraw);

            var indexBuffer = new IndexBuffer();
            indexBuffer.SetData(IndexData, BufferUsageHint.StaticDraw);

            _vao = new VertexArray("Simple2D", vertexBuffer, indexBuffer);

            vertexBuffer = new VertexBuffer();
            vertexBuffer.SetData(VertexDataUV, BufferUsageHint.StaticDraw);

            indexBuffer = new IndexBuffer();
            indexBuffer.SetData(IndexDataUV, BufferUsageHint.StaticDraw);

            _vaoQuad = new VertexArray("Quad", vertexBuffer, indexBuffer);

            var vertex = Shader.FromSource(VertexShader, ShaderType.VertexShader);
            var fragment = Shader.FromSource(FragmentShader, ShaderType.FragmentShader);

            shaderProgram = new ShaderProgram();

            shaderProgram.AttachShader(vertex);
            shaderProgram.AttachShader(fragment);

            shaderProgram.LinkProgram();

            shaderProgram.DetachAllShaders();

            vertex.Dispose();
            fragment.Dispose();

            vertex = Shader.FromSource(VertexShaderUV, ShaderType.VertexShader);
            fragment = Shader.FromSource(FragmentShaderUV, ShaderType.FragmentShader);

            shaderUV = new ShaderProgram();
            shaderUV.AttachShader(vertex);
            shaderUV.AttachShader(fragment);
            shaderUV.LinkProgram();
            shaderUV.DetachAllShaders();

            vertex.Dispose();
            fragment.Dispose();
            shaderUV.SetUniform1("tex", 0);

            // Create framebuffer
            Framebuffer = new Framebuffer(800, 600, FramebufferAttachements.All);
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);

            // Should be processed in the 
            if (Framebuffer.Size != Editor.Viewport)
                Framebuffer.Resize((int)Editor.Viewport.X, (int)Editor.Viewport.Y);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(new Color4(0.1f, 0.1f, 0.1f, 1.0f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Framebuffer.Bind();
            GL.ClearColor(ClearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            shaderProgram.Bind();
            _vao.Draw();
            Framebuffer.Unbind();

            layerStack.Render(args.Time);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected new void OnUnload()
        {
            
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            Width = e.Width;
            Height = e.Height;
        }
    }
}
