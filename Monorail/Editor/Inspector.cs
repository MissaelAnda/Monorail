using Necs;
using System;
using ImGuiNET;
using Monorail.ECS;
using Monorail.Input;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Monorail.Editor
{
    public static class Inspector
    {
        static string _tag = "";

        public static void Process()
        {
            ImGui.Begin("Inspector");
            var entity = Hierarchy.SelectedEntity;
            if (entity.IsValid)
            {
                Tag(entity);
                Transform2DInspector.Inspect(entity);

                // remove redundancy
                if (EditorManager.CurrentScene._registry.HasComponent<SpriteRenderer>(entity))
                    SpriteRendererInspector.Inspect(EditorManager.CurrentScene._registry.GetComponent<SpriteRenderer>(entity));
            }
            else ImGui.Text("None entity selected");
            ImGui.End();
        }

        public static void OnSelected(Entity entity)
        {
            _tag = EditorManager.CurrentScene._registry.GetComponentRef<TagComponent>(entity).Tag;
        }

        static void Tag(Entity entity)
        {
            if (ImGui.InputText("Tag", ref _tag, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                EditorManager.CurrentScene._registry.GetComponentRef<TagComponent>(entity).Tag = _tag;
            if (ImGui.IsItemFocused() && Keyboard.IsKeyPressed(Keys.Escape))
                _tag = EditorManager.CurrentScene._registry.GetComponentRef<TagComponent>(entity).Tag;
            ImGui.Separator();
        }

        public static void EditVector3(string label, ref ImVec3 vector, ImVec3 reset)
        {
            var textWidth = (ImGui.CalcItemWidth() / 5) * 2;
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, textWidth);
            ImGui.Text(label);
            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new ImVec2(0));
            ImGui.PushItemWidth(ImGui.CalcItemWidth() / 3);

            float lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;
            var buttonSize = new ImVec2(lineHeight + 3, lineHeight);

            // X var
            FloatDrag($"##X{label}", ref vector.X, () => reset.X, buttonSize, new ImVec3[] {
                new ImVec3(216f / 255f, 52f / 255f, 95f / 255f),
                new ImVec3(196f / 255f, 32f / 255f, 75f / 255f),
                new ImVec3(236f / 255f, 72f / 255f, 115f / 255f)
            });

            // Y var
            ImGui.SameLine();
            FloatDrag($"##Y{label}", ref vector.Y, () => reset.Y, buttonSize, new ImVec3[]
            {
                new ImVec3(1f, 214f / 255f, 107f / 255f),
                new ImVec3(1f, 234f / 255f, 127f / 255f),
                new ImVec3(235f / 255f, 194f / 255f, 87f / 255f)
            });

            // Z var
            ImGui.SameLine();
            FloatDrag($"##Z{label}", ref vector.Z, () => reset.Z, buttonSize, new ImVec3[]
            {
                new ImVec3(88f / 255f, 141f / 255f, 168f / 255f),
                new ImVec3(108f / 255f, 161f / 255f, 188f / 255f),
                new ImVec3(68f / 255f, 121f / 255f, 148f / 255f)
            });

            ImGui.PopItemWidth();
            ImGui.PopStyleVar();
            ImGui.Columns(1);
        }

        public static void EditVector2(string label, ref ImVec2 vector, ImVec2 reset)
        {
            var textWidth = (ImGui.CalcItemWidth() / 5) * 2;
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, textWidth);
            ImGui.Text(label);
            ImGui.NextColumn();

            float lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;
            var buttonSize = new ImVec2(lineHeight + 3, lineHeight);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new ImVec2(0));
            ImGui.PushItemWidth(ImGui.CalcItemWidth() / 2);

            // X var
            FloatDrag($"X{label}", ref vector.X, () => reset.X, buttonSize, new ImVec3[] {
                new ImVec3(216f / 255f, 52f / 255f, 95f / 255f),
                new ImVec3(196f / 255f, 32f / 255f, 75f / 255f),
                new ImVec3(236f / 255f, 72f / 255f, 115f / 255f)
            });

            // Y var
            ImGui.SameLine();
            FloatDrag($"Y{label}", ref vector.Y, () => reset.Y, buttonSize, new ImVec3[]
            {
                new ImVec3(1f, 214f / 255f, 107f / 255f),
                new ImVec3(1f, 234f / 255f, 127f / 255f),
                new ImVec3(235f / 255f, 194f / 255f, 87f / 255f)
            });

            ImGui.PopItemWidth();
            ImGui.PopStyleVar();
            ImGui.Columns(1);
        }

        public static void FloatDrag(string label, ref float val, Func<float> onLabelClicked, ImVec2 buttonSize, ImVec3[] colors, float speed = 0.1f)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new ImVec4(colors[0], 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImVec4(colors[1], 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImVec4(colors[2], 1f));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
            if (ImGui.Button(label[0].ToString(), buttonSize))
                val = onLabelClicked.Invoke();
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new ImVec4(colors[0], 0.6f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new ImVec4(colors[1], 0.6f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new ImVec4(colors[2], 0.6f));
            ImGui.SameLine();
            ImGui.DragFloat($"##{label}", ref val, speed);
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(6);
        }
    }
}
