using HtmlAgilityPack;

namespace Template.Web.Utils
{
    public static class HtmlUtils
    {
        /// <summary>
        /// Rimuove tutti i tag HTML da una stringa e restituisce solo il testo.
        /// </summary>
        public static string StripHtml(string html, int? maxLength = null)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var text = doc.DocumentNode.InnerText.Trim();
            
            if (maxLength.HasValue)
            {
                return text.Length > maxLength.Value ? text[..maxLength.Value] : text;
            }

            return text;
        }
    }
}
