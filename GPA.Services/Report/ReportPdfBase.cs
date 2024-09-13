using DinkToPdf;
using DinkToPdf.Contracts;

namespace GPA.Services.Report
{
    public interface IReportPdfBase
    {
        byte[] GeneratePdf(string htmlContent, string documentTitle = "", string header = "", string footer = "");
    }

    public class ReportPdfBase : IReportPdfBase
    {
        private readonly IConverter _converter;

        public ReportPdfBase(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GeneratePdf(string htmlContent, string documentTitle = "", string header = "", string footer = "")
        {
            var globalSettings = new GlobalSettings {
                ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                };

            if (documentTitle is { Length: > 0 })
            {
                globalSettings.DocumentTitle = documentTitle;
            }

            var objects = new ObjectSettings()
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" }
            };

            if (header is { Length: > 0 })
            {
                objects.HeaderSettings = new HeaderSettings { FontName = "Arial", FontSize = 9, Right = header, Line = true };
            }

            if (footer is { Length: > 0 })
            {
                objects.FooterSettings = new FooterSettings { FontName = "Arial", FontSize = 9, Line = true, Center = footer };
            }

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = {
                    objects
                }
            };

            return _converter.Convert(doc);
        }
    }
}
