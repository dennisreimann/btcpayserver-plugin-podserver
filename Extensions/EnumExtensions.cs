using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BTCPayServer.Plugins.PodServer.Extensions;

public static class EnumExtensions
{
    public static string DisplayName(this Type enumType, string input) =>
        enumType.GetMember(input).First().GetCustomAttribute<DisplayAttribute>()?.Name ?? input;
}
