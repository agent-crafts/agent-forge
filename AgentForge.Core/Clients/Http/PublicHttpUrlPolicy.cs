using System.Net;

namespace AgentForge.Core.Clients.Http;

/// <summary>Restricts configurable HTTP tools to public HTTPS destinations.</summary>
public static class PublicHttpUrlPolicy
{
    public static bool IsAllowed(Uri? url)
    {
        if (url is null || !url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps ||
            url.UserInfo.Length != 0 || url.Port != 443 || string.IsNullOrWhiteSpace(url.Host))
            return false;

        var host = url.IdnHost.TrimEnd('.');
        if (host.Length == 0 || !host.Contains('.') ||
            host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            new[] { ".local", ".internal", ".test", ".invalid", ".example" }
                .Any(suffix => host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            return false;

        return !IPAddress.TryParse(host, out var address) || IsPublicAddress(address);
    }

    public static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var bytes = address.GetAddressBytes();
        if (bytes.Length == 4)
        {
            var a = bytes[0];
            var b = bytes[1];
            var c = bytes[2];
            return a is not (0 or 10 or 127) &&
                   !(a == 100 && b is >= 64 and <= 127) &&
                   !(a == 169 && b == 254) &&
                   !(a == 172 && b is >= 16 and <= 31) &&
                   !(a == 192 && (b == 168 || b == 0 && c is 0 or 2 || b == 88 && c == 99)) &&
                   !(a == 198 && (b is 18 or 19 || b == 51 && c == 100)) &&
                   !(a == 203 && b == 0 && c == 113) &&
                   a < 224;
        }

        // Global unicast IPv6 only, excluding documentation prefix 2001:db8::/32.
        return bytes.Length == 16 && (bytes[0] & 0xE0) == 0x20 &&
               !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8);
    }
}
