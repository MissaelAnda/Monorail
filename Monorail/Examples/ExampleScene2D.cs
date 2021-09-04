using Monorail.ECS;
using Monorail.Renderer;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.Examples
{
    public class ExampleScene2D : Scene2D
    {
        Transform2D _parent;

        public override void OnLoad()
        {
            var text = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\blender2.83.png", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });
            var background = Texture2D.FromPath("C:\\Users\\guita\\Pictures\\charliebrown.jpg", new TextureBuilder()
            {
                GenerateMipmaps = true,
            });
            
            var entity = CreateEntity();
            _parent = _registry.GetComponent<Transform2D>(entity);
            var sprite = new SpriteRenderer(text);
            sprite.Sprite.Source = new System.Drawing.Rectangle(200, 200, 1000, 500);
            _registry.AddComponent(entity, sprite);
            
            entity = CreateEntity(entity);
            var quad = _registry.GetComponent<Transform2D>(entity);
            _registry.AddComponent(entity, new SpriteRenderer(background));
            
            _parent.Scale = new Vector2(500, 400);
            quad.LocalPosition = new Vector2(.5f, .5f);
            quad.LocalScale = new Vector2(.5f, .5f);
        }

        public override void OnUpdate(float delta)
        {
            _parent.RotationDegrees += delta;
        }
    }
}
