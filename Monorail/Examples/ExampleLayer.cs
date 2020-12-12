using Monorail.Debug;
using OpenTK.Windowing.Common;

namespace Monorail.Examples
{
    public class ExampleLayer : Layer
    {
        public ExampleLayer() : base("ExampleLayer")
        { }

        public override void OnAttached()
        {
            Log.Client.Succes("Example layer attached.");
        }

        public override void OnDetached()
        {
            Log.Client.Warn("Example layer detached.");
        }

        public override void OnRegisterEvents(ref EventList eventManager)
        {
            eventManager.Resize = OnResize;
        }

        public void OnResize(ResizeEventArgs args)
        {
            Log.Client.Info("Window resized: ({0}, {1})", args.Width, args.Height);
        }
    }
}
