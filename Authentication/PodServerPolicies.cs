namespace BTCPayServer.Plugins.PodServer.Authentication;

public class PodServerPolicies
{
    public const string CanView = "btcpay.plugin.podserver.canview";
    public const string CanManageEpisodes = "btcpay.plugin.podserver.canmanageepisodes";
    public const string CanManagePodcast = "btcpay.plugin.podserver.canmanagepodcast";

    public static IEnumerable<string> AllPolicies
    {
        get
        {
            yield return CanView;
            yield return CanManageEpisodes;
            yield return CanManagePodcast;
        }
    }
}
