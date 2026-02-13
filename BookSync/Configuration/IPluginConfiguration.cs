namespace BookSync.Configuration;

public interface IPluginConfiguration
{
    public uint Interval { get; set; }
    public string ExcludedTags { get; set; }
    public bool DeleteReadCloudMedia { get; set; }
    public bool DeleteReadLocalMedia { get; set; }
    public bool DeleteAfterSync { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public string[] GetExcludedTags()
    {
        return ExcludedTags.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
    }
}