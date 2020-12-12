namespace Monorail
{
    public abstract class Layer
    {
        public readonly string Name;

        public Layer(string name) => Name = name;

        public virtual void OnAttached() { }
        public virtual void OnUpdate(double delta) { }
        public virtual void OnRegisterEvents(ref EventList eventManager) { }
        public virtual void OnDetached() { }
    }
}
