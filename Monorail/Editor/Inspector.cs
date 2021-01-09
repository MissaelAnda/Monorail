using Necs;
using System;
using ImGuiNET;
using Monorail.ECS;
using Monorail.Util;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;

namespace Monorail.Editor
{
    public static class Inspector
    {
        enum Rotation
        {
            Radians,
            Degrees,
        }

        static Rotation _rotation = Rotation.Radians;
        static bool _local = false;

        public static void Process()
        {
            ImGui.Begin("Inspector");
            var entity = Hierarchy.SelectedEntity;
            if (entity.IsValid())
            {
                ImGui.Text($"Inspecting: {Hierarchy.SelectedEntity}");
                Transform(entity);
            }
            else ImGui.Text("None entity selected");
            ImGui.End();
        }

        static void Transform(Entity entity)
        {
            if (ImGui.TreeNodeEx("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var transform = EditorManager.CurrentScene._registry.GetComponent<Transform2D>(entity);

                // Position
                ImVec2 oldPosition = transform.LocalPosition.ToImVec2();
                ImVec2 position = oldPosition;
                EditVector2("Position", ref position, ImVec2.Zero);
                if (position != oldPosition)
                    transform.LocalPosition = position.ToVector2();

                // Scale
                ImVec2 oldScale = transform.LocalScale.ToImVec2();
                ImVec2 scale = oldScale;
                EditVector2("Scale", ref scale, ImVec2.One);
                if (scale != oldScale)
                    transform.LocalScale = scale.ToVector2();

                // Rotation
                var textWidth = ImGui.CalcItemWidth() / 3;
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, textWidth);
                ImGui.Text("Rotaion");
                ImGui.NextColumn();

                ImGui.PushItemWidth(ImGui.CalcItemWidth() / 2);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new ImVec2(0));

                float lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;

                var rotation = transform.LocalRotation;
                FloatDrag("X", ref rotation, () => 0, new ImVec2(lineHeight + 3, lineHeight), new ImVec3[] {
                        new ImVec3(216f / 255f, 52f / 255f, 95f / 255f),
                        new ImVec3(196f / 255f, 32f / 255f, 75f / 255f),
                        new ImVec3(236f / 255f, 72f / 255f, 115f / 255f)
                    }, 0.001f);

                if (rotation != transform.LocalRotation)
                    transform.LocalRotation = rotation;

                ImGui.PopStyleVar();
                ImGui.PopItemWidth();

                ImGui.TreePop();
            }
        }

        static void EditVector3(string label, ref ImVec3 vector, ImVec3 reset)
        {
            var textWidth = ImGui.CalcItemWidth() / 3;
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

        static void EditVector2(string label, ref ImVec2 vector, ImVec2 reset)
        {
            var textWidth = ImGui.CalcItemWidth() / 3;
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

        static void FloatDrag(string label, ref float val, Func<float> onLabelClicked, ImVec2 buttonSize, ImVec3[] colors, float speed = 0.1f)
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
