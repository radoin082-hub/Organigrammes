namespace Compta.Ledger.Core.orgTestapp.Helpers;

public interface IGuidProvider
{
    Guid NewV4();
    Guid NewV7();

    Guid Parse(string value);
    bool TryParse(string? value, out Guid guid);

    Guid Empty { get; }
}

public sealed class GuidProvider : IGuidProvider
{
    public Guid Empty => Guid.Empty;

    public Guid NewV4() => Guid.NewGuid();

    public Guid NewV7() => Guid.CreateVersion7();

    public Guid Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("GUID string cannot be null or empty.", nameof(value));

        return Guid.Parse(value);
    }

    public bool TryParse(string? value, out Guid guid)
        => Guid.TryParse(value, out guid);
}

