# Monorail
A simple 2D and 3D cross-platform game-engine made with DotNet Core using OpenGL 4.

This is more of a learning project for computer graphics aiming to do the bare minimum needed for a game engine, nothing fancy here :P

## Roadmap
- [x] 2D batch rendering
    - [X] Camera frustum culling
- [ ] Canvas for UI with editor
    - [ ] TTF loading
- [ ] Advanced 3D rendering
    - [ ] Forward clustered Rendering
    - [ ] PBR
    - [ ] Cascade Shadow Mapping
    - [ ] PCSS shadows
    - [ ] Basic global ilumination
    - [ ] User defined shaders (Custom GLSL)
    - [ ] SSAO
    - [ ] SSR
    - [ ] Procedural Environment
    - [ ] Skybox/IBL
    - [ ] Volumetric light
    - [ ] Volumetric Clouds
    - [ ] Bloom
    - [ ] Camera frustum culling
- [ ] 2D/3D Scene editor
    - [ ] Gizmos
- [ ] Basic 2D/3D physics integration
- [X] Entity-Component Scene
- [ ] Scripting functionality

## Dependencies
- .Net Core 5.0
- The core of this engine is [OpenTK 4](https://github.com/opentk/opentk)
- For console logging: [Colorful Console](https://github.com/tomakita/Colorful.Console)
- For GUI: [ImGui.NET](https://github.com/mellinoe/imgui.net)

## Inspiration
- This game engine started as my own version of [Hazel](https://github.com/TheCherno/Hazel)
- Also heavily inspired by the [Monogame Framework](https://www.monogame.net/) and [Nez](https://github.com/prime31/nez)
- The usage design is inspired by [Godot](https://godotengine.org/) and [Unity](https://unity.com/)
