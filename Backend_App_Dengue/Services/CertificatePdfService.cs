using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend_App_Dengue.Services
{
    public class CertificatePdfService
    {
        private readonly string _assetsPath;
        private readonly byte[]? _ucevaLogoBytes;
        private readonly byte[]? _uniamazoniaLogoBytes;
        private readonly byte[]? _mincienciasLogoBytes;
        private readonly ILogger<CertificatePdfService> _logger;

        public CertificatePdfService(IWebHostEnvironment env, ILogger<CertificatePdfService> logger)
        {
            _logger = logger;
            _assetsPath = Path.Combine(env.ContentRootPath, "Assets");

            // Configure QuestPDF license (Community license for open source)
            QuestPDF.Settings.License = LicenseType.Community;

            // Try to load images as byte arrays for better compatibility
            try
            {
                var ucevaPath = Path.Combine(_assetsPath, "uceva.png");
                var uniamazoniaPath = Path.Combine(_assetsPath, "logouniamazonia.png");
                var mincienciasPath = Path.Combine(_assetsPath, "minciencias.png");

                if (File.Exists(ucevaPath))
                {
                    _ucevaLogoBytes = File.ReadAllBytes(ucevaPath);
                    logger.LogInformation($"UCEVA logo loaded: {_ucevaLogoBytes.Length} bytes");
                }

                if (File.Exists(uniamazoniaPath))
                {
                    _uniamazoniaLogoBytes = File.ReadAllBytes(uniamazoniaPath);
                    logger.LogInformation($"Uniamazonia logo loaded: {_uniamazoniaLogoBytes.Length} bytes");
                }

                if (File.Exists(mincienciasPath))
                {
                    _mincienciasLogoBytes = File.ReadAllBytes(mincienciasPath);
                    logger.LogInformation($"Minciencias logo loaded: {_mincienciasLogoBytes.Length} bytes");
                }

                logger.LogInformation($"CertificatePdfService initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not load logo images: {ex.Message}. Will use text placeholders.");
            }
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

                    // Borde elegante y contenido - REDUCIDO PADDING Y SPACING PARA UNA SOLA PÁGINA
                    page.Content().Border(8).BorderColor("#1E8449")
                        .Border(2).BorderColor("#27AE60")
                        .Padding(35).Column(column =>
                        {
                            column.Spacing(8);

                            // === HEADER CON LOGOS (COMPACTO) ===
                            column.Item().Row(row =>
                            {
                                // Logo UCEVA (izquierda) - más pequeño
                                row.ConstantItem(110).Height(55).AlignMiddle().Container()
                                    .Border(1).BorderColor("#E8F8F5").Background("#F8F9FA")
                                    .Padding(4).AlignCenter().AlignMiddle()
                                    .Element(container =>
                                    {
                                        if (_ucevaLogoBytes != null && _ucevaLogoBytes.Length > 0)
                                        {
                                            try
                                            {
                                                container.Image(_ucevaLogoBytes).FitArea();
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogWarning($"Error loading UCEVA logo: {ex.Message}");
                                                container.Text("UCEVA").FontSize(12).Bold().FontColor("#2C5F2D");
                                            }
                                        }
                                        else
                                        {
                                            container.Column(col =>
                                            {
                                                col.Item().Text("UCEVA").FontSize(14).Bold().FontColor("#2C5F2D").AlignCenter();
                                                col.Item().Text("Universidad").FontSize(7).FontColor("#566573").AlignCenter();
                                            });
                                        }
                                    });

                                // Espacio central con título - más pequeño
                                row.RelativeItem().PaddingHorizontal(12).AlignMiddle().Column(col =>
                                {
                                    col.Item().AlignCenter().Text("CERTIFICADO DE ACREDITACIÓN")
                                        .FontSize(22)
                                        .Bold()
                                        .FontColor("#1E8449");

                                    col.Item().AlignCenter().PaddingTop(3).Container()
                                        .Width(250).Height(2)
                                        .Background("#27AE60");
                                });

                                // Logos colaboradores (derecha) - más pequeños
                                row.ConstantItem(150).Height(55).Row(logoRow =>
                                {
                                    // Uniamazonia
                                    logoRow.RelativeItem().Height(55).AlignMiddle().Container()
                                        .Border(1).BorderColor("#E8F8F5").Background("#F8F9FA")
                                        .Padding(3).AlignCenter().AlignMiddle()
                                        .Element(container =>
                                        {
                                            if (_uniamazoniaLogoBytes != null && _uniamazoniaLogoBytes.Length > 0)
                                            {
                                                try
                                                {
                                                    container.Image(_uniamazoniaLogoBytes).FitArea();
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogWarning($"Error loading Uniamazonia logo: {ex.Message}");
                                                    container.Text("UNIA").FontSize(9).Bold().FontColor("#2C5F2D");
                                                }
                                            }
                                            else
                                            {
                                                container.Text("UNIA").FontSize(9).Bold().FontColor("#2C5F2D");
                                            }
                                        });

                                    logoRow.ConstantItem(6);

                                    // Minciencias
                                    logoRow.RelativeItem().Height(55).AlignMiddle().Container()
                                        .Border(1).BorderColor("#E8F8F5").Background("#F8F9FA")
                                        .Padding(3).AlignCenter().AlignMiddle()
                                        .Element(container =>
                                        {
                                            if (_mincienciasLogoBytes != null && _mincienciasLogoBytes.Length > 0)
                                            {
                                                try
                                                {
                                                    container.Image(_mincienciasLogoBytes).FitArea();
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogWarning($"Error loading Minciencias logo: {ex.Message}");
                                                    container.Text("MIN").FontSize(9).Bold().FontColor("#2C5F2D");
                                                }
                                            }
                                            else
                                            {
                                                container.Text("MIN").FontSize(9).Bold().FontColor("#2C5F2D");
                                            }
                                        });
                                });
                            });

                            // === SEPARADOR DECORATIVO (REDUCIDO) ===
                            column.Item().PaddingVertical(6).Row(row =>
                            {
                                row.RelativeItem().Height(2).Background("#E8F8F5");
                                row.ConstantItem(50).Height(3).Background("#1E8449");
                                row.RelativeItem().Height(2).Background("#E8F8F5");
                            });

                            // === TÍTULO PRINCIPAL (MÁS COMPACTO) ===
                            column.Item().PaddingTop(8).AlignCenter().Column(titleCol =>
                            {
                                titleCol.Item().Text("Prevención y Control del Dengue")
                                    .FontSize(26)
                                    .Bold()
                                    .FontColor("#1E8449");

                                titleCol.Item().PaddingTop(3).Text("Certificado de Competencia en Salud Pública")
                                    .FontSize(13)
                                    .Italic()
                                    .FontColor("#7F8C8D");
                            });

                            // === CUERPO DEL CERTIFICADO (COMPACTO) ===
                            column.Item().PaddingTop(12).PaddingHorizontal(25).Container()
                                .Border(2).BorderColor("#E8F8F5").Background("#FDFEFE")
                                .Padding(18).Column(bodyCol =>
                                {
                                    // Texto certificación
                                    bodyCol.Item().Text(text =>
                                    {
                                        text.DefaultTextStyle(x => x.FontSize(11).FontColor("#34495E").LineHeight(1.4f));
                                        text.Span("Por medio del presente se certifica que ");
                                        text.Span(data.UserName).FontSize(14).Bold().FontColor("#1E8449");
                                        text.Span(", identificado(a) con correo electrónico ");
                                        text.Span(data.UserEmail).FontSize(10).Bold().FontColor("#2874A6").Underline();
                                        text.Span(", ha completado exitosamente el curso de capacitación en ");
                                        text.Span("Prevención y Control del Dengue").Bold();
                                        text.Span(", demostrando competencias en:");
                                    });

                                    // Competencias en tarjetas (más pequeñas)
                                    bodyCol.Item().PaddingTop(10).Row(compRow =>
                                    {
                                        // Competencia 1
                                        compRow.RelativeItem().PaddingRight(6).Container()
                                            .Background("#E8F8F5").Padding(8).Column(comp =>
                                            {
                                                comp.Item().Text("✓ Identificación").FontSize(9).Bold().FontColor("#1E8449");
                                                comp.Item().Text("Síntomas y signos").FontSize(8).FontColor("#566573");
                                            });

                                        // Competencia 2
                                        compRow.RelativeItem().PaddingHorizontal(3).Container()
                                            .Background("#E8F8F5").Padding(8).Column(comp =>
                                            {
                                                comp.Item().Text("✓ Prevención").FontSize(9).Bold().FontColor("#1E8449");
                                                comp.Item().Text("Eliminación criaderos").FontSize(8).FontColor("#566573");
                                            });

                                        // Competencia 3
                                        compRow.RelativeItem().PaddingLeft(6).Container()
                                            .Background("#E8F8F5").Padding(8).Column(comp =>
                                            {
                                                comp.Item().Text("✓ Protección").FontSize(9).Bold().FontColor("#1E8449");
                                                comp.Item().Text("Personal y comunitaria").FontSize(8).FontColor("#566573");
                                            });
                                    });
                                });

                            // === CALIFICACIÓN (COMPACTA) ===
                            column.Item().PaddingTop(12).Row(scoreRow =>
                            {
                                scoreRow.RelativeItem();

                                scoreRow.ConstantItem(250).Container()
                                    .Border(2).BorderColor("#1E8449")
                                    .Background("#E8F8F5")
                                    .Padding(14).Column(scoreCol =>
                                    {
                                        scoreCol.Item().AlignCenter().Row(row =>
                                        {
                                            row.RelativeItem().AlignCenter().Column(col =>
                                            {
                                                col.Item().Text("CALIFICACIÓN").FontSize(9).Bold().FontColor("#117864");
                                                col.Item().Text($"{data.Score:F1}%").FontSize(30).Bold().FontColor("#1E8449");
                                            });

                                            row.ConstantItem(12);

                                            row.ConstantItem(2).Height(50).Background("#27AE60");

                                            row.ConstantItem(12);

                                            row.RelativeItem().AlignMiddle().Column(col =>
                                            {
                                                col.Item().Text($"{data.CorrectAnswers}/{data.TotalQuestions}").FontSize(16).Bold().FontColor("#1E8449");
                                                col.Item().Text("Respuestas correctas").FontSize(8).FontColor("#566573");
                                            });
                                        });
                                    });

                                scoreRow.RelativeItem();
                            });

                            // === FOOTER CON FECHA Y CÓDIGO (COMPACTO) ===
                            column.Item().PaddingTop(14).Row(footerRow =>
                            {
                                // Fecha
                                footerRow.RelativeItem().Container()
                                    .Border(1).BorderColor("#E8F8F5")
                                    .Padding(10).Column(col =>
                                    {
                                        col.Item().AlignCenter().Text("Fecha de Emisión")
                                            .FontSize(8).FontColor("#7F8C8D");
                                        col.Item().AlignCenter().Text(data.IssuedAt.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES")))
                                            .FontSize(10).Bold().FontColor("#1E8449");
                                    });

                                footerRow.ConstantItem(20);

                                // Código de verificación
                                footerRow.RelativeItem().Container()
                                    .Border(1).BorderColor("#E8F8F5")
                                    .Padding(10).Column(col =>
                                    {
                                        col.Item().AlignCenter().Text("Código de Verificación")
                                            .FontSize(8).FontColor("#7F8C8D");
                                        col.Item().AlignCenter().Text(data.VerificationCode)
                                            .FontSize(10).Bold().FontColor("#2874A6");
                                    });
                            });

                            // === PIE DE PÁGINA (COMPACTO) ===
                            column.Item().PaddingTop(8).AlignCenter().Column(footCol =>
                            {
                                footCol.Item().Text(text =>
                                {
                                    text.Span("Universidad Central del Valle del Cauca - UCEVA").FontSize(8).Bold().FontColor("#1E8449");
                                    text.Span(" • ").FontSize(8).FontColor("#7F8C8D");
                                    text.Span("Sistema de Monitoreo y Control del Dengue").FontSize(8).Italic().FontColor("#566573");
                                });

                                footCol.Item().PaddingTop(2).Text("Verifique la autenticidad con el código de verificación en nuestro sistema")
                                    .FontSize(7).Italic().FontColor("#95A5A6");
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
