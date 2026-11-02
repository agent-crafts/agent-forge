using System.Text.Json;
using System.Text.RegularExpressions;
using AgentForge.Core.Clients.Http;

namespace AgentForge.Application.Tools.Registry.Http;

/// <summary>Resolves configured URL path placeholders using validated scalar inputs.</summary>
public static partial class HttpUrlTemplate
{
    [GeneratedRegex(@"\{([a-z][a-z0-9_]*)\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderPattern();

    public static bool TryResolve(string? template, JsonElement input, out Uri? url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(template) || input.ValueKind != JsonValueKind.Object)
            return false;

        // Validate the unexpanded destination and ensure placeholders only appear in the path.
        var sample = PlaceholderPattern().Replace(template, "1");
        if (!Uri.TryCreate(sample, UriKind.Absolute, out var baseUrl) ||
            !PublicHttpUrlPolicy.IsAllowed(baseUrl))
            return false;
        var authorityEnd = template.IndexOf('/', template.IndexOf("://", StringComparison.Ordinal) + 3);
        if (authorityEnd < 0) authorityEnd = template.Length;
        var queryStart = template.IndexOfAny(['?', '#'], authorityEnd);
        if (queryStart < 0) queryStart = template.Length;
        if (PlaceholderPattern().IsMatch(template[..authorityEnd]) ||
            PlaceholderPattern().IsMatch(template[queryStart..]) ||
            template[..queryStart].Contains('{') && !PlaceholderPattern().Replace(template[..queryStart], "").Equals(template[..queryStart].Replace("{", "").Replace("}", ""), StringComparison.Ordinal))
            return false;

        var names = PlaceholderPattern().Matches(template).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        if (input.EnumerateObject().Any(p => !names.Contains(p.Name)) ||
            names.Any(n => !input.TryGetProperty(n, out _)))
            return false;

        var resolved = PlaceholderPattern().Replace(template, match =>
        {
            var value = input.GetProperty(match.Groups[1].Value);
            return value.ValueKind switch
            {
                JsonValueKind.String => Uri.EscapeDataString(value.GetString()!),
                JsonValueKind.Number => Uri.EscapeDataString(value.GetRawText()),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => ""
            };
        });
        if (names.Any(n => input.GetProperty(n).ValueKind is not
            (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)))
            return false;
        return Uri.TryCreate(resolved, UriKind.Absolute, out url) && PublicHttpUrlPolicy.IsAllowed(url);
    }
}
