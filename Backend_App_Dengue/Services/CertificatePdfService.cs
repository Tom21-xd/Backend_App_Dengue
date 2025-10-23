using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend_App_Dengue.Services
{
    public class CertificatePdfService
    {
        private readonly string _assetsPath;

        public CertificatePdfService(IWebHostEnvironment env)
        {
            _assetsPath = Path.Combine(env.ContentRootPath, "Assets");

            // Configure QuestPDF license (Community license for open source)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateCertificatePdf(CertificateData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.PageColor(Colors.White);

                    page.Content().Border(15).BorderColor("#2C5F2D").Padding(40).Column(column =>
                    {
                        column.Spacing(15);

                        // Header with logos
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Column(col =>
                            {
                                col.Item().Height(55).Image(Path.Combine(_assetsPath, "uceva.png"));
                            });

                            row.RelativeItem().AlignCenter().PaddingTop(10).Text("CERTIFICADO DE ACREDITACIÓN")
                                .FontSize(24)
                                .Bold()
                                .FontColor("#2C5F2D");

                            row.RelativeItem().AlignRight().Row(logoRow =>
                            {
                                logoRow.AutoItem().Height(48).Image(Path.Combine(_assetsPath, "logouniamazonia.png")).FitWidth();
                                logoRow.AutoItem().Width(10);
                                logoRow.AutoItem().Height(48).Image(Path.Combine(_assetsPath, "minciencias.png")).FitWidth();
                            });
                        });

                        // Decorative line
                        column.Item().Height(3).Background("#2C5F2D");

                        // Main title
                        column.Item().PaddingTop(25).AlignCenter().Text("Prevención y Control del Dengue")
                            .FontSize(28)
                            .Bold()
                            .FontColor("#1E8449");

                        // Subtitle
                        column.Item().AlignCenter().Text("Certificado de Competencia en Salud Pública")
                            .FontSize(16)
                            .Italic()
                            .FontColor("#566573");

                        // Certificate text
                        column.Item().PaddingTop(25).PaddingHorizontal(50).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(14).FontColor("#34495E").LineHeight(1.5f));
                            text.Span("Se certifica que ");
                            text.Span(data.UserName).FontSize(18).Bold().FontColor("#2C5F2D");
                            text.Span(" identificado(a) con correo electrónico ");
                            text.Span(data.UserEmail).FontSize(14).Bold().FontColor("#2874A6");
                            text.Span(", ha completado satisfactoriamente el curso de formación en Prevención y Control del Dengue, demostrando conocimientos sólidos sobre:");
                        });

                        // Knowledge areas
                        column.Item().PaddingTop(12).PaddingHorizontal(90).Column(knowledgeCol =>
                        {
                            knowledgeCol.Item().Row(row =>
                            {
                                row.AutoItem().Width(15).AlignCenter().Text("•").FontSize(12).FontColor("#16A085");
                                row.RelativeItem().Text("Identificación de síntomas y signos de alarma del dengue").FontSize(12).FontColor("#34495E");
                            });

                            knowledgeCol.Item().Row(row =>
                            {
                                row.AutoItem().Width(15).AlignCenter().Text("•").FontSize(12).FontColor("#16A085");
                                row.RelativeItem().Text("Medidas de prevención y eliminación de criaderos del vector").FontSize(12).FontColor("#34495E");
                            });

                            knowledgeCol.Item().Row(row =>
                            {
                                row.AutoItem().Width(15).AlignCenter().Text("•").FontSize(12).FontColor("#16A085");
                                row.RelativeItem().Text("Protección personal y comunitaria frente al Aedes aegypti").FontSize(12).FontColor("#34495E");
                            });

                            knowledgeCol.Item().Row(row =>
                            {
                                row.AutoItem().Width(15).AlignCenter().Text("•").FontSize(12).FontColor("#16A085");
                                row.RelativeItem().Text("Actuación ante casos sospechosos y situaciones de emergencia").FontSize(12).FontColor("#34495E");
                            });
                        });

                        // Score box
                        column.Item().PaddingTop(20).AlignCenter().Container()
                            .Width(250)
                            .Border(2)
                            .BorderColor("#1E8449")
                            .Background("#E8F8F5")
                            .Padding(15)
                            .Column(scoreCol =>
                            {
                                scoreCol.Item().AlignCenter().Text("CALIFICACIÓN OBTENIDA")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor("#117864");

                                scoreCol.Item().AlignCenter().Text($"{data.Score:F1}%")
                                    .FontSize(32)
                                    .Bold()
                                    .FontColor("#1E8449");

                                scoreCol.Item().AlignCenter().Text($"{data.CorrectAnswers} de {data.TotalQuestions} preguntas correctas")
                                    .FontSize(10)
                                    .FontColor("#566573");
                            });

                        // Footer with date and verification
                        column.Item().PaddingTop(25).Row(row =>
                        {
                            // Date
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignCenter().Text("Fecha de Emisión")
                                    .FontSize(10)
                                    .FontColor("#566573");
                                col.Item().AlignCenter().Text(data.IssuedAt.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES")))
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor("#2C5F2D");
                                col.Item().PaddingTop(5).AlignCenter().LineHorizontal(1).LineColor("#2C5F2D");
                            });

                            row.ConstantItem(50);

                            // Verification code
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignCenter().Text("Código de Verificación")
                                    .FontSize(10)
                                    .FontColor("#566573");
                                col.Item().AlignCenter().Text(data.VerificationCode)
                                    .FontSize(10)
                                    .Bold()
                                    .FontColor("#2874A6");
                                col.Item().PaddingTop(5).AlignCenter().LineHorizontal(1).LineColor("#2C5F2D");
                            });
                        });

                        // Institution footer
                        column.Item().PaddingTop(15).AlignCenter().Text(text =>
                        {
                            text.Span("Universidad Central del Valle del Cauca - UCEVA").FontSize(10).Bold().FontColor("#2C5F2D");
                            text.Span(" | ").FontSize(10).FontColor("#566573");
                            text.Span("Sistema de Monitoreo de Dengue").FontSize(10).Italic().FontColor("#566573");
                        });

                        // Verification note
                        column.Item().AlignCenter().Text("Verifique la autenticidad de este certificado en el sistema con el código de verificación")
                            .FontSize(8)
                            .Italic()
                            .FontColor("#7F8C8D");

                        // Decorative elements at bottom
                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Height(3).Background("#E8F8F5");
                            row.ConstantItem(30).Height(3).Background("#1E8449");
                            row.RelativeItem().Height(3).Background("#E8F8F5");
                            row.ConstantItem(30).Height(3).Background("#1E8449");
                            row.RelativeItem().Height(3).Background("#E8F8F5");
                        });
                    });
                });
            });

            using (var stream = new MemoryStream())
            {
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
        }
    }

    public class CertificateData
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public DateTime IssuedAt { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
    }
}
