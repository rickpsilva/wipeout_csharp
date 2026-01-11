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
        /// Renders a 3D object from a specific category.
        /// </summary>
        /// <typeparam name="T">Marker type representing the category (CategoryShip, CategoryMsDos, CategoryTeams, etc.)</typeparam>
        /// <param name="categoryIndex">Index of the object within the category (0-based)</param>
        /// <example>
        /// _contentPreview3D.Render&lt;CategoryShip&gt;(0);     // Render first ship (FEISAR)
        /// _contentPreview3D.Render&lt;CategoryMsDos&gt;(1);    // Render second MsDos object
        /// _contentPreview3D.Render&lt;CategoryTeams&gt;(3);    // Render fourth team logo
        /// </example>
        void Render<T>(int categoryIndex);
        
        /// <summary>
        /// Renders a 3D object from a specific category with optional custom scale.
        /// </summary>
        /// <typeparam name="T">Marker type representing the category</typeparam>
        /// <param name="categoryIndex">Index of the object within the category (0-based)</param>
        /// <param name="customScale">Custom uniform scale for the object (optional)</param>
        void Render<T>(int categoryIndex, float? customScale);
        
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