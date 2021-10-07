using Monorail.ECS;
using Monorail.Debug;
using System.Drawing;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Monorail.Util;
using System.Runtime.CompilerServices;
using Monorail.Math;
using System.Collections.Generic;

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
                vec2  UV;
                vec4  Color;
            } vs_out;

            uniform mat4 u_Projection;

            void main()
            {
                vs_out.Color = a_Color;
                vs_out.UV = a_UV;
                vs_out.TexId = a_TexId;
                gl_Position = u_Projection * vec4(a_Position, 1.0);
            }";

        // TODO: Change TexId from float to flat byte
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

        static Camera2D _camera;

        static Dictionary<int, List<Vector2>> _elipsesCache = new Dictionary<int, List<Vector2>>(100);

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
            RenderCommand.ResetDrawCalls();
            _trianglesBatcher.Clear();
            _camera = camera;
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
        public static void DrawQuad(Transform2D transform, Color4 color, float depth, Texture2D texture = null, RectangleF? source = null)
        {
            float cos = (float)MathHelper.Cos(transform.Rotation);
            float sin = (float)MathHelper.Sin(transform.Rotation);
            var rotMat = new Matrix2(cos, -sin, sin, cos);

            var positions = new Vector2[4];
            // Push the vertices to the batcher

            for (int i = 0; i < 4; i++)
            {
                positions[i] = (rotMat * new Vector2(
                    _vertexPositions[i].X * transform.Scale.X,
                    _vertexPositions[i].Y * transform.Scale.Y
                )) + transform.Position;
            }

            float minX = MathExtra.Min(positions[0].X, positions[1].X, positions[2].X, positions[3].X);
            float minY = MathExtra.Min(positions[0].Y, positions[1].Y, positions[2].Y, positions[3].Y);
            float maxX = MathExtra.Max(positions[0].X, positions[1].X, positions[2].X, positions[3].X);
            float maxY = MathExtra.Max(positions[0].Y, positions[1].Y, positions[2].Y, positions[3].Y);

            // Check if quad is inside the camera bounds
            if (!FrustumCullTest(new RectangleF(minX, minY, maxX - minX, maxY - minY)))
                return;

            // Set the UV positions from texture source
            var uvPositions = new Vector2[4];
            if (texture != null)
            {
                float left = source?.X ?? 0;
                float bottom = source?.Y ?? 0;
                float right = source.HasValue ? source.Value.X + source.Value.Width : 1;
                float top = source.HasValue ? source.Value.Y + source.Value.Height : 1;

                // Bottom Left UV
                uvPositions[0].X = left;
                uvPositions[0].Y = bottom;

                // Top Left UV
                uvPositions[1].X = left;
                uvPositions[1].Y = top;

                // Top Right UV
                uvPositions[2].X = right;
                uvPositions[2].Y = top;

                // Bottom Right UV
                uvPositions[3].X = right;
                uvPositions[3].Y = bottom;
            }

            // Queue texture
            byte texPos = BatchTexture(texture);

            // Create the vertecies
            var vertices = new Vertex2D[4];
            for (int i = 0; i < 4; i++)
                vertices[i] = new Vertex2D(positions[i].ToVector3(depth), color, uvPositions[i], texPos);

            // Batch them
            _trianglesBatcher.PushVertices(vertices);

            // Add the 2 triangles indices
            _trianglesBatcher.PushIndices(
                _indexOffset, _indexOffset + 1, _indexOffset + 2, // Bottom left triangle
                _indexOffset + 2, _indexOffset + 3, _indexOffset  // Top Right triangle
            );
            // Two triangles moves the offset by 4
            _indexOffset += 4;
        }

        // TODO: Add transform to triangle
        public static void DrawTriangle(Vector2[] positions, Color4[] colors, Texture2D texture = null, Vector2[] texCoordinates = null)
        {
            Insist.AssertEq(positions.Length, 3);
            Insist.AssertEq(colors.Length, 3);
            if (texCoordinates != null)
                Insist.AssertEq(texCoordinates.Length, 3);

            // Frustum culling test
            float minX = MathExtra.Min(positions[0].X, positions[1].X, positions[2].X);
            float minY = MathExtra.Min(positions[0].Y, positions[1].Y, positions[2].Y);
            float maxX = MathExtra.Max(positions[0].X, positions[1].X, positions[2].X);
            float maxY = MathExtra.Max(positions[0].Y, positions[1].Y, positions[2].Y);

            float distX = maxX - minX;
            float distY = maxY - minY;

            if (!FrustumCullTest(new RectangleF(minX, minY, distX, distY)))
                return;

            // Create texture coordinates
            if (texCoordinates == null)
            {
                texCoordinates = new Vector2[3];

                texCoordinates[0].X = (positions[0].X - minX) / distX;
                texCoordinates[0].Y = (positions[0].Y - minY) / distY;

                texCoordinates[1].X = (positions[1].X - minX) / distX;
                texCoordinates[1].Y = (positions[1].Y - minY) / distY;

                texCoordinates[2].X = (positions[2].X - minX) / distX;
                texCoordinates[2].Y = (positions[2].Y - minY) / distY;
            }

            var texPos = BatchTexture(texture);

            Vertex2D[] vertices = new Vertex2D[3];
            for (int i = 0; i < 3; i++)
            {
                vertices[i].Position = positions[i].ToVector3();
                vertices[i].Color = colors[i];
                vertices[i].UV = texCoordinates[i];
                vertices[i].TextureIndex = texPos;
            }

            _trianglesBatcher.PushVertices(vertices);

            _trianglesBatcher.PushIndices(_indexOffset, _indexOffset + 1, _indexOffset + 2);
            _indexOffset += 3;
        }

        public static void DrawElipse(Transform2D trans, Color4 color, float radius = 1.0f, Texture2D texture = null, int? segments = null)
        {
            Insist.Assert(radius > 0);
            if (segments.HasValue)
                Insist.Assert(segments.Value >= 2);

            float sideWidth = radius * trans.Scale.X;
            float sideHeight = radius * trans.Scale.Y;

            var boundingBox = new RectangleF(
                trans.Position.X - sideWidth, trans.Position.Y - sideHeight,
                sideWidth * 2, sideHeight * 2);

            // If elipse's bounding box isn't inside the camera frustum
            // there's no need to batch any vertices
            if (!FrustumCullTest(boundingBox))
                return;

            var _segments = segments.HasValue ? segments.Value :
                MathHelper.Max(2, (int)(6 * (float)System.Math.Cbrt(radius * MathHelper.Max(trans.Scale.X, trans.Scale.Y))));
            var positions = CreateElipsePositions(_segments);

            var startPoint = new Vector2(trans.Position.X + radius * trans.Scale.X, trans.Position.Y);

            var texPos = BatchTexture(texture);
            // Create the UV coordinates if there is a texture to bind
            Vector2[] texCoordinates = new Vector2[positions.Length + 1];
            if (texture != null)
            {
                texCoordinates[0].X = 1.0f;
                texCoordinates[0].Y = 0.5f;

                for (int i = 0; i < positions.Length; i++)
                {
                    texCoordinates[i + 1].X = (positions[i].X / 2f) + 0.5f;
                    texCoordinates[i + 1].Y = (positions[i].Y / 2f) + 0.5f;
                }
            }

            _trianglesBatcher.PushVertices(new Vertex2D(startPoint.ToVector3(), color, texCoordinates[0], texPos));
            _trianglesBatcher.PushVertices(new Vertex2D(new Vector3(
                trans.Position.X + positions[0].X * radius * trans.Scale.X,
                trans.Position.Y + positions[0].Y * radius * trans.Scale.Y, 0),
                color, texCoordinates[1], texPos));

            for (uint i = 1; i < positions.Length; i++)
            {
                _trianglesBatcher.PushVertices(new Vertex2D(new Vector3(
                    trans.Position.X + positions[i].X * radius * trans.Scale.X,
                    trans.Position.Y + positions[i].Y * radius * trans.Scale.Y, 0),
                    color, texCoordinates[i + 1], texPos));
                _trianglesBatcher.PushIndices(_indexOffset);
                _trianglesBatcher.PushIndices(_indexOffset + i);
                _trianglesBatcher.PushIndices(_indexOffset + i + 1);
            }
            _indexOffset += (uint)positions.Length + 1;
        }

        static Vector2[] CreateElipsePositions(int segments)
        {
            // Check if points list with this amount of segments already exists
            if (_elipsesCache.TryGetValue(segments, out var list))
                return list.ToArray();

            float leap = MathHelper.TwoPi / segments;

            list = new List<Vector2>(segments);

            int i = 0;
            for (float pos = leap; pos < MathHelper.TwoPi; pos += leap)
            {
                list.Add(new Vector2((float)MathHelper.Cos(pos), (float)MathHelper.Sin(pos)));
                i++;
            }

            // Save the list if needed later
            _elipsesCache.Add(segments, list);

            return list.ToArray();
        }

        static byte BatchTexture(Texture2D texture)
        {
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
                    texture.Bind(TextureUnit.Texture0 + internalPos);
                }

                texPos = (byte)(internalPos + 1);
            }
            return texPos;
        }

        public static void End()
        {
            Insist.Assert(_begun, "Renderer hasn't begun.");
            Flush();
            _begun = false;
            _camera = null;
        }

        /// <summary>
        /// Checks wether the point is inside the camera bounds
        /// </summary>
        /// <param name="position">the point to cull check</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool FrustumCullTest(in Vector2 position)
        {
            RectangleF bounds = _camera != null ? _camera.Bounds : RenderCommand.Viewport;

            return bounds.IsInside(position);
        }

        /// <summary>
        /// Checks wether the point is inside the camera bounds
        /// </summary>
        /// <param name="position">the point to cull check</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool FrustumCullTest(in RectangleF box)
        {
            RectangleF bounds = _camera != null ? _camera.Bounds : RenderCommand.Viewport;

            return bounds.IntersectsWith(box);
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
