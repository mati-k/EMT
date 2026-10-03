namespace EMT.Models
{
    /// <summary>
    /// Sprite defined in .gfx files. Animated ones (frameAnimatedSpriteType) have all frames side by side in one texture.
    /// </summary>
    public record GfxSprite(string Name, string Path, int Frames = 1, double Fps = 0, double PauseOnLoop = 0)
    {
        public bool IsAnimated => Frames > 1 && Fps > 0;
    }
}
