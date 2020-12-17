using System;
using System.Runtime.CompilerServices;

namespace Monorail.Renderer
{
    public enum ResourceType
    {
        VertexArray,
        Buffer,
        Texture,
        Framebuffer,
        Shader,
        ShaderProgram,
    }


    public abstract class OpenGLResource : IDisposable
    {
        protected int _id = 0;
        public int ID => _id;

        protected bool _disposed = false;

        //~OpenGLResource() => Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose() => _disposed = true;
    }
}
