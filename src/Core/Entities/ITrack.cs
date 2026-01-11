using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// Interface for track entities.
    /// </summary>
    public interface ITrack
    {
        /// <summary>
        /// Gets or sets the name of the track.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets the list of sections that make up the track.
        /// </summary>
        public List<TrackSection> Sections { get; }

        /// <summary>
        /// Gets or sets the name of the track. 
        /// </summary>
        /// <param name="data"></param>
        void LoadFromBinary(byte[] data);

        /// <summary>
        /// Renders the track using the provided OpenGL renderer.
        /// </summary>
        /// <param name="renderer"></param>
        void Render(GLRenderer renderer);

    }
}
   