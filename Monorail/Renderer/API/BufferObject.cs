using System;
using Monorail.Debug;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace Monorail.Renderer
{
    public class BufferObject : OpenGLResource
    {
        public readonly BufferTarget Target;
        
        public BufferUsageHint Usage { get; protected set; }

        /// <summary>
        /// The size in bytes
        /// </summary>
        public int DataSize { get; protected set; } = 0;

        /// <summary>
        /// The amount of items this buffers has
        /// </summary>
        public int DataLength { get; protected set; } = 0;

        /// <summary>
        /// The size in bytes of the currently use data
        /// </summary>
        public int ElementSize { get; protected set; } = 0;

        public BufferObject(BufferTarget target)
        {
            GL.CreateBuffers(1, out _id);
            if (_id <= 0)
                throw new OpenGLResourceCreationException(ResourceType.Buffer);

            Target = target;
        }

        public virtual void SetData<T>(T[] data, BufferUsageHint usage) where T : struct
        {
            Usage = usage;

            DataLength = data.Length;
            DataSize = DataLength * Unsafe.SizeOf<T>();

            GL.NamedBufferData(_id, DataSize, data, Usage);
        }

        public virtual void SetElementSize(int size)
        {
            ElementSize = size;
        }

        public virtual void SetSubData(int elements, IntPtr data, IntPtr? offset = null)
        {
            GL.NamedBufferSubData(_id, offset.HasValue ? offset.Value : IntPtr.Zero, elements * ElementSize, data);
        }

        public virtual void AllocateEmpty(int capacity, BufferUsageHint usage)
        {
            Usage = usage;
            DataLength = capacity;
            DataSize = capacity * ElementSize;

            GL.NamedBufferData(_id, DataSize, IntPtr.Zero, Usage);
        }

        public virtual void Bind()
        {
            if (!_disposed)
            {
                GL.BindBuffer(Target, _id);
            }
        }

        public override void Dispose()
        {
            if(!_disposed)
            {
                GL.DeleteBuffer(_id);
                base.Dispose();
            }
        }
    }

    public class VertexBuffer : BufferObject
    {
        public VertexArray VAO { get; set; }

        public VertexBuffer() : base(BufferTarget.ArrayBuffer) { }

        public override void Bind()
        {
            VAO.Bind();
            base.Bind();
        }
    }

    public class IndexBuffer : BufferObject
    {
        public DrawElementsType ElementsType;

        public IndexBuffer() : base(BufferTarget.ElementArrayBuffer) { }

        public override void SetData<T>(T[] data, BufferUsageHint usage)
        {
            if (data is uint[])
                ElementsType = DrawElementsType.UnsignedInt;
            else if (data is ushort[])
                ElementsType = DrawElementsType.UnsignedShort;
            else if (data is byte[])
                ElementsType = DrawElementsType.UnsignedByte;
            else
                Insist.Fail("Data type must be unsingend int, short or byte.");

            base.SetData(data, usage);
        }
    }
}
