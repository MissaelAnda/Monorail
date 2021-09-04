using Monorail.ECS;
using Monorail.Renderer;
using OpenTK.Mathematics;
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
        }
    }
}
