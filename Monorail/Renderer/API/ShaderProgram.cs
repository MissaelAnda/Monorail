using System;
using Monorail.Debug;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Monorail.Renderer
{
    public class ShaderProgram : OpenGLResource
    {
        public static int CurrentlyBinded { get; protected set; } = 0;

        Shader[] _shaders = new Shader[Enum.GetNames(typeof(ShaderType)).Length - 2];

        Dictionary<string, int> _uniformsLocations = new Dictionary<string, int>();

        static int GetShaderPosition(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.FragmentShader:
                    return 0;
                case ShaderType.VertexShader:
                    return 1;
                case ShaderType.GeometryShader:
                    return 2;
                case ShaderType.TessEvaluationShader:
                    return 3;
                case ShaderType.TessControlShader:
                    return 4;
                case ShaderType.ComputeShader:
                    return 5;
                default:
                    return 0;
            }
        }

        public ShaderProgram()
        {
            _id = GL.CreateProgram();
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.ShaderProgram);
        }

        public void LinkProgram()
        {
            GL.LinkProgram(_id);
            GL.GetProgram(_id, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                GL.GetProgramInfoLog(_id, out var log);
                Insist.Fail(log);
            }
        }

        public Shader GetShader(ShaderType type) => _shaders[GetShaderPosition(type)];

        public void AttachShader(Shader shader)
        {
            Insist.Assert(shader.Compiled, "Shaders must be compiled before attaching them to a program.");

            _shaders[GetShaderPosition(shader.Type)] = shader;
            shader.ProgramsAttached.Add(this);

            GL.AttachShader(_id, shader.ID);
        }

        public void DetachShader(ShaderType type)
        {
            var pos = GetShaderPosition(type);
            var shader = _shaders[pos];
            if (shader != null)
            {
                shader.ProgramsAttached.Remove(this);
                GL.DetachShader(_id, shader.ID);
                _shaders[pos] = null;
            }
        }

        public void DetachAllShaders()
        {
            for(int i = 0; i < _shaders.Length; i++)
            {
                if (_shaders[i] != null)
                {
                    _shaders[i].ProgramsAttached.Remove(this);
                    GL.DetachShader(_id, _shaders[i].ID);
                    _shaders[i] = null;
                }
            }
        }

        public void Bind()
        {
            if (CurrentlyBinded != _id)
            {
                CurrentlyBinded = _id;
                GL.UseProgram(_id);
            }
        }

        public void Unbind()
        {
            if (CurrentlyBinded == _id)
            {
                CurrentlyBinded = 0;
                GL.UseProgram(0);
            }
        }

        public static void UnbindAny()
        {
            CurrentlyBinded = 0;
            GL.UseProgram(0);
        }

        #region Uniforms setters

        int GetLocation(string name)
        {
            if (_uniformsLocations.TryGetValue(name, out int loc))
                return loc;

            loc = GL.GetUniformLocation(_id, name);
            if (loc < 0)
                Log.Core.Warn("Location in shader '{0}' for uniform '{1}' not found.", _id, name);

            _uniformsLocations.Add(name, loc);

            return loc;
        }

        public void SetUniform1(string name, int val) => GL.ProgramUniform1(_id, GetLocation(name), val);
        public void SetUniform1(string name, uint val) => GL.ProgramUniform1(_id, GetLocation(name), val);
        public void SetUniform1(string name, float val) => GL.ProgramUniform1(_id, GetLocation(name), val);
        public void SetUniform1(string name, double val) => GL.ProgramUniform1(_id, GetLocation(name), val);

        public void SetUniform1(string name, int[] val) => GL.ProgramUniform1(_id, GetLocation(name), val.Length, val);
        public void SetUniform1(string name, uint[] val) => GL.ProgramUniform1((uint)_id, GetLocation(name), val.Length, val);
        public void SetUniform1(string name, float[] val) => GL.ProgramUniform1(_id, GetLocation(name), val.Length, val);
        public void SetUniform1(string name, double[] val) => GL.ProgramUniform1(_id, GetLocation(name), val.Length, val);

        public void SetUniform2(string name, ref Vector2 val) => GL.ProgramUniform2(_id, GetLocation(name), ref val);
        public void SetUniform2(string name, ref Vector2i val) => GL.ProgramUniform2(_id, GetLocation(name), val);
        public void SetUniform2(string name, ref Vector2d val) => GL.ProgramUniform2(_id, GetLocation(name), val.X, val.Y);
        public void SetUniform2(string name, Vector2 val) => GL.ProgramUniform2(_id, GetLocation(name), ref val);
        public void SetUniform2(string name, Vector2i val) => GL.ProgramUniform2(_id, GetLocation(name), val);
        public void SetUniform2(string name, Vector2d val) => GL.ProgramUniform2(_id, GetLocation(name), val.X, val.Y);
        public void SetUniform2(string name, int x, int y) => GL.ProgramUniform2(_id, GetLocation(name), x, y);
        public void SetUniform2(string name, uint x, uint y) => GL.ProgramUniform2(_id, GetLocation(name), x, y);
        public void SetUniform2(string name, float x, float y) => GL.ProgramUniform2(_id, GetLocation(name), x, y);
        public void SetUniform2(string name, double x, double y) => GL.ProgramUniform2(_id, GetLocation(name), x, y);

        public void SetUniform3(string name, ref Vector3 val) => GL.ProgramUniform3(_id, GetLocation(name), ref val);
        public void SetUniform3(string name, ref Vector3i val) => GL.ProgramUniform3(_id, GetLocation(name), val);
        public void SetUniform3(string name, ref Vector3d val) => GL.ProgramUniform3(_id, GetLocation(name), val.X, val.Y, val.Z);
        public void SetUniform3(string name, Vector3 val) => GL.ProgramUniform3(_id, GetLocation(name), ref val);
        public void SetUniform3(string name, Vector3i val) => GL.ProgramUniform3(_id, GetLocation(name), val);
        public void SetUniform3(string name, Vector3d val) => GL.ProgramUniform3(_id, GetLocation(name), val.X, val.Y, val.Z);
        public void SetUniform3(string name, int x, int y, int z) => GL.ProgramUniform3(_id, GetLocation(name), x, y, z);
        public void SetUniform3(string name, uint x, uint y, uint z) => GL.ProgramUniform3(_id, GetLocation(name), x, y, z);
        public void SetUniform3(string name, float x, float y, float z) => GL.ProgramUniform3(_id, GetLocation(name), x, y, z);
        public void SetUniform3(string name, double x, double y, double z) => GL.ProgramUniform3(_id, GetLocation(name), x, y, z);

        public void SetUniform4(string name, ref Vector4 val) => GL.ProgramUniform4(_id, GetLocation(name), val);
        public void SetUniform4(string name, ref Vector4i val) => GL.ProgramUniform4(_id, GetLocation(name), val);
        public void SetUniform4(string name, ref Vector4d val) => GL.ProgramUniform4(_id, GetLocation(name), val.X, val.Y, val.Z, val.W);
        public void SetUniform4(string name, Vector4 val) => GL.ProgramUniform4(_id, GetLocation(name), val);
        public void SetUniform4(string name, Vector4i val) => GL.ProgramUniform4(_id, GetLocation(name), val);
        public void SetUniform4(string name, Vector4d val) => GL.ProgramUniform4(_id, GetLocation(name), val.X, val.Y, val.Z, val.W);
        public void SetUniform4(string name, int x, int y, int z, int w) => GL.ProgramUniform4(_id, GetLocation(name), x, y, z, w);
        public void SetUniform4(string name, uint x, uint y, uint z, uint w) => GL.ProgramUniform4(_id, GetLocation(name), x, y, z, w);
        public void SetUniform4(string name, float x, float y, float z, float w) => GL.ProgramUniform4(_id, GetLocation(name), x, y, z, w);
        public void SetUniform4(string name, double x, double y, double z, double w) => GL.ProgramUniform4(_id, GetLocation(name), x, y, z, w);

        public void SetUniformMat3(string name, bool transpose, ref Matrix3 val) => GL.ProgramUniformMatrix3(_id, GetLocation(name), transpose, ref val);
        public void SetUniformMat3(string name, bool transpose, Matrix3 val) => GL.ProgramUniformMatrix3(_id, GetLocation(name), transpose, ref val);
        public void SetUniformMat4(string name, bool transpose, ref Matrix4 val) => GL.ProgramUniformMatrix4(_id, GetLocation(name), transpose, ref val);
        public void SetUniformMat4(string name, bool transpose, Matrix4 val) => GL.ProgramUniformMatrix4(_id, GetLocation(name), transpose, ref val);

        #endregion

        public override void Dispose()
        {
            if (!_disposed)
            {
                Unbind();
                DetachAllShaders();
                GL.DeleteProgram(_id);
                base.Dispose();
            }
        }
    }
}
