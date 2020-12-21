using Monorail.ECS;
using Monorail.Debug;
using System.Drawing;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Monorail.Renderer
{
    public static class Renderer2D
    {
        static readonly Vector2[] _vertexPositions =
        {
            new Vector2(-0.5f, -0.5f),
            new Vector2(-0.5f,  0.5f),
            new Vector2( 0.5f,  0.5f),
            new Vector2( 0.5f, -0.5f),
        };

        static readonly string VertexShader = @"
            #version 330 core
            layout (location = 0) in vec3 a_Position;
            layout (location = 1) in vec4 a_Color;
            layout (location = 2) in vec2 a_UV;
            layout (location = 3) in float a_TexId;

            out VS_OUT {
                float TexId;
                vec2     UV;
                vec4     Color;
            } vs_out;

            uniform mat4 u_Projection;

            void main()
            {
                vs_out.Color = a_Color;
                vs_out.UV = a_UV;
                vs_out.TexId = a_TexId;
                gl_Position = u_Projection * vec4(a_Position, 1.0);
            }";

        static readonly string FragmentShader = @"
            #version 330 core
            out vec4 FragColor;

            in VS_OUT {
                float TexId;
                vec2     UV;
                vec4     Color;
            } fs_in;

            uniform sampler2D u_Textures[32];

            void main()
            {
                if (fs_in.TexId > 0.0)
                {
                    FragColor = texture(u_Textures[int(fs_in.TexId) - 1], fs_in.UV) * fs_in.Color;
                }
                else FragColor = fs_in.Color;
            }";


        static ShaderProgram _shader;

        static Batcher _trianglesBatcher;

        static uint _indexOffset = 0;

        static byte _textureOffset = 0;

        static bool _begun = false;

        static Texture2D[] _textures = new Texture2D[32];

        static Renderer2D()
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

            int[] samplers = new int[32];
            for (int i = 0; i < 32; i++)
                samplers[i] = i;
            _shader.SetUniform1("u_Textures", samplers);

            _trianglesBatcher = new Batcher();
        }

        public static void Begin(Camera2D camera)
        {
            Insist.AssertFalse(_begun, "Renderer has already begun.");
            _trianglesBatcher.Clear();
            _shader.SetUniformMat4("u_Projection", false, camera.ProjectionView);
            _begun = true;
        }

        /// <summary>
        /// Base function for batching a new Quad
        /// </summary>
        /// <param name="transform">The quad's transform</param>
        /// <param name="color">Each vertex color, must be exactly 4</param>
        /// <param name="texture">The texture to use</param>
        /// <param name="source">The texture source in UV space (Vertical inverted)</param>
        public static void DrawQuad(Transform2D transform, Color4[] color, Texture2D texture = null, RectangleF? source = null)
        {
            Insist.AssertEq(color.Length, 4, "4 vertex color required");
            // TODO: Add per triangle frustrum culling

            float cos = (float)MathHelper.Cos(transform.Rotation);
            float sin = (float)MathHelper.Sin(transform.Rotation);
            var rotMat = new Matrix2(cos, -sin, sin, cos);

            var uvPositions =  new Vector2[4];
            if (texture != null)
            {
                // Bottom Left UV
                uvPositions[0].X = source.HasValue ? source.Value.X : 0;
                uvPositions[0].Y = source.HasValue ? 1f - source.Value.Height : 0;

                // Top Left UV
                uvPositions[1].X = source.HasValue ? source.Value.X : 0;
                uvPositions[1].Y = source.HasValue ? 1f - source.Value.Y : 1;

                // Top Right UV
                uvPositions[2].X = source.HasValue ? source.Value.Width : 1;
                uvPositions[2].Y = source.HasValue ? 1f - source.Value.Y : 1;

                // Bottom Right UV
                uvPositions[3].X = source.HasValue ? source.Value.Width : 1;
                uvPositions[3].Y = source.HasValue ? 1f - source.Value.Height : 0;
            }

            // Queue texture
            byte texPos = 0;
            if (texture != null)
            {
                // Search if texture is already queue
                sbyte internalPos = -1;
                for (sbyte i = 0; i < _textureOffset; i++)
                {
                    if (_textures[i] == texture)
                    {
                        internalPos = i;
                        break;
                    }
                }

                // If it isn't queued already do so
                if (internalPos < 0)
                {
                    if (_textureOffset >= 32)
                        Flush();

                    _textures[_textureOffset] = texture;
                    internalPos = (sbyte)_textureOffset++;
                }

                texture.Bind(TextureUnit.Texture0 + internalPos);
                texPos = (byte)(internalPos + 1);
            }

            // Push the vertices to the batcher
            for (int i = 0; i < 4; i++)
            {
                var rotation = rotMat * new Vector2(
                    _vertexPositions[i].X * transform.Scale.X,
                    _vertexPositions[i].Y * transform.Scale.Y
                );

                _trianglesBatcher.PushVertices(new Vertex2D(
                    new Vector3(rotation.X + transform.Position.X, rotation.Y + transform.Position.Y, 0),
                    color[i], uvPositions[i], texPos
                ));
            }

            // Add the 2 triangles indices
            _trianglesBatcher.PushIndices(
                _indexOffset, _indexOffset + 1, _indexOffset + 2, // Bottom left triangle
                _indexOffset + 2, _indexOffset + 3, _indexOffset  // Top Right triangle
            );

            _indexOffset += 4;
        }

        public static void End()
        {
            Insist.Assert(_begun, "Renderer hasn't begun.");
            Flush();
            _begun = false;
        }

        static void Flush()
        {
            _shader.Bind();
            _trianglesBatcher.FlushData();
            _trianglesBatcher.Draw();
            _trianglesBatcher.Clear();
            _indexOffset = 0;

            // Remove used textures
            for (int i = 0; i < _textureOffset; i++)
            {
                _textures[i]?.Unbind();
                _textures[i] = null;
            }

            _textureOffset = 0;
        }
    }
}
