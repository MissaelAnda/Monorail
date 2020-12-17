using Monorail.Debug;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Monorail.Renderer
{
    public class Shader : OpenGLResource
    {
        public readonly ShaderType Type;

        public List<ShaderProgram> ProgramsAttached = new List<ShaderProgram>();

        public bool Compiled { get; protected set; } = false;

        public static Shader FromFile(string path, ShaderType type)
        {
            var shader = new Shader(type);

            shader.SetSourceFromFile(path);

            return shader;
        }

        public static Shader FromSource(string source, ShaderType type)
        {
            var shader = new Shader(type);

            shader.SetSource(source);

            return shader;
        }

        public Shader(ShaderType type)
        {
            Type = type;

            _id = GL.CreateShader(Type);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Shader);
        }

        public void SetSourceFromFile(string path)
        {
            string source;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                source = reader.ReadToEnd();
            }

            SetSource(source);
        }

        public void SetSource(string source)
        {
            GL.ShaderSource(_id, source);
            GL.CompileShader(_id);
            var infoLogVert = GL.GetShaderInfoLog(_id);
            if (infoLogVert != string.Empty)
                Insist.Fail("Failed to compile {0} Shader: {1}", Type, infoLogVert);

            Compiled = true;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                foreach (var program in ProgramsAttached)
                    program.DetachShader(Type);

                GL.DeleteShader(_id);
                base.Dispose();
            }
        }
    }
}
