using System.Text.RegularExpressions;

namespace EduCore.Helpers
{
    public enum VideoKind { Iframe, File, Link }

    public record VideoEmbed(VideoKind Kind, string Url);

    /// <summary>
    /// Turns a pasted video URL into something embeddable:
    /// YouTube / Vimeo => an iframe embed URL, a direct video file => a &lt;video&gt; source,
    /// anything else => a plain link.
    /// </summary>
    public static class VideoEmbedHelper
    {
        public static VideoEmbed Resolve(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new VideoEmbed(VideoKind.Link, "#");

            url = url.Trim();

            var youTubeId = Match(url, @"(?:youtube\.com/(?:watch\?v=|embed/|shorts/)|youtu\.be/)([\w-]{11})");
            if (youTubeId != null)
                // nocookie + modestbranding + rel=0 minimises YouTube branding and related/share overlays
                return new VideoEmbed(VideoKind.Iframe,
                    $"https://www.youtube-nocookie.com/embed/{youTubeId}?rel=0&modestbranding=1");

            var vimeoId = Match(url, @"vimeo\.com/(?:video/)?(\d+)");
            if (vimeoId != null)
                // title/byline/portrait off removes the Vimeo title and share/author overlays
                return new VideoEmbed(VideoKind.Iframe,
                    $"https://player.vimeo.com/video/{vimeoId}?title=0&byline=0&portrait=0");

            if (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                url.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
                url.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                return new VideoEmbed(VideoKind.File, url);

            return new VideoEmbed(VideoKind.Link, url);
        }

        private static string? Match(string input, string pattern)
        {
            var m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
