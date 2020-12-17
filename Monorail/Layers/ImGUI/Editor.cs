using System;
using ImGuiNET;
using System.Numerics;

namespace Monorail.Layers.ImGUI
{
    public static class Editor
    {
        public static void Start()
        {
            SetupStyle();

            StartDockspace();

            //ImGui.ShowStyleEditor();

            ImGui.Begin("View");
            ImGui.Image((IntPtr)App.Framebuffer.Color, new Vector2(800, 600), new Vector2(0f, 1f), new Vector2(1, 0));
            ImGui.End();
        }

        public static void SetupStyle()
        {
            //ImGui.StyleColorsDark();
            var style = ImGui.GetStyle();
            style.FrameRounding = 1;
            style.WindowRounding = 0;
            style.WindowBorderSize = 0;
            style.ScrollbarRounding = 2;
            style.TabRounding = 0;
        }

        public static void StartDockspace()
        {
            ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None;

            // We are using the ImGuiWindowFlags_NoDocking flag to make the parent window not dockable into,
            // because it would be confusing to have two docking targets within each others.
            ImGuiWindowFlags window_flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            window_flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            window_flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

            // When using ImGuiDockNodeFlags_PassthruCentralNode, DockSpace() will render our background
            // and handle the pass-thru hole, so we ask Begin() to not render a background.
            window_flags |= ImGuiWindowFlags.NoBackground;

            // Important: note that we proceed even if Begin() returns false (aka window is collapsed).
            // This is because we want to keep our DockSpace() active. If a DockSpace() is inactive,
            // all active windows docked into it will lose their parent and become undocked.
            // We cannot preserve the docking relationship between an active window and an inactive docking, otherwise
            // any change of dockspace/settings would lead to windows being stuck in limbo and never being visible.
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.Begin("DockSpace Demo", window_flags);

            // Pop padding style
            ImGui.PopStyleVar();

            // Pop fullscreen style
            ImGui.PopStyleVar(2);

            // DockSpace
            uint dockspace_id = ImGui.GetID("EditorDockspace");
            ImGui.DockSpace(dockspace_id, new Vector2(0.0f, 0.0f), dockspace_flags);

            StartMenuBar();

            ImGui.End();
        }

        public static void StartMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.MenuItem("Open");
                    ImGui.MenuItem("Save");
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    ImGui.MenuItem("Copy");
                    ImGui.MenuItem("Paste");
                    ImGui.EndMenu();
                }

                ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                    ImGui.TextUnformatted("HI!");
                    ImGui.PopTextWrapPos();
                    ImGui.EndTooltip();
                }

                ImGui.EndMenuBar();
            }
        }
    }
}
