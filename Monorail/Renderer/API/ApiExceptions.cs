using System;
using OpenTK.Graphics.OpenGL4;

namespace Monorail.Renderer
{
    public class OpenGLException : Exception
    {
        public readonly ErrorCode ErrorCode;

        public OpenGLException()
        {
            ErrorCode = GL.GetError();
        }

        public OpenGLException(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }
    }

    public class OpenGLResourceCreationException : OpenGLException
    {
        public readonly ResourceType ResourceType;

        public OpenGLResourceCreationException(ResourceType type) : base()
        {
            ResourceType = type;
        }
    }
}
