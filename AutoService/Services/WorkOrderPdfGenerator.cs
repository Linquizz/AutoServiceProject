using System.Globalization;
using AutoService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoService.Services;

public class WorkOrderPdfGenerator
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");
    private const string FontName = "Arial";
    private const float Line = 0.5f;

    static WorkOrderPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string Generate(WorkOrder order, string folder)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"ЗаказНаряд_{order.Id}.pdf");

        try
        {
            Build(order).GeneratePdf(path);
        }
        catch (IOException)
        {
            path = Path.Combine(folder, $"ЗаказНаряд_{order.Id}_{DateTime.Now:HHmmss}.pdf");
            Build(order).GeneratePdf(path);
        }

        return path;
    }

    private Document Build(WorkOrder order)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily(FontName).FontSize(9.5f));

                page.Header().Element(c => ComposeHeader(c, order));
                page.Content().PaddingVertical(8).Element(c => ComposeContent(c, order));
                page.Footer().AlignRight().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
                    t.Span($"Заказ-наряд № {order.Id}  |  стр. ");
                    t.CurrentPageNumber();
                    t.Span(" из ");
                    t.TotalPages();
                });
            });
        });
    }

    private static void ComposeHeader(IContainer container, WorkOrder o)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignBottom().Text($"ДОГОВОР ЗАКАЗ-НАРЯД НА РАБОТЫ № {o.Id}").Bold().FontSize(13);
                row.ConstantItem(220).Column(dates =>
                {
                    dates.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Дата приема заказа :");
                        r.ConstantItem(88).AlignRight().Text(o.StartDate.ToString("dd.MM.yyyy HH:mm", Ru));
                    });
                    dates.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Дата окончания работ :");
                        r.ConstantItem(88).AlignRight().Text(o.EndDate?.ToString("dd.MM.yyyy HH:mm", Ru) ?? "");
                    });
                });
            });

            col.Item().PaddingTop(4).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container, WorkOrder o)
    {
        var car = o.Car;
        var client = car?.Client;

        container.Column(col =>
        {
            col.Spacing(8);

            col.Item().Row(row =>
            {
                row.RelativeItem(3).Column(left =>
                {
                    Field(left, "Потребитель", client?.FullName);
                    Field(left, "Телефон", client?.Phone);
                    Field(left, "Автомобиль", car?.Title);
                    Field(left, "Год выпуска", car?.Year.ToString());
                    Field(left, "Пробег", car == null ? "" : $"{car.Mileage} км");
                    Field(left, "Гос. номер", car?.RegistrationNumber);
                });

                row.RelativeItem(2).Column(right =>
                {
                    Field(right, "VIN-номер", car?.VIN);
                    Field(right, "Тип двигателя", car?.EngineType);
                });
            });

            col.Item().Text(t =>
            {
                t.Span("Причины обращения : ").Bold();
                t.Span(o.Reason ?? "");
            });

            col.Item().Element(c => ComposeWorks(c, o));
            col.Item().Element(c => ComposeParts(c, o));
            col.Item().Element(c => ComposeTotals(c, o));
        });
    }

    private static void Field(ColumnDescriptor column, string label, string? value)
    {
        column.Item().Row(r =>
        {
            r.ConstantItem(95).AlignRight().Text(label + " :").Bold();
            r.RelativeItem().PaddingLeft(6).Text(value ?? "");
        });
    }

    private void ComposeWorks(IContainer container, WorkOrder o)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(5);
                c.RelativeColumn(2.2f);
                c.ConstantColumn(52);
                c.ConstantColumn(78);
            });

            table.Header(h =>
            {
                h.Cell().ColumnSpan(4).Element(Head).AlignCenter().Text("Выполненные работы").Bold().FontSize(10.5f);
                foreach (var title in new[] { "Выполненные работы", "Исполнитель", "Время, ч", "Стоимость" })
                    h.Cell().Element(Head).AlignCenter().Text(title).Bold();
            });

            foreach (var w in o.Works)
            {
                table.Cell().Element(Body).Text(w.Service?.Name ?? "");
                table.Cell().Element(Body).Text(w.Employee?.ShortName ?? "");
                table.Cell().Element(Body).AlignCenter().Text(w.Duration.HasValue ? w.Duration.Value.ToString("0.00", Ru) : "");
                table.Cell().Element(Body).AlignRight().Text(Money(w.Sum));
            }

            table.Cell().ColumnSpan(3).AlignRight().PaddingRight(4).PaddingTop(2).Text("Итого :").Bold();
            table.Cell().Element(Body).AlignRight().Text(Money(o.WorksTotal)).Bold();
        });
    }

    private void ComposeParts(IContainer container, WorkOrder o)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(105);
                c.RelativeColumn(5);
                c.ConstantColumn(48);
                c.ConstantColumn(66);
                c.ConstantColumn(78);
            });

            table.Header(h =>
            {
                h.Cell().ColumnSpan(5).Element(Head).AlignCenter().Text("Запасные части и расходные материалы").Bold().FontSize(10.5f);
                foreach (var title in new[] { "Номер", "Наименование", "Кол-во", "Цена", "Сумма" })
                    h.Cell().Element(Head).AlignCenter().Text(title).Bold();
            });

            foreach (var p in o.Parts)
            {
                table.Cell().Element(Body).AlignCenter().Text(p.Article);
                table.Cell().Element(Body).Text(p.Part?.Name ?? "");
                table.Cell().Element(Body).AlignCenter().Text(p.Quantity.ToString());
                table.Cell().Element(Body).AlignRight().Text(Money(p.Price));
                table.Cell().Element(Body).AlignRight().Text(Money(p.Sum));
            }

            table.Cell().ColumnSpan(2).AlignRight().PaddingRight(4).PaddingTop(2).Text("Итого :").Bold();
            table.Cell().Element(Body).AlignCenter().Text(o.Parts.Sum(p => p.Quantity).ToString()).Bold();
            table.Cell().Text("");
            table.Cell().Element(Body).AlignRight().Text(Money(o.PartsTotal)).Bold();
        });
    }

    private static void ComposeTotals(IContainer container, WorkOrder o)
    {
        container.Column(col =>
        {
            col.Spacing(4);

            col.Item().AlignRight().Column(sum =>
            {
                SumRow(sum, "Итого по работам :", Money(o.WorksTotal), false);
                SumRow(sum, "Итого по запасным частям :", Money(o.PartsTotal), false);
                SumRow(sum, "ВСЕГО К ОПЛАТЕ, руб. :", Money(o.Total), true);
            });

            col.Item().PaddingTop(28).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.5f);
                    c.Item().Text("Исполнитель (подпись, дата)").FontSize(8);
                });
                row.ConstantItem(40);
                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.5f);
                    c.Item().Text("Заказчик (подпись, дата)").FontSize(8);
                });
            });
        });
    }

    private static void SumRow(ColumnDescriptor col, string label, string value, bool bold)
    {
        col.Item().Row(r =>
        {
            var l = r.ConstantItem(160).AlignRight().Text(label);
            var v = r.ConstantItem(85).AlignRight().Text(value);
            if (bold)
            {
                l.Bold().FontSize(10.5f);
                v.Bold().FontSize(10.5f);
            }
        });
    }

    private static IContainer Head(IContainer c) =>
        c.Border(Line).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).Padding(3).AlignMiddle();

    private static IContainer Body(IContainer c) =>
        c.Border(Line).BorderColor(Colors.Black).Padding(3);

    private static string Money(decimal value) => value.ToString("F2", Ru);
}
