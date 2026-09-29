namespace DancingBallOnBeats.Core
{
    /// <summary>
    /// Tiny static bridge for passing the player's song choice from the Main Menu scene into
    /// the Gameplay scene without needing DontDestroyOnLoad managers or a full save-file round
    /// trip. Reset only by choosing a new song from Song Select.
    /// </summary>
    public static class GameSession
    {
        public static string SelectedSongId = "real_backbeat";
    }
}
