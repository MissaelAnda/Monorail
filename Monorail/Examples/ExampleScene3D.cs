using Monorail.ECS;
using Monorail.Input;
using Monorail.Renderer;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.Examples
{
    class ExampleScene3D : Scene3D
    {
        public override void OnLoad()
        {
            var entity = CreateEntity();
            var local = _registry.GetComponent<Transform>(entity);
            local.Position = new Vector3(0, 0, 5);
            var mesh = new MeshRenderer(new CubeMesh());
            mesh.Texture = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\charliebrown.jpg", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });
            _registry.AddComponent(entity, mesh);

            entity = CreateEntity();
            local = _registry.GetComponent<Transform>(entity);
            local.Position = new Vector3(0, 0, 5);
            mesh = new MeshRenderer(new CubeMesh());
            // mesh.Texture = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\blender2.83.png", new TextureBuilder()
            // {
            //     GenerateMipmaps = true,
            // });
            _registry.AddComponent(entity, mesh);
        }

        public override void OnUpdate(float delta)
        {
            if (Keyboard.IsKeyPressed(Keys.Tab))
            {
                Camera3D.ProjectionType = Camera3D.ProjectionType == ProjectionType.Perspective ?
                    ProjectionType.Orthographic : ProjectionType.Perspective;
            }
        }
    }
}
