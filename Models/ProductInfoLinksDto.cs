using System.ComponentModel.DataAnnotations;

namespace MetaSearchApp.Models;

public sealed class ProductInfoLinksDto
{
    public int ProductId { get; set; }

    [MaxLength(500)] public string? PrescribingUrl { get; set; }
    public bool PrescribingEnabled { get; set; }

    [MaxLength(500)] public string? PatientInfoUrl { get; set; }
    public bool PatientInfoEnabled { get; set; }

    [MaxLength(500)] public string? MedicationGuideUrl { get; set; }
    public bool MedicationGuideEnabled { get; set; }

    [MaxLength(500)] public string? InstructionsForUseUrl { get; set; }
    public bool InstructionsForUseEnabled { get; set; }
}
