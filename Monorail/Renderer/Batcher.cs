using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Monorail.Renderer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex2D
    {
        public Vector3 Position;
        // TODO: Change Color4 from floats to bytes
        public Color4 Color;
        public Vector2 UV;
        public byte TextureIndex; // Texture index 0 means no texture

        public Vertex2D(Vector3 position, Color4 color, Vector2? uv = null, byte texId = 0)
        {
            Position = position;
            Color = color;
            UV = uv.HasValue ? uv.Value : Vector2.Zero;
            TextureIndex = texId;
        }
    }

    public class Batcher
    {
        public const int DEFAULT_SIZE = 10000;

        // 4 so the default size can handle up to 10k quads
        List<Vertex2D> _vertexBatch = new List<Vertex2D>(DEFAULT_SIZE * 4);
        
        // A quad has 6 indices
        List<uint> _indexBatch = new List<uint>(DEFAULT_SIZE * 6);

        VertexArray _vao;

        static Batcher()
        {
            var layout = new VertexLayout()
                .AddAttrib(new VertexAttrib("a_Position", VertexAttribDataType.Float3))
                .AddAttrib(new VertexAttrib("a_Color", VertexAttribDataType.Float4))
                .AddAttrib(new VertexAttrib("a_UV", VertexAttribDataType.Float2))
                .AddAttrib(new VertexAttrib("a_TexId", VertexAttribDataType.Byte));

            VertexArray.AddVertexLayout("Renderer2D", layout);
        }

        public Batcher()
        {
            _vao = new VertexArray("Renderer2D", new VertexBuffer(), new IndexBuffer());
            _vao.VertexBuffer.SetElementSize(Unsafe.SizeOf<Vertex2D>());
            _vao.VertexBuffer.AllocateEmpty(_vertexBatch.Capacity, BufferUsageHint.DynamicDraw);

            _vao.IndexBuffer.SetElementSize(sizeof(uint));
            _vao.IndexBuffer.ElementsType = DrawElementsType.UnsignedInt;
            _vao.IndexBuffer.AllocateEmpty(_indexBatch.Capacity, BufferUsageHint.DynamicDraw);
        }

        public Batcher PushVertices(params Vertex2D[] vertex)
        {
            _vertexBatch.AddRange(vertex);
            return this;
        }

        public Batcher PushIndices(params uint[] indices)
        {
            _indexBatch.AddRange(indices);
            return this;
        }

        public void FlushData()
        {
            // Set vertex subdata
            if (_vertexBatch.Count > _vao.VertexBuffer.DataLength)
                _vao.VertexBuffer.AllocateEmpty(_vertexBatch.Count, BufferUsageHint.DynamicDraw);

            var verticesArray = _vertexBatch.ToArray();
            unsafe
            {
                fixed (Vertex2D* pArray = verticesArray)
                {
                    IntPtr intPtr = new IntPtr(pArray);
                    _vao.VertexBuffer.SetSubData(_vertexBatch.Count, intPtr);
                }
            }

            // Set index subdata
            if (_indexBatch.Count > _vao.IndexBuffer.DataLength)
                _vao.IndexBuffer.AllocateEmpty(_indexBatch.Count, BufferUsageHint.DynamicDraw);

            var indicesArray = _indexBatch.ToArray();
            unsafe
            {
                fixed (uint* pArray = indicesArray)
                {
                    IntPtr intPtr = new IntPtr(pArray);
                    _vao.IndexBuffer.SetSubData(_indexBatch.Count, intPtr);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _vertexBatch.Clear();
            _indexBatch.Clear();
        }

        public void Draw()
        {
            RenderCommand.DrawIndexed(_vao, BeginMode.Triangles, _indexBatch.Count);
        }
    }
}
