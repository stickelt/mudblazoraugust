using System;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FD.Shared.Utils
{
    public static class UrlUtils
    {
        /// <summary>
        /// Safely appends a "from" query string to a URL.
        /// Preserves fragments (#...), skips mailto:/tel:/javascript:, and avoids duplicate "from".
        /// Use for internal NG links only.
        /// </summary>
        public static string AppendFromUrl(string? url, string? from)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(from))
                return url ?? string.Empty;

            var u = url.TrimStart();

            // Skip unsafe schemes
            if (u.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase) ||
                u.StartsWith("mailto:", StringComparison.InvariantCultureIgnoreCase) ||
                u.StartsWith("tel:", StringComparison.InvariantCultureIgnoreCase))
                return url;

            // Handle fragment
            var hashIdx = u.IndexOf('#');
            var fragment = hashIdx >= 0 ? u.Substring(hashIdx) : string.Empty;
            var baseUrl  = hashIdx >= 0 ? u.Substring(0, hashIdx) : u;

            // Avoid duplicate "from"
            var qIdx = baseUrl.IndexOf('?');
            var query = qIdx >= 0 ? baseUrl.Substring(qIdx + 1) : string.Empty;
            if (!string.IsNullOrEmpty(query))
            {
                var hasFrom = query.Split('&').Any(kv =>
                    kv.StartsWith("from=", true, CultureInfo.InvariantCulture) ||
                    kv.StartsWith("%66%72%6f%6d=", true, CultureInfo.InvariantCulture));
                if (hasFrom) return url;
            }

            var encoded = HttpUtility.UrlEncode(from);
            var withFrom = qIdx < 0
                ? $"{baseUrl}?from={encoded}"
                : $"{baseUrl}&from={encoded}";

            return withFrom + fragment;
        }

        /// <summary>
        /// Builds a tracked URL for the Product Sidebar.
        /// Internal links: append "from" directly.
        /// External links: go through link-forward with original URL, "from", and product ID.
        /// </summary>
        public static string BuildTrackedHrefForSidebar(
            string rawUrl,
            string fromTag,
            int? drugId,
            Func<string, bool> isNgLink,
            Func<string, string, int?, string> constructLinkForward)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return rawUrl;

            if (isNgLink(rawUrl))
            {
                // Internal NG link
                return AppendFromUrl(rawUrl, fromTag);
            }

            // External link
            return constructLinkForward(rawUrl, fromTag, drugId);
        }
    }
}
