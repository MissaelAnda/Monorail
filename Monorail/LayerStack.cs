using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Monorail
{
    public struct EventList
    {
        public Action<MouseMoveEventArgs> MouseMove;
        public Action<MouseButtonEventArgs> MouseUp;
        public Action<MouseButtonEventArgs> MouseDown;
        public Action MouseEnter;
        public Action MouseLeave;
        public Action<MonitorEventArgs> MonitorConnected;
        public Action<KeyboardKeyEventArgs> KeyUp;
        public Action<TextInputEventArgs> TextInput;
        public Action<FocusedChangedEventArgs> FocusedChanged;
        public Action<JoystickEventArgs> JoystickConnected;
        public Action<MinimizedEventArgs> Minimized;
        public Action Closed;
        public Action<CancelEventArgs> Closing;
        public Action Refresh;
        public Action<ResizeEventArgs> Resize;
        public Action<WindowPositionEventArgs> Move;
        public Action<KeyboardKeyEventArgs> KeyDown;
        public Action<MaximizedEventArgs> Maximized;
        public Action<FileDropEventArgs> FileDrop;
        public Action<MouseWheelEventArgs> MouseWheel;
    }

    public class LayerStack
    {
        List<Layer> _layerStack = new List<Layer>();
        Dictionary<Layer, EventList> _eventManager = new Dictionary<Layer, EventList>();
        int _layerInsert = 0;
        GameWindow _gameWindow;

        public LayerStack(GameWindow gameWindow) => _gameWindow = gameWindow;

        public void PushLayer(Layer layer)
        {
            _layerStack.Insert(_layerInsert, layer);
            _layerInsert++;

            layer.OnAttached();
            RegisterEvents(layer);
        }

        public void PushOverlay(Layer overlay)
        {
            _layerStack.Add(overlay);
            overlay.OnAttached();
            RegisterEvents(overlay);
        }

        public void PopLayer(Layer layer)
        {
            var removed = _layerStack.Remove(layer);
            if (removed)
            {
                _layerInsert--;
                layer.OnDetached();

                DeregisterEvents(layer);
            }
        }

        public void PopOverlay(Layer overlay)
        {
            var removed = _layerStack.Remove(overlay);
            if (removed)
            {
                overlay.OnDetached();
                DeregisterEvents(overlay);
            }
        }

        public void Update(double delta)
        {
            for (int i = 0; i < _layerStack.Count; i++)
                _layerStack[i].OnUpdate(delta);
        }

        public void Render(double delta)
        {
            for (int i = 0; i < _layerStack.Count; i++)
                _layerStack[i].OnRender(delta);
        }

        public void RegisterEvents(Layer layer)
        {
            var actions = new EventList();
            layer.OnRegisterEvents(ref actions);
            _eventManager.Add(layer, actions);

            _gameWindow.MouseMove += actions.MouseMove;
            _gameWindow.MouseUp += actions.MouseUp;
            _gameWindow.MouseDown += actions.MouseDown;
            _gameWindow.MouseEnter += actions.MouseEnter;
            _gameWindow.MouseLeave += actions.MouseLeave;
            _gameWindow.MonitorConnected += actions.MonitorConnected;
            _gameWindow.KeyUp += actions.KeyUp;
            _gameWindow.TextInput += actions.TextInput;
            _gameWindow.FocusedChanged += actions.FocusedChanged;
            _gameWindow.JoystickConnected += actions.JoystickConnected;
            _gameWindow.Minimized += actions.Minimized;
            _gameWindow.Closed += actions.Closed;
            _gameWindow.Closing += actions.Closing;
            _gameWindow.Refresh += actions.Refresh;
            _gameWindow.Resize += actions.Resize;
            _gameWindow.Move += actions.Move;
            _gameWindow.KeyDown += actions.KeyDown;
            _gameWindow.Maximized += actions.Maximized;
            _gameWindow.FileDrop += actions.FileDrop;
            _gameWindow.MouseWheel += actions.MouseWheel;
        }

        public void DeregisterEvents(Layer layer)
        {
            if (!_eventManager.TryGetValue(layer, out var actions))
            {
                return;
            }

            _gameWindow.MouseMove -= actions.MouseMove;
            _gameWindow.MouseUp -= actions.MouseUp;
            _gameWindow.MouseDown -= actions.MouseDown;
            _gameWindow.MouseEnter -= actions.MouseEnter;
            _gameWindow.MouseLeave -= actions.MouseLeave;
            _gameWindow.MonitorConnected -= actions.MonitorConnected;
            _gameWindow.KeyUp -= actions.KeyUp;
            _gameWindow.TextInput -= actions.TextInput;
            _gameWindow.FocusedChanged -= actions.FocusedChanged;
            _gameWindow.JoystickConnected -= actions.JoystickConnected;
            _gameWindow.Minimized -= actions.Minimized;
            _gameWindow.Closed -= actions.Closed;
            _gameWindow.Closing -= actions.Closing;
            _gameWindow.Refresh -= actions.Refresh;
            _gameWindow.Resize -= actions.Resize;
            _gameWindow.Move -= actions.Move;
            _gameWindow.KeyDown -= actions.KeyDown;
            _gameWindow.Maximized -= actions.Maximized;
            _gameWindow.FileDrop -= actions.FileDrop;
            _gameWindow.MouseWheel -= actions.MouseWheel;

            _eventManager.Remove(layer);
        }
    }
}
