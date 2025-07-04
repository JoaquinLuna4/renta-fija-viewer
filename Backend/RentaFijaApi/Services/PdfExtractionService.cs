using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Globalization;
using System.Text.RegularExpressions;
using RentaFijaApi.DTOs; // Asegúrate de que tu DTO esté en este namespace o ajusta.

namespace RentaFijaApi.Services
{
    public class PdfExtractionService : IPdfExtractionService
    {
        public string ExtractFullTextFromPdf(string pdfFilePath)
        {
            using (PdfDocument document = PdfDocument.Open(pdfFilePath))
            {
                var textBuilder = new System.Text.StringBuilder();
                foreach (Page page in document.GetPages())
                {
                    // `page.Text` suele dar un buen resultado de texto en orden de lectura
                    // aunque a veces puede necesitar post-procesamiento para saltos de línea.
                    textBuilder.AppendLine(page.Text);
                    textBuilder.AppendLine("--- FIN DE PÁGINA ---"); // Opcional, para delimitar páginas en el prompt si es necesario
                }
                Console.WriteLine($"Texto extraído del PDF: {textBuilder.Length} caracteres.");
                return textBuilder.ToString();
            }
        }
    }
}