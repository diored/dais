namespace Dais.Core.OAuth;

public class AuthenticateHeader
{
    private readonly Dictionary<string, string> _parts = [];

    public AuthenticateHeader Add(string property, string value)
    {
        _parts[property] = value;

        return this;
    }

    public AuthenticateHeader AddIfNotEmpty(string property, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _parts[property] = value;
        }

        return this;
    }

    public override string ToString()
    {
        string[] parts = [.. _parts.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\"")];

        return string.Join(", ", parts);
    }
}