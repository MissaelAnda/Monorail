using Necs;
using System;
using ImGuiNET;
using Monorail.ECS;
using ImVec2 = System.Numerics.Vector2;
using ImVec4 = System.Numerics.Vector4;

namespace Monorail.Editor
{
    public static class Hierarchy
    {
        public static Entity SelectedEntity { get; private set; } = Entity.Invalid;

        public static Entity ToDelete { get; private set; } = Entity.Invalid;

        static ImGuiTreeNodeFlags DefaultFlags;

        static bool Open = true;

        static Hierarchy()
        {
            DefaultFlags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
        }

        public static void Process()
        {
            if (ImGui.Begin("Hierarchy", ref Open))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new ImVec2(0));

                if (ImGui.BeginPopupContextWindow("##Options", ImGuiMouseButton.Right, false))
                {
                    if (ImGui.Selectable("Create new entity"))
                    {
                        SelectedEntity = EditorManager.CurrentScene.CreateEntity();
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
                ImGui.PopStyleVar();

                var registry = EditorManager.CurrentScene._registry;
                if (EditorManager.CurrentScene is Scene2D)
                    registry
                        .GetView(typeof(Transform2D), typeof(TagComponent))
                        .Each(Entities2D);
                else
                    registry
                        .GetView(typeof(Transform), typeof(TagComponent))
                        .Each(Entities);

                // To delete
                if (ToDelete.IsValid)
                    DeleteDialog();

                ImGui.End();
            }
        }

        public static void Entities(Group group)
        {
            var transform = group.Get<Transform>();
            if (transform.Parent == null)
            {
                DrawEntity(transform);
            }
        }

        public static void Entities2D(Group group)
        {
            var transform = group.Get<Transform2D>();
            if (transform.Parent == null)
            {
                DrawEntity2D(transform);
            }
        }

        public static void DrawEntity2D(Transform2D transform)
        {
            ImGuiTreeNodeFlags nodeFlags = DefaultFlags;
            if (SelectedEntity == transform.Entity)
                nodeFlags |= ImGuiTreeNodeFlags.Selected;

            var tag = EditorManager.CurrentScene._registry.GetComponentRef<TagComponent>(transform.Entity);
            bool opened = ImGui.TreeNodeEx((IntPtr)transform.Entity.Id, nodeFlags, $"{tag.Tag}");

            if (ImGui.IsItemClicked() && transform.Entity != SelectedEntity)
            {
                SelectedEntity = transform.Entity;
                Inspector.OnSelected(transform.Entity);
            }

            EntityPopupMenu2D(transform);

            if (opened)
            {
                for (int i = 0; i < transform.Children.Count; i++)
                    DrawEntity2D(transform.Children[i]);
                ImGui.TreePop();
            }
        }

        public static void DrawEntity(Transform transform)
        {
            ImGuiTreeNodeFlags nodeFlags = DefaultFlags;
            if (SelectedEntity == transform.Entity)
                nodeFlags |= ImGuiTreeNodeFlags.Selected;

            var tag = EditorManager.CurrentScene._registry.GetComponentRef<TagComponent>(transform.Entity);
            bool opened = ImGui.TreeNodeEx((IntPtr)transform.Entity.Id, nodeFlags, $"{tag.Tag}");

            if (ImGui.IsItemClicked() && transform.Entity != SelectedEntity)
            {
                SelectedEntity = transform.Entity;
                Inspector.OnSelected(transform.Entity);
            }

            EntityPopupMenu(transform);

            if (opened)
            {
                for (int i = 0; i < transform.Children.Count; i++)
                    DrawEntity(transform.Children[i]);
                ImGui.TreePop();
            }
        }

        static void EntityPopupMenu2D(Transform2D transform)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new ImVec2(0));
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("Create Child Entity"))
                    EditorManager.CurrentScene.CreateEntity(transform.Entity);
                if (ImGui.Selectable("Delete"))
                    ToDelete = transform.Entity;
                ImGui.EndPopup();
            }
            ImGui.PopStyleVar();
        }

        static void EntityPopupMenu(Transform transform)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new ImVec2(0));
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("Create Child Entity"))
                    EditorManager.CurrentScene.CreateEntity(transform.Entity);
                if (ImGui.Selectable("Delete"))
                    ToDelete = transform.Entity;
                ImGui.EndPopup();
            }
            ImGui.PopStyleVar();
        }

        static void DeleteDialog()
        {
            ImVec2 center = new ImVec2(ImGui.GetIO().DisplaySize.X * 0.5f, ImGui.GetIO().DisplaySize.Y * 0.5f);
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new ImVec2(0.5f, 0.5f));

            bool open = true;
            ImGui.OpenPopup("Are you sure you want to delete this entity?");
            if (ImGui.BeginPopupModal("Are you sure you want to delete this entity?", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("All children entities will be deleted aswell");
                ImGui.Text("All those entities will be lost, like tears in the rain");
                ImGui.Separator();

                ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, ImVec2.Zero);

                var buttonSize = new ImVec2(ImGui.CalcItemWidth() / 2, ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2);

                ImGui.PushStyleColor(ImGuiCol.Button, new ImVec4(04f, 0.05f, 0.05f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new ImVec4(03f, 0.05f, 0.05f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImVec4(05f, 0.05f, 0.05f, 1f));
                if (ImGui.Button("Delete", buttonSize))
                {
                    EditorManager.CurrentScene.DeleteEntity(ToDelete);
                    if (SelectedEntity == ToDelete)
                        SelectedEntity = Entity.Invalid;
                    ToDelete = Entity.Invalid;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.PopStyleColor(3);

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", buttonSize))
                {
                    ToDelete = Entity.Invalid;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.PopStyleVar();
                ImGui.EndPopup();
            }
            else ToDelete = Entity.Invalid;
        }
    }
}
