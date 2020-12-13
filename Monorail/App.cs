using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Monorail.Debug;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Monorail.ImGUI;
using System.Runtime.InteropServices;
using System;

namespace Monorail
{
    public class App : GameWindow
    {
        LayerStack layerStack;
        Layer imguiLayer;
        Color4 ClearColor = Color4.CornflowerBlue;

        int vertexArray, vertexBuffer, indexBuffer, shader;

        float[] VertexData =
        {
            -1.0f, -1.0f, 1.0f, 0.0f, 0.0f,
             0.0f,  1.0f, 0.0f, 1.0f, 0.0f,
             1.0f, -1.0f, 0.0f, 0.0f, 1.0f,
        };

        uint[] IndexData =
        {
            0, 1, 2
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
                FragColor = vec4(Color, 1.0);
            }";

        public App() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Title = "Monorail";
            Size = new OpenTK.Mathematics.Vector2i(1280, 720);
            layerStack = new LayerStack(this);

            UpdateFrame += OnUpdate;
        }

        public void GLDebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
#nullable enable
            string? msg = Marshal.PtrToStringAnsi(message, length);
#nullable disable

            if (msg == null)
                msg = "Failed to decode error message.";

            string log = string.Format("OpenGL Message: [Source: - {0}] [Type: - {1}] [ID: - {2}]\n\n[Message: - {3}]", source, type, id, msg);

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

            //GL.Enable(EnableCap.DebugOutput);
            //GL.DebugMessageCallback(GLDebugCallback, 0);

            imguiLayer = new ImGuiLayer(this);
            layerStack.PushLayer(imguiLayer);


            vertexArray = GL.GenVertexArray();

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexData.Length * sizeof(float), VertexData, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(vertexArray);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 2);

            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexData.Length * sizeof(uint), IndexData, BufferUsageHint.StaticDraw);

            var vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, VertexShader);
            GL.CompileShader(vertex);
            var infoLogVert = GL.GetShaderInfoLog(vertex);
            if (infoLogVert != string.Empty)
                Log.Core.Error($"Failed to compile Fragment Shader: {infoLogVert}");

            var fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, FragmentShader);
            GL.CompileShader(fragment);
            infoLogVert = GL.GetShaderInfoLog(fragment);
            if (infoLogVert != string.Empty)
                Log.Core.Error($"Failed to compile Fragment Shader: {infoLogVert}");

            shader = GL.CreateProgram();
            GL.AttachShader(shader, vertex);
            GL.AttachShader(shader, fragment);

            GL.LinkProgram(shader);

            GL.DetachShader(shader, vertex);
            GL.DetachShader(shader, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            GL.UseProgram(shader);


            GL.ClearColor(ClearColor);
        }

        protected void OnUpdate(FrameEventArgs args)
        {
            layerStack.Update(args.Time);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            layerStack.Render(args.Time);

            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.DrawElements(BeginMode.Triangles, 3, DrawElementsType.UnsignedInt, 0);

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
        }
    }
}
