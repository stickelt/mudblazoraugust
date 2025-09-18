using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MetaSearchApp.Pages;

public partial class ProductInfoDialog : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private int SelectedProductId { get; set; } = 123; // Default value for demo

    private async Task OpenDialog()
    {
        if (SelectedProductId <= 0)
        {
            Snackbar.Add("Please enter a valid Product ID", Severity.Warning);
            return;
        }

        var parameters = new DialogParameters<ProductInfoLinksDialog>
        {
            { x => x.ProductId, SelectedProductId }
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<ProductInfoLinksDialog>(
            $"Product Links - ID: {SelectedProductId}", 
            parameters, 
            options);

        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            Snackbar.Add($"Product {SelectedProductId} links updated successfully!", Severity.Success);
        }
    }
}
