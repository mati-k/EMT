namespace EMT.Models
{
    /// <summary>
    /// Paths picked on the configuration screen. Property names match the old paths.json so it can be imported.
    /// </summary>
    public class ConfigData
    {
        public string MissionFile { get; set; } = "";
        public string LocalisationFile { get; set; } = "";
        public string VanillaFolder { get; set; } = "";
        public string ModFolder { get; set; } = "";
    }
}
