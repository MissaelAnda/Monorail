using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.ECS
{
    public class Scene2D : Scene
    {
        public Camera2D Camera2D => Camera as Camera2D;

        public Scene2D(Camera2D camera)
        {
            Camera = camera;
        }
    }
}
