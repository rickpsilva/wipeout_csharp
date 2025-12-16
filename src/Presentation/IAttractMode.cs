using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Presentation
{
    public interface IAttractMode
    {
        void Start();
        void Stop();
        void Render(IRenderer renderer);
        void Update(float time);

        bool IsActive { get; }
    }
}