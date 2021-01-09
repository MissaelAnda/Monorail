using Necs;
using System;
using ImGuiNET;
using Monorail.ECS;

namespace Monorail.Editor
{
    public static class Hierarchy
    {
        public static Entity SelectedEntity { get; private set; } = Entity.Invalid;

        static ImGuiTreeNodeFlags DefaultFlags;

        static Hierarchy()
        {
            DefaultFlags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
        }

        public static void Process()
        {
            ImGui.Begin("Hierarchy");
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Button("Create new entity"))
                {
                    SelectedEntity = EditorManager.CurrentScene.CreateEntity();
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            EditorManager.CurrentScene._registry
                .GetView(typeof(Transform2D), typeof(TagComponent)).Each(Entities);
            ImGui.End();
        }

        public static void Entities(Group group)
        {
            var transform = group.Get<Transform2D>();
            if (transform.Parent == null)
            {
                DrawEntity(transform);
            }
        }

        public static void DrawEntity(Transform2D transform)
        {
            ImGuiTreeNodeFlags nodeFlags = DefaultFlags;
            if (SelectedEntity == transform.Entity)
                nodeFlags |= ImGuiTreeNodeFlags.Selected;

            var tag = EditorManager.CurrentScene._registry.GetComponentRef<TagComponent>(transform.Entity);
            bool opened = ImGui.TreeNodeEx((IntPtr)transform.Entity.Id, nodeFlags, $"{tag.Tag}");
            
            //if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            //    ImGui.BeginPopupContextWindow()

            if (ImGui.IsItemClicked())
                SelectedEntity = transform.Entity;

            EntityPopupMenu(transform);

            if (opened)
            {
                for (int i = 0; i < transform.Children.Count; i++)
                {
                    DrawEntity(transform.Children[i]);
                }
                ImGui.TreePop();
            }
        }

        static void EntityPopupMenu(Transform2D transform)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Button("Delete"))
                {
                    //EditorManager.CurrentScene.DeleteEntity(transform.Entity);
                    //if (SelectedEntity == transform.Entity)
                    //    SelectedEntity = Entity.Invalid;
                }
                if (ImGui.Button("Create Child Entity"))
                {
                    EditorManager.CurrentScene.CreateEntity(transform);
                }
                ImGui.EndPopup();
            }
        }
    }
}
