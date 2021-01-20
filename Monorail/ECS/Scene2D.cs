using Necs;
using Monorail.Renderer;
using OpenTK.Graphics.OpenGL4;

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
            editorCamera.MaxZoom = 10;

            _registry.AddComponent(editorCameraEntity, cameraTransform);
            _registry.AddComponent(editorCameraEntity, editorCamera);
            Camera = editorCamera;
        }

        public override Entity CreateEntity(Entity? parent = null)
        {
            var entity = _registry.CreateEntity();

            var transform = new Transform2D(entity);
            if (parent.HasValue)
                transform.Parent = _registry.GetComponent<Transform2D>(parent.Value);

            _registry.AddComponent(entity, transform);
            _registry.AddComponent(entity, new TagComponent("GameObject"));

            return entity;
        }

        public override void DeleteEntity(Entity entity)
        {
            var transform = _registry.GetComponent<Transform2D>(entity);
            transform.Parent = null;

            for (int i = 0; i < transform.Children.Count; i++)
                DeleteEntity(transform.Children[i].Entity);

            _registry.DeleteEntity(entity);
        }

        public override void Update(float delta)
        {
            
        }

        public override void Render(float delta)
        {
            RenderTarget.Bind();

            RenderCommand.SetClearColor(ClearColor);
            RenderCommand.SetCaps(EnableCap.Blend);
            RenderCommand.Clear();

            Renderer2D.Begin(Camera2D);
            foreach (var entity in _registry.GetView(typeof(SpriteRenderer), typeof(Transform2D)).Each())
            {
                entity.Get<SpriteRenderer>().Render(entity.Get<Transform2D>());
            }
            Renderer2D.End();

            RenderTarget.Unbind();
        }
    }
}
