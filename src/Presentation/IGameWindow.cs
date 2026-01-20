using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace WipeoutRewrite
{
    /// <summary>
    /// Abstraction for game window to enable testing and mocking.
    /// This interface extracts the essential window capabilities without coupling to OpenTK's GameWindow.
    /// </summary>
    public interface IGameWindow : IDisposable
    {
        // Window properties
        Vector2i Size { get; }
        Vector2i ClientSize { get; }
        WindowState WindowState { get; set; }
        bool IsExiting { get; }

        // Input
        KeyboardState KeyboardState { get; }

        // Control
        void Close();
        void Run();
        void SwapBuffers();

        // Events (match OpenTK GameWindow signatures)
        event Action? Load;
        event Action<FrameEventArgs>? UpdateFrame;
        event Action<FrameEventArgs>? RenderFrame;
        event Action<ResizeEventArgs>? Resize;
        event Action? Unload;
    }

    /// <summary>
    /// Adapter that makes OpenTK's GameWindow compatible with IGameWindow interface.
    /// This allows Game class to work with both real GameWindow and test mocks.
    /// </summary>
    public abstract class GameWindowAdapter : IGameWindow
    {
        protected abstract class GameWindowBase : OpenTK.Windowing.Desktop.GameWindow
        {
            public GameWindowBase(
                OpenTK.Windowing.Desktop.GameWindowSettings gws,
                OpenTK.Windowing.Desktop.NativeWindowSettings nws)
                : base(gws, nws)
            {
            }
        }

        // GameWindow events wrapped as IGameWindow events
        public event Action? Load;
        public event Action<FrameEventArgs>? UpdateFrame;
        public event Action<FrameEventArgs>? RenderFrame;
        public event Action<ResizeEventArgs>? Resize;
        public event Action? Unload;

        // Abstract properties to be implemented by Game class
        public abstract Vector2i Size { get; }
        public abstract Vector2i ClientSize { get; }
        public abstract WindowState WindowState { get; set; }
        public abstract KeyboardState KeyboardState { get; }
        public abstract bool IsExiting { get; }

        // Abstract methods
        public abstract void Close();
        public abstract void Run();
        public abstract void SwapBuffers();
        public abstract void Dispose();

        // Protected methods for derived classes to raise events
        protected void RaiseLoadEvent() => Load?.Invoke();
        protected void RaiseUpdateFrameEvent(FrameEventArgs args) => UpdateFrame?.Invoke(args);
        protected void RaiseRenderFrameEvent(FrameEventArgs args) => RenderFrame?.Invoke(args);
        protected void RaiseResizeEvent(ResizeEventArgs args) => Resize?.Invoke(args);
        protected void RaiseUnloadEvent() => Unload?.Invoke();
    }
}
