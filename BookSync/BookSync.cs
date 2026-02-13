using BookSync.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace BookSync;

public class BookSync : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private static BookSync _instance = null!;

    public BookSync(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) :
        base(applicationPaths, xmlSerializer)
    {
        _instance = this;
    }

    public static IPluginConfiguration PluginConfiguration => _instance.Configuration;

    public override string Name => Resources.PluginName;
    public override Guid Id => Guid.Parse(Resources.PluginId);

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Resources.PluginName,
                EmbeddedResourcePath = $"{GetType().Namespace}.Web.configPage.html"
            }
        ];
    }
}