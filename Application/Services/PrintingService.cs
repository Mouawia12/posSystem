using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Application.Services
{
    public sealed class PrintingService : IPrintingService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public PrintingService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<bool> PrintInvoiceAsync(long invoiceId, PrintTemplateType templateType, string? printerName = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var invoice = await db.Invoices
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

            if (invoice is null)
            {
                return false;
            }

            var document = templateType == PrintTemplateType.A4
                ? BuildA4Document(invoice)
                : BuildThermalDocument(invoice);

            var printDialog = new PrintDialog();
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                var server = new LocalPrintServer();
                var queue = server.GetPrintQueues().FirstOrDefault(x =>
                    string.Equals(x.Name, printerName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.FullName, printerName, StringComparison.OrdinalIgnoreCase));

                if (queue is not null)
                {
                    printDialog.PrintQueue = queue;
                }
            }

            if (printDialog.ShowDialog() != true)
            {
                return false;
            }

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            printDialog.PrintDocument(paginator, $"Invoice {invoice.InvoiceNumber}");
            return true;
        }

        private static FlowDocument BuildA4Document(Core.Entities.Invoice invoice)
        {
            var document = new FlowDocument
            {
                PageWidth = 793.7,
                PageHeight = 1122.5,
                PagePadding = new Thickness(40),
                FontSize = 12
            };

            document.Blocks.Add(new Paragraph(new Run("INVOICE")) { FontSize = 24, FontWeight = FontWeights.SemiBold });
            document.Blocks.Add(new Paragraph(new Run($"Invoice #: {invoice.InvoiceNumber}")));
            document.Blocks.Add(new Paragraph(new Run($"Date: {invoice.CreatedAt:yyyy-MM-dd HH:mm}")));
            document.Blocks.Add(new Paragraph(new Run($"Customer: {invoice.Customer?.FullName ?? "Walk-in"}")));
            document.Blocks.Add(new Paragraph(new Run(string.Empty)));

            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(300) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });
            table.Columns.Add(new TableColumn { Width = new GridLength(120) });
            table.Columns.Add(new TableColumn { Width = new GridLength(120) });

            var rowGroup = new TableRowGroup();
            rowGroup.Rows.Add(new TableRow
            {
                Cells =
                {
                    new TableCell(new Paragraph(new Run("Item")) { FontWeight = FontWeights.SemiBold }),
                    new TableCell(new Paragraph(new Run("Qty")) { FontWeight = FontWeights.SemiBold }),
                    new TableCell(new Paragraph(new Run("Price")) { FontWeight = FontWeights.SemiBold }),
                    new TableCell(new Paragraph(new Run("Total")) { FontWeight = FontWeights.SemiBold })
                }
            });

            foreach (var item in invoice.Items)
            {
                rowGroup.Rows.Add(new TableRow
                {
                    Cells =
                    {
                        new TableCell(new Paragraph(new Run(item.Product.Name))),
                        new TableCell(new Paragraph(new Run(item.Quantity.ToString("0.###")))),
                        new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("0.00")))),
                        new TableCell(new Paragraph(new Run(item.LineTotal.ToString("0.00"))))
                    }
                });
            }

            table.RowGroups.Add(rowGroup);
            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run(string.Empty)));

            document.Blocks.Add(new Paragraph(new Run($"Subtotal: {invoice.Subtotal:0.00}")));
            document.Blocks.Add(new Paragraph(new Run($"Discount: {invoice.Discount:0.00}")));
            document.Blocks.Add(new Paragraph(new Run($"Tax: {invoice.Tax:0.00}")));
            document.Blocks.Add(new Paragraph(new Run($"Total: {invoice.Total:0.00}")) { FontWeight = FontWeights.Bold });

            var qr = BuildQrImage(invoice.Id.ToString());
            document.Blocks.Add(new BlockUIContainer(qr));

            return document;
        }

        private static FlowDocument BuildThermalDocument(Core.Entities.Invoice invoice)
        {
            var document = new FlowDocument
            {
                PageWidth = 302,
                PagePadding = new Thickness(8),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run("POS RECEIPT")) { FontWeight = FontWeights.Bold, FontSize = 16, TextAlignment = TextAlignment.Center });
            document.Blocks.Add(new Paragraph(new Run($"#{invoice.InvoiceNumber}")) { TextAlignment = TextAlignment.Center });
            document.Blocks.Add(new Paragraph(new Run($"{invoice.CreatedAt:yyyy-MM-dd HH:mm}")) { TextAlignment = TextAlignment.Center });
            document.Blocks.Add(new Paragraph(new Run("----------------------------------------")));

            foreach (var item in invoice.Items)
            {
                document.Blocks.Add(new Paragraph(new Run($"{item.Product.Name}")));
                document.Blocks.Add(new Paragraph(new Run($"{item.Quantity:0.###} x {item.UnitPrice:0.00} = {item.LineTotal:0.00}")));
            }

            document.Blocks.Add(new Paragraph(new Run("----------------------------------------")));
            document.Blocks.Add(new Paragraph(new Run($"Subtotal: {invoice.Subtotal:0.00}")));
            document.Blocks.Add(new Paragraph(new Run($"Discount: {invoice.Discount:0.00}")));
            document.Blocks.Add(new Paragraph(new Run($"Tax: {invoice.Tax:0.00}")));
            document.Blocks.Add(new Paragraph(new Run($"Total: {invoice.Total:0.00}")) { FontWeight = FontWeights.Bold });
            document.Blocks.Add(new Paragraph(new Run(string.Empty)));
            document.Blocks.Add(new BlockUIContainer(BuildQrImage(invoice.Id.ToString(), 100)));
            document.Blocks.Add(new Paragraph(new Run("Thank you!")) { TextAlignment = TextAlignment.Center });

            return document;
        }

        private static Image BuildQrImage(string payload, int pixels = 120)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            var bytes = png.GetGraphic(10);

            var image = new BitmapImage();
            using var stream = new MemoryStream(bytes);
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();

            return new Image
            {
                Source = image,
                Width = pixels,
                Height = pixels,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
    }
}
