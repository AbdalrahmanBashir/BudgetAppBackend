using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.Reports;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BudgetAppBackend.Infrastructure.Services
{
    public class PdfReportService : IPdfReportService
    {
        public byte[] GenerateFinancialReport(FinancialReportDto reportData)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header()
                        .PaddingBottom(10)
                        .BorderBottom(1)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("Financial Report")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3).SemiBold();

                            column.Item().AlignCenter().Text($"{reportData.UserFullName} – {reportData.ReportDate:yyyy-MM-dd}")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken2).SemiBold();

                            column.Item().AlignCenter().Text($"Report ID: {reportData.ReportId}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium).SemiBold();

                            column.Item().AlignCenter().Text($"Report Period: {reportData.ReportPeriod}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium).SemiBold();
                        });


                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(20);

                            // Summary Section  
                            col.Item().Component(new SummarySection(reportData));

                            // Budgets Section  
                            col.Item().Component(new BudgetsSection(reportData.Budgets));

                            // Transactions Section  
                            col.Item().Component(new TransactionsSection(reportData.RecentTransactions));
                        });

                    page.Footer()
                        .BorderTop(1)
                        .PaddingTop(10)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }


    public class SummarySection : QuestPDF.Infrastructure.IComponent
    {
        private readonly FinancialReportDto _data;
        private readonly Color PrimaryColor = Colors.Blue.Darken2;
        private readonly Color SuccessColor = Colors.Green.Darken2;
        private readonly Color DangerColor = Colors.Red.Darken2;

        public SummarySection(FinancialReportDto data) => _data = data;
        public void Compose(QuestPDF.Infrastructure.IContainer container)
        {
            container
                .Column(col =>
                {
                    col.Item()
                        .PaddingBottom(10)
                        .Text("Financial Summary")
                        .Bold()
                        .FontSize(18)
                        .FontColor(PrimaryColor);

                    //Table for summary
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(3);  
                            columns.RelativeColumn(2); 
                        });

                        // Table header
                        table.Header(header =>
                        {

                            header.Cell().PaddingBottom(5).Text("Income").FontColor(Colors.Grey.Darken1).SemiBold();
                            header.Cell().PaddingBottom(5).Text("Expenses").FontColor(Colors.Grey.Darken1).SemiBold();
                            header.Cell().PaddingBottom(5).AlignRight().Text("Net Savings").FontColor(Colors.Grey.Darken1).SemiBold();
                        });

                        // Table rows

                        table.Cell().Text(_data.TotalIncome.ToString("C2")).FontSize(12).FontColor(Colors.Green.Darken2);
                        table.Cell().Text(_data.TotalExpenses.ToString("C2")).FontSize(12).FontColor(Colors.Red.Darken2);
                        table.Cell()
                        .AlignRight()
                            .Text(_data.NetSavings.ToString("C2"))
                            .FontSize(12)
                            .FontColor(_data.NetSavings >= 0 ? SuccessColor : DangerColor);
                    });
                });
        }
    }

    public class BudgetsSection : QuestPDF.Infrastructure.IComponent
    {
        private readonly List<BudgetDto> _budgets;
        private readonly Color PrimaryColor = Colors.Blue.Darken2;
        private readonly Color SuccessColor = Colors.Green.Darken2;
        private readonly Color DangerColor = Colors.Red.Darken2;

        public BudgetsSection(List<BudgetDto> budgets) => _budgets = budgets;

        public void Compose(QuestPDF.Infrastructure.IContainer container)
        {
            container
                .Column(col =>
                {
                    // Fixed padding application
                    col.Item().PaddingBottom(15).Text("Budget Overview")
                        .Bold()
                        .FontSize(16)
                        .FontColor(PrimaryColor);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f);  // Category
                            columns.RelativeColumn(3);  // Budgeted
                            columns.RelativeColumn(2.5f);  // Spent
                            columns.RelativeColumn(2);  // Remaining
                        });

                        // Table header
                        table.Header(header =>
                        {
                        
                            header.Cell().PaddingBottom(5).Text("Budgeted").FontColor(Colors.Grey.Darken1).SemiBold();
                            header.Cell().PaddingBottom(5).Text("Spent").FontColor(Colors.Grey.Darken1).SemiBold();
                            header.Cell().PaddingBottom(5).Text("Remaining").FontColor(Colors.Grey.Darken1).SemiBold();
                            header.Cell().PaddingBottom(5).AlignRight().Text("Category").FontColor(Colors.Grey.Darken1).SemiBold();
                        });

                        foreach (var budget in _budgets)
                        {
                            var remaining = budget.TotalAmount - budget.SpentAmount;

                            table.Cell().Text(budget.TotalAmount.ToString("C2")).FontSize(10);
                            table.Cell().Text(budget.SpentAmount.ToString("C2")).FontSize(10);
                            table.Cell().Text(remaining.ToString("C2"))
                                .FontSize(10)
                                .FontColor(remaining >= 0 ? SuccessColor : DangerColor);
                            table.Cell().AlignRight().Text(budget.Category).FontSize(10);
                        }
                    });
                });
        }
    }

    public class TransactionsSection : QuestPDF.Infrastructure.IComponent
    {
        private readonly List<TransactionDto> _transactions;
        private readonly Color PrimaryColor = Colors.Blue.Darken2;
        private readonly Color PositiveColor = Colors.Green.Darken3;
        private readonly Color NegativeColor = Colors.Red.Darken3;

        public TransactionsSection(List<TransactionDto> transactions) => _transactions = transactions;

        public void Compose(QuestPDF.Infrastructure.IContainer container)
        {
            container.Column(col =>
            {
                // Fixed padding application
                col.Item().PaddingBottom(10).Text("Transactions")
                    .Bold()
                    .FontSize(16)
                    .FontColor(PrimaryColor);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2);
                    });

                    // Header with fixed padding
                    table.Header(header =>
                    {
                        header.Cell().PaddingBottom(5).Text("Date").FontColor(Colors.Grey.Darken1).SemiBold();
                        header.Cell().PaddingBottom(5).Text("Payee").FontColor(Colors.Grey.Darken1).SemiBold();
                        header.Cell().PaddingBottom(5).Text("Amount").FontColor(Colors.Grey.Darken1).SemiBold();
                        header.Cell().PaddingBottom(5).AlignRight().Text("Categories").FontColor(Colors.Grey.Darken1).SemiBold();
                    });

                    foreach (var transaction in _transactions)
                    {
                        table.Cell().Text(transaction.TransactionDate.ToString("dd MMM yyyy")).FontSize(10);
                        table.Cell().Text(transaction.Payee).FontSize(10);
                        table.Cell()
                            .Text(transaction.Amount.ToString("C2"))
                            .FontSize(10)
                            .FontColor(transaction.Amount > 0 ? PositiveColor : NegativeColor);
                        table.Cell().AlignRight().Text(string.Join(", ", transaction.Categories)).FontSize(10);
                    }
                });
            });
        }
    }
}