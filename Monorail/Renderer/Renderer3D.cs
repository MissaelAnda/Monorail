using Monorail.Debug;
using Monorail.ECS;
using OpenTK.Graphics.OpenGL4;

namespace Monorail.Renderer
{
    public class Renderer3D
    {
        static readonly string VertexShader = @"
            #version 330 core
            layout (location = 0) in vec3 a_Position;
            layout (location = 1) in vec3 a_Normal;
            layout (location = 2) in vec2 a_UV;

            uniform mat4 u_ProjView;
            uniform mat4 u_Model;
  
            out vec2 o_TexCoord;
            out vec3 o_Normal;

            void main()
            {
                gl_Position = u_ProjView * u_Model * vec4(a_Position, 1.0f);
                o_TexCoord = a_UV;
                o_Normal = a_Normal;
            }
        ";

        static readonly string FragmentShader = @"
            #version 330 core
            out vec4 FragColor;

            in vec2 o_TexCoord;
            in vec3 o_Normal;

            uniform sampler2D texture1;
            //uniform sampler2D texture2;

            void main()
            {
                FragColor = texture(texture1, o_TexCoord);
                //FragColor = vec4(o_TexCoord, 0.5, 1.0);
                //FragColor = vec4(o_Normal, 1.0);
            }
        ";

        ShaderProgram _shader;

        bool _started = false;

        static Renderer3D _instance;

        static Renderer3D Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Renderer3D();

                return _instance;
            }
        }

        private Renderer3D()
        {
            var vertex = Shader.FromSource(VertexShader, ShaderType.VertexShader);
            var fragment = Shader.FromSource(FragmentShader, ShaderType.FragmentShader);

            _shader = new ShaderProgram();

            _shader.AttachShader(vertex);
            _shader.AttachShader(fragment);

            _shader.LinkProgram();

            _shader.DetachAllShaders();

            vertex.Dispose();
            fragment.Dispose();

            var layout = new VertexLayout()
                .AddAttrib(new VertexAttrib("a_Position", VertexAttribDataType.Float3))
                .AddAttrib(new VertexAttrib("a_Normal", VertexAttribDataType.Float3))
                .AddAttrib(new VertexAttrib("a_UV", VertexAttribDataType.Float2));

            VertexArray.AddVertexLayout("Basic3D", layout);

            // TODO: 3D Batcher
        }

        public static void Initialize()
        {
            _instance = new Renderer3D();
        }

        public static void Begin(Camera3D camera)
        {
            Insist.AssertFalse(Instance._started, "Renderer has already begun.");
            RenderCommand.ResetDrawCalls();
            RenderCommand.EnableCaps(EnableCap.CullFace);
            Instance._shader.SetUniformMat4("u_ProjView", false, camera.ProjectionView);
            Instance._started = true;
        }

        public static void DrawMesh(MeshRenderer meshRenderer, Transform transform)
        {
            Insist.Assert(Instance._started, "Renderer hasn't begun.");
            Instance._shader.Bind();
            Instance._shader.SetUniformMat4("u_Model", false, transform.WorldMatrix);
            meshRenderer.Draw();
        }

        public static void End()
        {
            Insist.Assert(Instance._started, "Renderer hasn't begun.");
            Instance._started = false;
        }
    }
}
