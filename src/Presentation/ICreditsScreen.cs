namespace WipeoutRewrite.Presentation
{
    /// <summary>
    /// Represents the credits screen of the game.
    /// </summary>
    public interface ICreditsScreen
    {
        void Reset();

        void Update(float deltaTime);


        void Render(int screenWidth, int screenHeight);

        
    }
}