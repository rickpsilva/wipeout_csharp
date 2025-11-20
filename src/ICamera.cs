using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WipeoutRewrite
{
    public interface ICamera
    {
        // Matrizes de transformação
        Matrix4 GetViewMatrix();
        Matrix4 GetProjectionMatrix();
        
        void SetAspectRatio(float aspectRatio);

        // Propriedades de câmera
        Vector3 Position { get; }
        Vector3 Target { get; }
        float Fov { get; set; }
        float Yaw { get; }
        float Pitch { get; }
        
        // Atualização com input
        void Update(KeyboardState keyboardState, MouseState mouseState);
        
        // Controles de câmera
        void Rotate(float yaw, float pitch);
        void Move(Vector3 direction);
        void Zoom(float delta);
        void ResetView();
        void SetIsometricMode(bool useIsometric, float scale = 1.0f);
    }
}   