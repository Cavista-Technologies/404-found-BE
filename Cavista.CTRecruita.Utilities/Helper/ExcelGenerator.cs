using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Helper
{
    public class ExcelGenerator
    {
        public static byte[] GenerateExcelBytes( IEnumerable<IDictionary<string, object>> rows, IList<string> headers, string sheetName = "Sheet1", ISet<string> hyperlinkColumns = null)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (headers == null || headers.Count == 0) throw new ArgumentNullException(nameof(headers));
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet(sheetName);
            var helper = workbook.GetCreationHelper();
            var headerStyle = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);
            var linkStyle = workbook.CreateCellStyle();
            var linkFont = workbook.CreateFont();
            linkFont.Underline = FontUnderlineType.Single;
            linkFont.Color = IndexedColors.Blue.Index;
            linkStyle.SetFont(linkFont);
            var headerRow = sheet.CreateRow(0);
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }
            int rowIndex = 1;
            foreach (var item in rows)
            {
                var row = sheet.CreateRow(rowIndex++);
                for (int i = 0; i < headers.Count; i++)
                {
                    var cell = row.CreateCell(i);
                    if (!item.TryGetValue(headers[i], out var value) || value == null)
                    {
                        cell.SetCellValue(string.Empty);
                        continue;
                    }
                    if (hyperlinkColumns != null && hyperlinkColumns.Contains(headers[i]))
                    {
                        var text = value.ToString();
                        cell.SetCellValue(text);
                        if (Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                        {
                            var link = helper.CreateHyperlink(HyperlinkType.Url);
                            link.Address = text;
                            cell.Hyperlink = link;
                            cell.CellStyle = linkStyle;
                        }
                        continue;
                    }
                    switch (Type.GetTypeCode(value.GetType()))
                    {
                        case TypeCode.Boolean:
                            cell.SetCellValue((bool)value);
                            break;
                        case TypeCode.DateTime:
                            cell.SetCellValue((DateTime)value);
                            break;
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            cell.SetCellValue(Convert.ToDouble(value));
                            break;
                        default:
                            cell.SetCellValue(value.ToString());
                            break;
                    }
                }
            }
            for (int i = 0; i < headers.Count; i++)
                sheet.SetColumnWidth(i, 20 * 256);
            using var ms = new MemoryStream();
            workbook.Write(ms, leaveOpen: false);
            return ms.ToArray();
        }
    }
}
