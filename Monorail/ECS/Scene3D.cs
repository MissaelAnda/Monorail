using Necs;
using System;
using Monorail.Renderer;
using OpenTK.Graphics.OpenGL4;

namespace Monorail.ECS
{
    public class Scene3D : Scene
    {
        public Camera3D Camera3D => Camera as Camera3D;

        public Scene3D()
        {
            var editorCameraEntity = _registry.CreateEntity();

            var cameraTransform = new Transform(editorCameraEntity);
            var editorCamera = new Camera3D(cameraTransform);

            _registry.AddComponent(editorCameraEntity, cameraTransform);
            _registry.AddComponent(editorCameraEntity, editorCamera);
            Camera = editorCamera;

            OnLoad();
        }

        public sealed override Entity CreateEntity(Entity? parent = null)
        {
            var entity = _registry.CreateEntity();

            var transform = new Transform(entity);
            if (parent.HasValue)
                transform.Parent = _registry.GetComponent<Transform>(parent.Value);

            _registry.AddComponent(entity, transform);
            _registry.AddComponent(entity, new TagComponent("GameObject"));

            return entity;
        }

        public sealed override void DeleteEntity(Entity entity)
        {
            var transform = _registry.GetComponent<Transform>(entity);
            transform.Parent = null;

            for (int i = 0; i < transform.Children.Count; i++)
                DeleteEntity(transform.Children[i].Entity);

            _registry.DeleteEntity(entity);
        }

        public sealed override void Render(float delta)
        {
            BeforeRender(delta);
            RenderTarget.Bind();

            BeforeRender(delta);
            // TODO: Add Gamma correction

            RenderCommand.SetClearColor(ClearColor);
            RenderCommand.SetCaps(EnableCap.Blend, EnableCap.DepthTest);
            RenderCommand.SetClearMasks(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RenderCommand.Clear();

            Renderer3D.Begin(Camera3D);
            foreach (var (mesh, transform) in _registry.GetView(typeof(MeshRenderer), typeof(Transform)).Unpack<MeshRenderer, Transform>())
            {
                Renderer3D.DrawMesh(mesh, transform);
            }
            Renderer3D.End();

            // TODO: Add to delta the rendering time-lapse
            AfterRender(delta);

            RenderTarget.Unbind();
        }

        public sealed override void Update(float delta)
        {
            OnUpdate(delta);
        }
    }
}
