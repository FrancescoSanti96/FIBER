using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Template.Web.Services
{
    public interface IPdfService
    {
        byte[] GenerateBulletinPdf(string title, string content, string author, DateTime? publishDate, DateOnly? expireDate, List<string> provinces, List<string> coltures);
    }

    public class PdfService : IPdfService
    {
        public byte[] GenerateBulletinPdf(string title, string content, string author, DateTime? publishDate, DateOnly? expireDate, List<string> provinces, List<string> coltures)
        {
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().PaddingBottom(20).Column(headerCol =>
                    {
                        headerCol.Item().Text(title).FontSize(22).Bold();
                        headerCol.Item().PaddingTop(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    });
                    
                    page.Content().Column(col =>
                    {
                        // Metadati con spaziatura migliorata
                        col.Item().PaddingVertical(12).Row(row =>
                        {
                            row.RelativeItem().Column(metaCol =>
                            {
                                metaCol.Item().Text($"Autore: {author}").FontSize(11).FontColor(Colors.Grey.Darken1);
                                if (publishDate.HasValue)
                                    metaCol.Item().Text($"Data pubblicazione: {publishDate.Value:dd/MM/yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
                                if (expireDate.HasValue)
                                    metaCol.Item().Text($"Data scadenza: {expireDate.Value:dd/MM/yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
                            });
                            
                            if (provinces.Count > 0 || coltures.Count > 0)
                            {
                                row.RelativeItem().Column(tagCol =>
                                {
                                    if (provinces.Count > 0)
                                        tagCol.Item().Text($"Province: {string.Join(", ", provinces)}").FontSize(11).FontColor(Colors.Blue.Darken1);
                                    if (coltures.Count > 0)
                                        tagCol.Item().Text($"Colture: {string.Join(", ", coltures)}").FontSize(11).FontColor(Colors.Green.Darken1);
                                });
                            }
                        });
                        
                        col.Item().PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        
                        RenderHtmlToPdf(col, content);
                    });
                    
                    page.Footer().AlignCenter().Text($"Generato il {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            return pdf.GeneratePdf();
        }

        // Main HTML to PDF renderer
        private void RenderHtmlToPdf(ColumnDescriptor col, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                col.Item().Text("Nessun contenuto disponibile");
                return;
            }

            // Decodifica le entità HTML più comuni
            htmlContent = htmlContent
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'");

            // Sostituisci <br> con newline
            htmlContent = Regex.Replace(htmlContent, "<br ?/?>", "\n", RegexOptions.IgnoreCase);

            // Parser HTML semplice per blocchi principali
            var blockRegex = new Regex(@"(<h1>.*?</h1>|<h2>.*?</h2>|<h3>.*?</h3>|<ol>.*?</ol>|<ul>.*?</ul>|<p>.*?</p>|<li>.*?</li>)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var blocks = blockRegex.Split(htmlContent).Where(b => !string.IsNullOrWhiteSpace(b)).ToList();
            int olCounter = 1;
            
            foreach (var block in blocks)
            {
                if (Regex.IsMatch(block, "^<h1>", RegexOptions.IgnoreCase))
                {
                    var text = StripTag(block, "h1");
                    col.Item().PaddingTop(10).PaddingBottom(8).Text(t => t.Span(text).FontSize(20).Bold());
                }
                else if (Regex.IsMatch(block, "^<h2>", RegexOptions.IgnoreCase))
                {
                    var text = StripTag(block, "h2");
                    col.Item().PaddingTop(8).PaddingBottom(6).Text(t => t.Span(text).FontSize(16).Bold());
                }
                else if (Regex.IsMatch(block, "^<h3>", RegexOptions.IgnoreCase))
                {
                    var text = StripTag(block, "h3");
                    col.Item().PaddingTop(6).PaddingBottom(4).Text(t => t.Span(text).FontSize(14).Bold());
                }
                else if (Regex.IsMatch(block, "^<ol>", RegexOptions.IgnoreCase))
                {
                    olCounter = 1;
                    
                    // Spaziatura prima della lista
                    col.Item().PaddingTop(4);
                    
                    var items = Regex.Matches(block, "<li>(.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                        .Select(m => m.Groups[1].Value.Trim()).ToList();
                    foreach (var item in items)
                    {
                        col.Item().PaddingVertical(2).Row(row =>
                        {
                            row.ConstantItem(18).Text($"{olCounter}.").FontSize(12);
                            row.RelativeItem().Element(e => RenderFormattedLine(e, item));
                        });
                        olCounter++;
                    }
                    
                    // Spaziatura dopo la lista  
                    col.Item().PaddingBottom(4);
                }
                else if (Regex.IsMatch(block, "^<ul>", RegexOptions.IgnoreCase))
                {
                    // Spaziatura prima della lista
                    col.Item().PaddingTop(4);
                    
                    var items = Regex.Matches(block, "<li>(.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                        .Select(m => m.Groups[1].Value.Trim()).ToList();
                    foreach (var item in items)
                    {
                        col.Item().PaddingVertical(2).Row(row =>
                        {
                            row.ConstantItem(12).Text("•").FontSize(12);
                            row.RelativeItem().Element(e => RenderFormattedLine(e, item));
                        });
                    }
                    
                    // Spaziatura dopo la lista
                    col.Item().PaddingBottom(4);
                }
                else if (Regex.IsMatch(block, "^<li>", RegexOptions.IgnoreCase))
                {
                    // Caso raro: <li> fuori da <ul>/<ol>
                    var text = StripTag(block, "li");
                    col.Item().Text(text);
                }
                else if (Regex.IsMatch(block, "^<p>", RegexOptions.IgnoreCase))
                {
                    var text = StripTag(block, "p");
                    var lines = text.Split('\n');
                    
                    // Spaziatura per paragrafi
                    col.Item().PaddingVertical(3);
                    
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            col.Item().Element(e => RenderFormattedLine(e, line));
                        }
                        else
                        {
                            // Riga vuota = spazio extra
                            col.Item().PaddingVertical(2);
                        }
                    }
                }
                else
                {
                    // Testo libero o con newline
                    var lines = block.Split('\n');
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            col.Item().PaddingVertical(1).Element(e => RenderFormattedLine(e, line));
                        }
                        else
                        {
                            // Riga vuota = spazio extra  
                            col.Item().PaddingVertical(3);
                        }
                    }
                }
            }
        }

        private string StripTag(string html, string tag)
        {
            var result = Regex.Replace(html, $"</?{tag}>", "", RegexOptions.IgnoreCase).Trim();
            return NormalizeSpaces(result);
        }

        // Gestione grassetto/corsivo inline
        private void RenderFormattedLine(IContainer container, string htmlLine)
        {
            if (string.IsNullOrWhiteSpace(htmlLine))
            {
                container.Text("");
                return;
            }
            var tagRegex = new Regex(@"(<(/?)(strong|b|i|em)>)", RegexOptions.IgnoreCase);
            var matches = tagRegex.Matches(htmlLine);
            var segments = new List<(string text, bool bold, bool italic)>();
            int lastIndex = 0;
            bool bold = false, italic = false;
            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    segments.Add((htmlLine.Substring(lastIndex, match.Index - lastIndex), bold, italic));
                }
                string tag = match.Groups[3].Value.ToLower();
                bool isClosing = match.Groups[2].Value == "/";
                if ((tag == "strong" || tag == "b"))
                    bold = !isClosing;
                if ((tag == "i" || tag == "em"))
                    italic = !isClosing;
                lastIndex = match.Index + match.Length;
            }
            if (lastIndex < htmlLine.Length)
                segments.Add((htmlLine.Substring(lastIndex), bold, italic));
            container.Text(text =>
            {
                foreach (var seg in segments)
                {
                    var span = text.Span(seg.text).FontSize(12);
                    if (seg.bold) span = span.Bold();
                    if (seg.italic) span = span.Italic();
                }
            });
        }

        // Metodo helper per normalizzare gli spazi
        private string NormalizeSpaces(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
                
            // Normalizza spazi multipli
            text = Regex.Replace(text, @"\s+", " ");
            
            // Rimuovi spazi all'inizio e fine
            text = text.Trim();
            
            return text;
        }
        
        // Metodo helper per gestire contenuto vuoto tra tag
        private bool IsEmptyContent(string content)
        {
            return string.IsNullOrWhiteSpace(content) || 
                   content.Trim() == "&nbsp;" || 
                   content.Trim() == " ";
        }
    }
}

