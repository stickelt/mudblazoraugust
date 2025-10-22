@inherits ProductSideBarComponentBase

@if (Loading)
{
    <div class="sidebar-loading">
        <p>Loading product links...</p>
    </div>
}
else if (Items is not null && Items.Any())
{
    <div class="product-sidebar">
        <h4>Product Resources</h4>
        <ul>
            @foreach (var link in Items)
            {
                <li>
                    <a href="@link.Href"
                       target="_blank"
                       rel="noopener noreferrer">
                        @link.Title
                        @if (link.Href.Contains("link-forward", StringComparison.OrdinalIgnoreCase))
                        {
                            <span class="external-link">↗</span>
                        }
                    </a>
                </li>
            }
        </ul>
    </div>
}
else
{
    <div class="sidebar-empty">
        <p>No product links available.</p>
    </div>
}

@code {
    // optional inline CSS – you can remove this and move to a CSS file if preferred
    private string ExternalLinkStyle => @"
        .product-sidebar { padding: 0.75rem 1rem; background: #fafafa; border-radius: 8px; }
        .product-sidebar h4 { font-size: 1.1rem; font-weight: 600; margin-bottom: 0.5rem; }
        .product-sidebar ul { list-style: none; padding: 0; margin: 0; }
        .product-sidebar li { margin: 0.25rem 0; }
        .product-sidebar a { color: #005fa3; text-decoration: none; }
        .product-sidebar a:hover { text-decoration: underline; }
        .external-link { margin-left: 4px; font-size: 0.85rem; color: #777; }
        .sidebar-loading, .sidebar-empty { color: #666; font-style: italic; }
    ";
}
