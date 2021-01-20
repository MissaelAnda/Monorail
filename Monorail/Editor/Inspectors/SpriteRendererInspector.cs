using ImGuiNET;
using Monorail.ECS;
using Monorail.Util;
using ImVec2 = System.Numerics.Vector2;
using ImVec4 = System.Numerics.Vector4;

namespace Monorail.Editor
{
    public static class SpriteRendererInspector
    {
        public static void Inspect(SpriteRenderer spriteRenderer)
        {
            if (ImGui.TreeNodeEx("Sprite Renderer", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var color = spriteRenderer.Modulate.ToImVec4();
                ImGui.Image(spriteRenderer.Sprite.Texture, new ImVec2(200, 200), new ImVec2(0, 1), new ImVec2(1, 0), color);

                ImGui.ColorEdit4("Modulate", ref color);
                spriteRenderer.Modulate = color.ToColor4();

                float depth = spriteRenderer.Depth;
                ImGui.SliderFloat("Depth", ref depth, -1.0f, 1.0f);
                spriteRenderer.Depth = depth;

                ImGui.TreePop();
            }
        }
    }
}
