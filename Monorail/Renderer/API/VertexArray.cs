using Monorail.Debug;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Monorail.Renderer
{
    public enum VertexAttribDataType
    {
        // Variable type ----- Shader variable type
        Bool,               // bool
        Bool2,              // bvec2
        Bool3,              // bvec3
        Bool4,              // bvec4

        Byte,               // int
        UnsignedByte,       // uint
        Byte2,              // vec2 - ivec2
        UnsignedByte2,      // uvec2
        Byte3,              // vec3 - ivec3
        UnsignedByte3,      // uvec3
        Byte4,              // vec4 - ivec4
        UnsignedByte4,      // uvec4

        Short,              // int
        UnsignedShort,      // uint

        Int,                // int
        UnsignedInt,        // uint
        Int2,               // ivec2
        UnsignedInt2,       // uvec2
        Int3,               // ivec3
        UnsignedInt3,       // uvec3
        Int4,               // ivec4
        UnsignedInt4,       // uvec4

        Float,              // float
        Float2,             // vec2
        Float3,             // vec3
        Float4,             // vec4 - mat2

        Double,             // double
        Double2,            // dvec2
        Double3,            // dvec3
        Double4,            // dvec4 - dmat2

        Mat3,               // mat3
        Mat4,               // mat4

        DMat3,              // dmat3
        DMat4,              // dmat4
    }


    public struct VertexAttrib
    {
        public string Name;
        public VertexAttribDataType Type;
        public bool Normalized;

        public VertexAttrib(string name, VertexAttribDataType type, bool normalized = false)
        {
            Name = name;
            Type = type;
            Normalized = normalized;
        }

        public int DataSize => GetDataSize(Type);

        public int ElementsCount => GetElementsCount(Type);

        public int Stride => GetStride(Type);

        public VertexAttribType AttribType => GetAttribType(Type);


        public static int GetStride(VertexAttribDataType type)
        {
            return GetDataSize(type) * GetElementsCount(type);
        }

        public static int GetDataSize(VertexAttribDataType type)
        {
            switch(type)
            {
                case VertexAttribDataType.Bool:
                case VertexAttribDataType.Bool2:
                case VertexAttribDataType.Bool3:
                case VertexAttribDataType.Bool4:
                    return sizeof(bool);
                case VertexAttribDataType.Byte:
                case VertexAttribDataType.UnsignedByte:
                case VertexAttribDataType.Byte2:
                case VertexAttribDataType.UnsignedByte2:
                case VertexAttribDataType.Byte3:
                case VertexAttribDataType.UnsignedByte3:
                case VertexAttribDataType.Byte4:
                case VertexAttribDataType.UnsignedByte4:
                    return sizeof(byte);
                case VertexAttribDataType.Short:
                    return sizeof(short);
                case VertexAttribDataType.UnsignedShort:
                    return sizeof(ushort);
                case VertexAttribDataType.Int:
                case VertexAttribDataType.Int2:
                case VertexAttribDataType.Int3:
                case VertexAttribDataType.Int4:
                    return sizeof(int);
                case VertexAttribDataType.UnsignedInt:
                case VertexAttribDataType.UnsignedInt2:
                case VertexAttribDataType.UnsignedInt3:
                case VertexAttribDataType.UnsignedInt4:
                    return sizeof(uint);
                case VertexAttribDataType.Float:
                case VertexAttribDataType.Float2:
                case VertexAttribDataType.Float3:
                case VertexAttribDataType.Float4:
                case VertexAttribDataType.Mat3:
                case VertexAttribDataType.Mat4:
                    return sizeof(float);
                case VertexAttribDataType.Double:
                case VertexAttribDataType.Double2:
                case VertexAttribDataType.Double3:
                case VertexAttribDataType.Double4:
                case VertexAttribDataType.DMat3:
                case VertexAttribDataType.DMat4:
                    return sizeof(double);
                default:
                    return 0;
            }
        }

        public static VertexAttribType GetAttribType(VertexAttribDataType type)
        {
            switch (type)
            {
                case VertexAttribDataType.Bool:
                case VertexAttribDataType.Bool2:
                case VertexAttribDataType.Bool3:
                case VertexAttribDataType.Bool4:
                case VertexAttribDataType.Byte:
                case VertexAttribDataType.Byte2:
                case VertexAttribDataType.Byte3:
                case VertexAttribDataType.Byte4:
                    return VertexAttribType.Byte;
                case VertexAttribDataType.UnsignedByte:
                case VertexAttribDataType.UnsignedByte2:
                case VertexAttribDataType.UnsignedByte3:
                case VertexAttribDataType.UnsignedByte4:
                    return VertexAttribType.UnsignedByte;
                case VertexAttribDataType.Short:
                    return VertexAttribType.Short;
                case VertexAttribDataType.UnsignedShort:
                    return VertexAttribType.UnsignedShort;
                case VertexAttribDataType.Int:
                case VertexAttribDataType.Int2:
                case VertexAttribDataType.Int3:
                case VertexAttribDataType.Int4:
                    return VertexAttribType.Int;
                case VertexAttribDataType.UnsignedInt:
                case VertexAttribDataType.UnsignedInt2:
                case VertexAttribDataType.UnsignedInt3:
                case VertexAttribDataType.UnsignedInt4:
                    return VertexAttribType.UnsignedInt;
                case VertexAttribDataType.Float:
                case VertexAttribDataType.Float2:
                case VertexAttribDataType.Float3:
                case VertexAttribDataType.Float4:
                case VertexAttribDataType.Mat3:
                case VertexAttribDataType.Mat4:
                    return VertexAttribType.Float;
                case VertexAttribDataType.Double:
                case VertexAttribDataType.Double2:
                case VertexAttribDataType.Double3:
                case VertexAttribDataType.Double4:
                case VertexAttribDataType.DMat3:
                case VertexAttribDataType.DMat4:
                    return VertexAttribType.Double;
                default:
                    return VertexAttribType.Float;
            }
        }

        public static int GetElementsCount(VertexAttribDataType type)
        {
            switch(type)
            {
                case VertexAttribDataType.Bool:
                case VertexAttribDataType.Byte:
                case VertexAttribDataType.UnsignedByte:
                case VertexAttribDataType.Short:
                case VertexAttribDataType.UnsignedShort:
                case VertexAttribDataType.Int:
                case VertexAttribDataType.UnsignedInt:
                case VertexAttribDataType.Float:
                case VertexAttribDataType.Double:
                    return 1;
                case VertexAttribDataType.Byte2:
                case VertexAttribDataType.UnsignedByte2:
                case VertexAttribDataType.Bool2:
                case VertexAttribDataType.Int2:
                case VertexAttribDataType.UnsignedInt2:
                case VertexAttribDataType.Float2:
                case VertexAttribDataType.Double2:
                    return 2;
                case VertexAttribDataType.Bool3:
                case VertexAttribDataType.Byte3:
                case VertexAttribDataType.UnsignedByte3:
                case VertexAttribDataType.Int3:
                case VertexAttribDataType.UnsignedInt3:
                case VertexAttribDataType.Float3:
                case VertexAttribDataType.Double3:
                    return 3;
                case VertexAttribDataType.Bool4:
                case VertexAttribDataType.Byte4:
                case VertexAttribDataType.UnsignedByte4:
                case VertexAttribDataType.Int4:
                case VertexAttribDataType.UnsignedInt4:
                case VertexAttribDataType.Float4:
                case VertexAttribDataType.Double4:
                    return 4;
                case VertexAttribDataType.Mat3:
                case VertexAttribDataType.DMat3:
                    return 3 * 3;
                case VertexAttribDataType.Mat4:
                case VertexAttribDataType.DMat4:
                    return 4 * 4;
                default:
                    return 0;
            }
        }
    }


    public class VertexLayout
    {
        List<VertexAttrib> _attribs = new List<VertexAttrib>();

        public int AttibsLenght => _attribs.Count;

        public int Stride { get; protected set; } = 0;

        public VertexLayout() { }

        public VertexLayout AddAttrib(VertexAttrib attrib)
        {
            Stride += attrib.Stride;
            _attribs.Add(attrib);

            return this;
        }

        public void SetAttribs(VertexArray vao)
        {
            int offset = 0;
            for (int i = 0; i < _attribs.Count; i++)
            {
                GL.EnableVertexArrayAttrib(vao.ID, i);
                GL.VertexArrayAttribBinding(vao.ID, i, 0);
                GL.VertexArrayAttribFormat(vao.ID, i, _attribs[i].ElementsCount, _attribs[i].AttribType, _attribs[i].Normalized, offset);

                offset += _attribs[i].Stride;
            }
        }
    }


    public class VertexArray : OpenGLResource
    {
        public static int CurrentlyBinded { get; private set; } = 0;

        static Dictionary<string, VertexLayout> _layouts = new Dictionary<string, VertexLayout>();

        public VertexBuffer VertexBuffer { get; protected set; }

        public IndexBuffer IndexBuffer { get; protected set; }

        public VertexLayout VertexLayout { get; protected set; }

        public VertexArray(string vertexLayoutName, VertexBuffer vertexBuffer, IndexBuffer indexBuffer = null)
        {
            Insist.Assert(_layouts.ContainsKey(vertexLayoutName), "The vertex layout '{0}' doesn't exists.", vertexLayoutName);
            Insist.AssertNotNull(vertexBuffer, "Vertex buffer is required");

            GL.CreateVertexArrays(1, out _id);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.VertexArray);

            // Create VAO bindings
            _layouts.TryGetValue(vertexLayoutName, out var vertexLayout);
            VertexLayout = vertexLayout;

            GL.VertexArrayVertexBuffer(_id, 0, vertexBuffer.ID, IntPtr.Zero, VertexLayout.Stride);
            if(indexBuffer != null)
                GL.VertexArrayElementBuffer(_id, indexBuffer.ID);

            VertexLayout.SetAttribs(this);

            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
        }

        public void Bind()
        {
            if (CurrentlyBinded != _id)
            {
                GL.BindVertexArray(ID);
                CurrentlyBinded = ID;
            }
        }

        public void Unbind()
        {
            if (CurrentlyBinded == _id)
            {
                GL.BindVertexArray(0);
                CurrentlyBinded = 0;
            }
        }

        public void Draw()
        {
            Bind();

            if (IndexBuffer != null)
                GL.DrawElements(BeginMode.Triangles, IndexBuffer.DataLength, IndexBuffer.ElementsType, 0);
            else
                GL.DrawArrays(PrimitiveType.Triangles, 0, VertexBuffer.DataLength);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                Unbind();

                VertexBuffer.Dispose();
                IndexBuffer.Dispose();

                GL.DeleteVertexArray(_id);
                base.Dispose();
            }
        }

        public static void AddVertexLayout(string name, VertexLayout layout)
        {
            Insist.AssertFalse(_layouts.ContainsKey(name), "A layout with the name '{0}' already exists.", name);
            Insist.Assert(layout.AttibsLenght > 0, "Layout can't be empty.");

            _layouts.Add(name, layout);
        }
    }
}
