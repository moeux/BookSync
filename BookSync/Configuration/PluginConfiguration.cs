using MediaBrowser.Model.Plugins;

namespace BookSync.Configuration;

public class PluginConfiguration : BasePluginConfiguration, IPluginConfiguration
{
    public uint Interval { get; set; } = 1;
    public string ExcludedTags { get; set; } = string.Empty;
    public bool DeleteReadCloudMedia { get; set; } = false;
    public bool DeleteReadLocalMedia { get; set; } = false;
    public bool DeleteAfterSync { get; set; } = false;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ClientId { get; set; } = "qNAx1RDb";
    public string ClientSecret { get; set; } = "K3YYSjCgDJNoWKdGVOyO1mrROp3MMZqqRNXNXTmh";
}