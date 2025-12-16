namespace WipeoutRewrite.Presentation
{
    /// <summary>
    /// Represents the title screen of the game.
    /// </summary>
    public interface ITitleScreen
    {
        void Initialize();
        
        void Reset();

        void Update(float deltaTime, out bool shouldStartAttract, out bool shouldStartMenu);

        void OnAttractComplete();

        void Render(int screenWidth, int screenHeight);

        void OnStartPressed();
    }
}