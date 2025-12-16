namespace WipeoutRewrite.Presentation
{
    /// <summary>
    /// Interface for previewing 3D content (ships, other 3d objects) in the UI.
    /// </summary>
    public interface IContentPreview3D
    {
        /// <summary>
        /// Sets the 3D model to preview.
        /// </summary>
        void SetModel(int modelId);

        /// <summary>
        /// Renders the 3D content preview.
        /// </summary>
        void Render<T>(int modelId);
        
        /// <summary>
        /// Configura a posição da nave no preview 3D (X, Y, Z)
        /// </summary>
        void SetShipPosition(float x, float y, float z);
        
        /// <summary>
        /// Configura o offset da câmera relativo à nave
        /// </summary>
        void SetCameraOffset(float x, float y, float z);
        
        /// <summary>
        /// Configura a velocidade de rotação
        /// </summary>
        void SetRotationSpeed(float speed);
    }
}