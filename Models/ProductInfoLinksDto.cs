using System.ComponentModel.DataAnnotations;

namespace MetaSearchApp.Models;

public record LinkItem(bool Enabled, string? Url);

public record ProductInfoLinksDto
{
    public string ProductId { get; set; } = default!;
    public LinkItem PrescribingInformation { get; set; } = new(false, null);
    public LinkItem PatientInformation { get; set; } = new(false, null);
    public LinkItem MedicationGuide { get; set; } = new(false, null);
    public LinkItem InstructionsForUse { get; set; } = new(false, null);
}

public static class UrlRules
{
    public static bool IsHttpOrHttps(string? url)
        => !string.IsNullOrWhiteSpace(url) &&
           Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u) &&
           (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}
