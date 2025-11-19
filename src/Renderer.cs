using OpenTK.Graphics.OpenGL4;

namespace WipeoutRewrite
{
    public static class Renderer
    {
        public static void Init()
        {
            // Render resource initialization (shaders, buffers, etc.)
        }

        public static void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void Present()
        {
            // SwapBuffers is called by GameWindow; we can do flushing here if needed
        }
    }
}
