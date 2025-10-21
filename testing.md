1) Add this to ProductSideBarComponent.razor.cs (near your other fields)
#if DEBUG
using Microsoft.AspNetCore.WebUtilities;
#endif

// …

#if DEBUG
private bool _showHarness;
private List<HarnessRow> _harnessRows = new();

private sealed record HarnessCase(string Title, string RawUrl, bool? Show);
private sealed record HarnessRow(string Title, string RawUrl, bool? Show, string Classification, string Href);

// Call this when ?harness=1 is present
private void RunSidebarLinkHarness()
{
    _harnessRows.Clear();

    // Set up scenarios you want to verify
    var fromTag = ProductSidebarSuffixTrackingFrom ?? "ProductSidebar";
    var productId = (int?)ForeignId ?? null; // or drug?.DrugId after you fetch it

    var currentHost = new Uri(NavigationManager.BaseUri).Host;

    var cases = new List<HarnessCase>
    {
        // INTERNAL (relative)
        new("PI (internal relative)", "/package-inserts?id=123", true),
        new("PI (internal relative no query)", "/package-inserts", true),

        // INTERNAL (absolute, same host)
        new("Patient Info (internal absolute)", $"https://{currentHost}/package-inserts?id=456", true),

        // EXTERNAL
        new("Med Guide (external)", "https://www.merck.com/product/usa/pi_circulars/x/xanthingo_pi.pdf", true),
        new("IFU (external q+fragment)", "https://www.genetechbiopharma.com/docs/ifu.pdf?ver=2#page=3", true),

        // Existing from=
        new("Internal with existing from", "/pi?from=OLD", true),

        // Unsafe schemes (should pass through unchanged)
        new("mailto", "mailto:help@fd.com", true),
        new("tel", "tel:+18005551212", true),
        new("javascript", "javascript:alert('x')", true),

        // Nullable show cases
        new("Show = null (hidden)", "/package-inserts?id=999", null),
        new("Show = false (hidden)", "/package-inserts?id=1000", false),
    };

    foreach (var c in cases)
    {
        // Mimic your Add(...) gate
        if (c.Show != true || string.IsNullOrWhiteSpace(c.RawUrl))
        {
            _harnessRows.Add(new HarnessRow(c.Title, c.RawUrl, c.Show, "Hidden (show!=true)", "(n/a)"));
            continue;
        }

        // Use the same helper your component uses
        var href = TextUtils.BuildTrackedHrefForSidebar(
            rawUrl: c.RawUrl,
            fromTag: fromTag,
            drugId: productId,
            currentHost: currentHost
        );

        var classification = href.StartsWith("/link-forward", StringComparison.OrdinalIgnoreCase)
            ? "EXTERNAL → forward"
            : "INTERNAL (from appended or passthrough)";

        _harnessRows.Add(new HarnessRow(c.Title, c.RawUrl, c.Show, classification, href));
    }
}
#endif


2) In OnParametersSetAsync(), enable the harness when ?harness=1
Find the top of your method and add:
#if DEBUG
// Show harness when query contains ?harness=1
var uri = new Uri(NavigationManager.Uri);
var q = QueryHelpers.ParseQuery(uri.Query);
_showHarness = q.TryGetValue("harness", out var hv) && hv.ToString() == "1";
#endif

After you fetch drug (so ForeignId etc. are available), add:
#if DEBUG
if (_showHarness)
{
    RunSidebarLinkHarness();
    // optional: return here if you want to skip normal rendering during harness runs
    // return;
}
#endif


3) Add this Razor markup in ProductSideBarComponent.razor (anywhere reasonable)
@* DEBUG harness UI *@
#if DEBUG
@if (_showHarness)
{
    <div class="ng-card" style="margin-bottom:1rem;">
        <div class="card-content">
            <h4>Sidebar Link Harness (DEV)</h4>
            <p>BaseUri Host: @new Uri(NavigationManager.BaseUri).Host</p>
            <table class="table" style="width:100%; font-size:0.95rem;">
                <thead>
                    <tr>
                        <th style="text-align:left;">Title</th>
                        <th style="text-align:left;">RawUrl</th>
                        <th>Show</th>
                        <th style="text-align:left;">Classification</th>
                        <th style="text-align:left;">Rendered Href</th>
                    </tr>
                </thead>
                <tbody>
                @foreach (var r in _harnessRows)
                {
                    <tr>
                        <td>@r.Title</td>
                        <td>@r.RawUrl</td>
                        <td>@(r.Show?.ToString() ?? "null")</td>
                        <td>@r.Classification</td>
                        <td><a href="@r.Href" target="_blank">@r.Href</a></td>
                    </tr>
                }
                </tbody>
            </table>
            <p style="margin-top:0.5rem;color:#666;">
                Tip: change <code>currentHost</code> by running locally (localhost) vs DEV; edit cases inside <code>RunSidebarLinkHarness()</code>.
            </p>
        </div>
    </div>
}
#endif


How to use it
Run the app locally, navigate to a product page with this component, and add ?harness=1 to the URL:
https://localhost:5001/product/123?tab=details&harness=1

You'll see a table listing each scenario:
RawUrl (what Admin would enter)
Show (true/false/null)
Classification (Internal vs External→Forward)
Rendered Href (the final anchor the user will click)
Click the rendered hrefs to verify:
Internal → goes straight to the internal URL with from=....
External → first hits /link-forward?... then redirects.
To simulate different environments, keep the currentHost driven from NavigationManager.BaseUri. On localhost, absolute DEV URLs will be treated as external (correct), while relative URLs are still internal (also correct). On DEV, absolute links to the same host are internal.

FAQ
Where does /link-forward come from?
From your app's route/constant you found. The harness and helpers assume that's the forward endpoint that logs and 302-redirects.
What if Admin accidentally pastes a /link-forward?... link as the RawUrl?
The harness will show it as internal relative and append another from. If you'd rather detect and leave it alone, add a guard in BuildTrackedHrefForSidebar:
if (rawUrl.StartsWith("/link-forward", StringComparison.OrdinalIgnoreCase))
    return rawUrl;

Can I expand scenarios?
Absolutely—just add more HarnessCase entries: different Show (null/false/true), existing from=, more externals, etc.

This gives you peace-of-mind testing without adding a test project, and you can quickly screenshot the table for your team/QA. When you're done, just omit ?harness=1 (or wrap the whole block in a feature flag).
