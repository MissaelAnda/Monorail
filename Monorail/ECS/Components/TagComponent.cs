using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.ECS
{
    public struct TagComponent
    {
        public string Tag;

        public TagComponent(string tag)
        {
            Tag = tag;
        }
    }
}
