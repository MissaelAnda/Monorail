﻿using ImGuiNET;
using Monorail.Renderer;
using Monorail.Util;
using OpenTK.Mathematics;
using ImVec2 = System.Numerics.Vector2;

namespace Monorail.Layers
{
    public static class Editor
    {
        public static Vector2 Viewport { get; internal set; } = Vector2.Zero;
        public static bool Enabled { get; internal set; } = false;
        public static string text = "";

        public static void Process()
        {
            Dockspace();

            //ImGui.ShowStyleEditor();

            // Hierarchy
            ImGui.Begin("Hierarchy");
            ImGui.Text($"Draw calls per frame: {RenderCommand.DrawCalls}");
            ImGui.End();

            // GameWindow
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new ImVec2(0, 0));
            ImGui.Begin("World Viewport");
            var viewPort = ImGui.GetContentRegionAvail();
            Viewport = viewPort.ToVector2();
            ImGui.Image(App.Framebuffer.Color, viewPort, new ImVec2(0, 1), new ImVec2(1, 0));
            ImGui.End();
            ImGui.PopStyleVar();
            

            // WorldEditor


            // Properties


            // Resource manager (File explorer)


            // Console

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

        public static void Dockspace()
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
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new ImVec2(0.0f, 0.0f));
            ImGui.Begin("EditorDockspace", window_flags);

            // Pop padding style
            ImGui.PopStyleVar();

            // Pop fullscreen style
            ImGui.PopStyleVar(2);

            // DockSpace
            uint dockspace_id = ImGui.GetID("EditorDockspace");
            ImGui.DockSpace(dockspace_id, new ImVec2(0.0f, 0.0f), dockspace_flags);

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
