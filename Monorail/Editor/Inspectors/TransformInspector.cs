﻿using Necs;
using ImGuiNET;
using Monorail.ECS;
using Monorail.Util;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using System;
using OpenTK.Mathematics;

namespace Monorail.Editor
{
    public static class TransformInspector
    {
        enum Rotation
        {
            Radians,
            Degrees,
        }

        static Rotation _rotation = Rotation.Radians;
        static bool _local = true;

        public static void Inspect(Entity entity)
        {
            if (ImGui.TreeNodeEx("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var registry = EditorManager.CurrentScene._registry;
                try
                {
                    var transform = registry.GetComponent<Transform2D>(entity);
                    Transform2D(transform);
                } catch (Exception)
                {
                    var transform = registry.GetComponent<Transform>(entity);
                    Transform3D(transform);
                }
            }
        }

        public static void Transform2D(Transform2D transform)
        {
            ImGui.Checkbox("Local", ref _local);

            // Position
            ImVec2 oldPosition = (_local ? transform.LocalPosition : transform.Position).ToImVec2();
            ImVec2 position = oldPosition;
            Inspector.EditVector2("Position", ref position, ImVec2.Zero);
            if (position != oldPosition)
            {
                if (_local)
                    transform.LocalPosition = position.ToVector2();
                else
                    transform.Position = position.ToVector2();
            }

            // Scale
            ImVec2 oldScale = (_local ? transform.LocalScale : transform.Scale).ToImVec2();
            ImVec2 scale = oldScale;
            Inspector.EditVector2("Scale", ref scale, ImVec2.One);
            if (scale != oldScale)
            {
                if (_local)
                    transform.LocalScale = scale.ToVector2();
                else
                    transform.Scale = scale.ToVector2();
            }

            // Rotation
            var textWidth = (ImGui.CalcItemWidth() / 5) * 2;
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, textWidth);
            ImGui.Text("Rotation");
            ImGui.SameLine();

            float lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, ImVec2.Zero);
            if (ImGui.Button(_rotation == Rotation.Radians ? "R" : "D", new ImVec2(lineHeight - 3, lineHeight - 6)))
                _rotation = _rotation == Rotation.Radians ? Rotation.Degrees : Rotation.Radians;
            ImGui.PopStyleVar();
            // Tooltip
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted($"Toggles rotation to " + (_rotation == Rotation.Radians ? "degrees" : "radians"));
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.CalcItemWidth() / 2);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new ImVec2(0));

            var oldRotation = _rotation == Rotation.Radians ?
                _local ? transform.LocalRotation : transform.Rotation :
                _local ? transform.LocalRotationDegrees : transform.RotationDegrees;
            var rotation = oldRotation;
            Inspector.FloatDrag("X", ref rotation, () => 0, new ImVec2(lineHeight + 3, lineHeight), new ImVec3[] {
                    new ImVec3(216f / 255f, 52f / 255f, 95f / 255f),
                    new ImVec3(196f / 255f, 32f / 255f, 75f / 255f),
                    new ImVec3(236f / 255f, 72f / 255f, 115f / 255f)
                }, 0.001f);

            if (rotation != oldRotation)
            {
                if (_rotation == Rotation.Radians)
                {
                    if (_local)
                        transform.LocalRotation = rotation;
                    else
                        transform.Rotation = rotation;
                }
                else
                {
                    if (_local)
                        transform.LocalRotationDegrees = rotation;
                    else
                        transform.RotationDegrees = rotation;
                }
            }

            ImGui.PopStyleVar();
            ImGui.PopItemWidth();
            ImGui.Columns(1);

            ImGui.TreePop();
        }

        public static void Transform3D(Transform transform)
        {
            ImGui.Checkbox("Local", ref _local);

            // Position
            ImVec3 oldPosition = (_local ? transform.LocalPosition : transform.Position).ToImVec3();
            ImVec3 position = oldPosition;
            Inspector.EditVector3("Position", ref position, ImVec3.Zero);
            if (position != oldPosition)
            {
                if (_local)
                    transform.LocalPosition = position.ToVector3();
                else
                    transform.Position = position.ToVector3();
            }

            // Scale
            ImVec3 oldScale = (_local ? transform.LocalScale : transform.Scale).ToImVec3();
            ImVec3 scale = oldScale;
            Inspector.EditVector3("Scale", ref scale, ImVec3.One);
            if (scale != oldScale)
            {
                if (_local)
                    transform.LocalScale = scale.ToVector3();
                else
                    transform.Scale = scale.ToVector3();
            }

            // Tempo Scale
            ImVec3 oldRotation = (_local ? transform.LocalRotation : transform.Rotation).ToEulerAngles().ToImVec3();
            ImVec3 rotation = oldRotation;
            Inspector.EditVector3("Rotation", ref rotation, ImVec3.One);
            if (oldRotation != rotation)
            {
                if (_local)
                    transform.LocalRotation = Quaternion.FromEulerAngles(rotation.ToVector3());
                else
                    transform.Rotation = Quaternion.FromEulerAngles(rotation.ToVector3());
            }

            // Rotation
            //var textWidth = (ImGui.CalcItemWidth() / 5) * 2;
            //ImGui.Columns(2);
            //ImGui.SetColumnWidth(0, textWidth);
            //ImGui.Text("Rotation");
            //ImGui.SameLine();
            //
            //float lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2;
            //
            //ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, ImVec2.Zero);
            //if (ImGui.Button(_rotation == Rotation.Radians ? "R" : "D", new ImVec2(lineHeight - 3, lineHeight - 6)))
            //    _rotation = _rotation == Rotation.Radians ? Rotation.Degrees : Rotation.Radians;
            //ImGui.PopStyleVar();
            //// Tooltip
            //if (ImGui.IsItemHovered())
            //{
            //    ImGui.BeginTooltip();
            //    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            //    ImGui.TextUnformatted($"Toggles rotation to " + (_rotation == Rotation.Radians ? "degrees" : "radians"));
            //    ImGui.PopTextWrapPos();
            //    ImGui.EndTooltip();
            //}
            //ImGui.NextColumn();
            //
            //ImGui.PushItemWidth(ImGui.CalcItemWidth() / 2);
            //ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new ImVec2(0));
            //
            //var oldRotation = _rotation == Rotation.Radians ?
            //    _local ? transform.LocalRotation : transform.Rotation :
            //    _local ? transform.LocalRotationDegrees : transform.RotationDegrees;
            //var rotation = oldRotation;
            //Inspector.FloatDrag("X", ref rotation, () => 0, new ImVec2(lineHeight + 3, lineHeight), new ImVec3[] {
            //        new ImVec3(216f / 255f, 52f / 255f, 95f / 255f),
            //        new ImVec3(196f / 255f, 32f / 255f, 75f / 255f),
            //        new ImVec3(236f / 255f, 72f / 255f, 115f / 255f)
            //    }, 0.001f);
            //
            //if (rotation != oldRotation)
            //{
            //    if (_rotation == Rotation.Radians)
            //    {
            //        if (_local)
            //            transform.LocalRotation = rotation;
            //        else
            //            transform.Rotation = rotation;
            //    }
            //    else
            //    {
            //        if (_local)
            //            transform.LocalRotationDegrees = rotation;
            //        else
            //            transform.RotationDegrees = rotation;
            //    }
            //}
            //
            //ImGui.PopStyleVar();
            //ImGui.PopItemWidth();
            //ImGui.Columns(1);

            ImGui.TreePop();
        }

    }
}
