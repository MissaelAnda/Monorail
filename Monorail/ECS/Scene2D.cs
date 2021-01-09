using Necs;

namespace Monorail.ECS
{
    public class Scene2D : Scene
    {
        public Camera2D Camera2D => Camera as Camera2D;

        public Scene2D()
        {
            // Editor camera must only be added in edit mode
            var editorCameraEntity = _registry.CreateEntity();

            var cameraTransform = new Transform2D(editorCameraEntity);
            var editorCamera = new Camera2D(cameraTransform);

            _registry.AddComponent(editorCameraEntity, cameraTransform);
            _registry.AddComponent(editorCameraEntity, editorCamera);
            Camera = editorCamera;
        }

        public override Entity CreateEntity(Transform2D parent = null)
        {
            var entity = _registry.CreateEntity();
            var transform = new Transform2D(entity);
            if (parent != null)
                transform.Parent = parent;
            _registry.AddComponent(entity, transform);
            _registry.AddComponent(entity, new TagComponent("GameObject"));
            return entity;
        }
    }
}
