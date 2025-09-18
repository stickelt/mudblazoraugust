using System.Net.Http.Json;
using MetaSearchApp.Models;

namespace MetaSearchApp.Services;

public sealed class ProductInfoLinksService(HttpClient http) : IProductInfoLinksService
{
    // Fallback in-memory storage for demo when API is not available
    private static readonly Dictionary<int, ProductInfoLinksDto> _storage = new();

    public async Task<ProductInfoLinksDto?> GetAsync(int id)
    {
        try
        {
            // Try API first
            return await http.GetFromJsonAsync<ProductInfoLinksDto>($"api/product-info-links/{id}");
        }
        catch
        {
            // Fallback to in-memory storage for demo
            if (_storage.TryGetValue(id, out var existing))
            {
                return existing;
            }

            // Create sample data for demo
            var sampleData = new ProductInfoLinksDto
            {
                ProductId = id.ToString(),
                PrescribingInformation = new(true, "https://example.com/prescribing-info"),
                PatientInformation = new(false, ""),
                MedicationGuide = new(true, "https://example.com/medication-guide"),
                InstructionsForUse = new(false, "")
            };

            _storage[id] = sampleData;
            return sampleData;
        }
    }

    public async Task SaveAsync(ProductInfoLinksDto dto)
    {
        try
        {
            // Try API first
            var resp = await http.PutAsJsonAsync($"api/product-info-links/{dto.ProductId}", dto);
            resp.EnsureSuccessStatusCode();
        }
        catch
        {
            // Fallback to in-memory storage for demo
            if (int.TryParse(dto.ProductId, out var productId))
            {
                _storage[productId] = dto;
            }
            await Task.Delay(500); // Simulate API delay
        }
    }
}
