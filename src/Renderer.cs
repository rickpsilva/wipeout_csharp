using OpenTK.Graphics.OpenGL4;

namespace WipeoutRewrite
{
    public static class Renderer
    {
        public static void Init()
        {
            // Inicialização de recursos de render (shaders, buffers, etc.)
        }

        public static void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void Present()
        {
            // SwapBuffers é chamado pelo GameWindow; aqui podemos fazer flushing se necessário
        }
    }
}
