using Monorail.Renderer;
using OpenTK.Graphics.OpenGL4;

namespace Monorail.ECS
{
    public class Mesh
    {
        private float[] _vertices;

        private uint[] _indices;

        public float[] Vertices => _vertices;

        public uint[] Indices => _indices;

        public Mesh(float[] vertices, uint[] indices = null)
        {
            _vertices = vertices;
            _indices = indices;
        }
    }

    public class CubeMesh : Mesh
    {
        static readonly float[] CubeVertices = {
            // Back
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f,  // 0
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,  // 1
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f,  // 2
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,  // 3
            // Front
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,  // 4
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f,  // 5
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,  // 6
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f,  // 7
            // Left
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f,  // 8
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,  // 9
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f, 0.0f, 0.0f,  // 10
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,  // 11
            // Right
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,  // 12
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f, 1.0f, 1.0f,  // 13
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,  // 14
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f,  // 15
            // Bottom
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, 0.0f, 0.0f,  // 16
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,  // 17
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f,  // 18
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,  // 19
            // Top
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f,  // 20
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,  // 21
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, 0.0f, 0.0f,  // 22
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f,  // 23
        };

        static readonly uint[] CubeIndices =
        {
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20,
        };

        public CubeMesh() : base(CubeVertices, CubeIndices)
        { }
    }

    public class MeshRenderer
    {
        public Texture2D Texture { get; set; }

        Mesh _mesh;
        VertexBuffer _vbo;
        IndexBuffer _ibo;

        public MeshRenderer(Mesh mesh)
        {
            _mesh = mesh;
            _vbo = new VertexBuffer();
            _vbo.SetElementSize(sizeof(float) * 8);
            _vbo.SetData(_mesh.Vertices, BufferUsageHint.StaticDraw);
            VertexArray.BindVertexBuffer("Basic3D", _vbo);

            if (_mesh.Indices != null)
            {
                _ibo = new IndexBuffer
                {
                    ElementsType = DrawElementsType.UnsignedInt
                };
                _ibo.SetElementSize(sizeof(uint));
                _ibo.SetData(_mesh.Indices, BufferUsageHint.StaticDraw);
            }
        }

        public void Draw()
        {
            Texture?.Bind(TextureUnit.Texture0);

            if (_ibo != null)
                RenderCommand.DrawIndexed(_vbo, _ibo, BeginMode.Triangles, _mesh.Indices.Length);
            else
                RenderCommand.DrawArrays(_vbo, PrimitiveType.Triangles, _mesh.Vertices.Length);

            Texture?.Unbind();
        }
    }
}
