using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Dynamic;
using System.Reflection;

namespace Api.Extensions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcelColumnAttribute : Attribute
    {
        public ExcelColumnAttribute(int columnIndex)
        {
            Index = columnIndex;
            Name = GetExcelColumnName(columnIndex);
        }

        public ExcelColumnAttribute(string excelColumn)
        {
            Index = GetExcelColumnIndex(excelColumn);
            Name = excelColumn;
        }

        public int Index { get; }
        public string Name { get; }

        private static int GetExcelColumnIndex(string columnHeader)
        {
            columnHeader = columnHeader.ToUpper();

            int result = 0;
            for (int i = 0; i < columnHeader.Length; i++)
            {
                result *= 26;
                char letter = columnHeader[i];

                if (letter < 'A') letter = 'A';
                if (letter > 'Z') letter = 'Z';

                result += letter - 'A' + 1;
            }

            return result - 1;
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            if (columnNumber == 0)
                return "A";

            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26 + 1;
                columnName = Convert.ToChar(65 + modulo).ToString(); // + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }

    /// <summary>
    /// Reads excel file and coverts the content into multiple results.
    /// </summary>
    public static class Excel
    {
        private static DataTable ExcelToDataTable(this ISheet sheet, int titleIndex = 0, int dataIndex = 1)
        {
            var dtExcelTable = new DataTable();
            dtExcelTable.Rows.Clear();
            dtExcelTable.Columns.Clear();
            dtExcelTable.TableName = sheet.SheetName;
            var headerRow = sheet.GetRow(titleIndex);
            int colCount = headerRow.LastCellNum;
            for (var c = 0; c < colCount; c++)
                dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
            var i = dataIndex;
            var currentRow = sheet.GetRow(i);
            while (currentRow != null)
            {
                var dr = dtExcelTable.NewRow();
                for (var j = 0; j < colCount; j++)
                {
                    var cell = currentRow.GetCell(j);
                    dr[j] = GetCellValue(cell);
                }
                dtExcelTable.Rows.Add(dr);
                i++;
                currentRow = sheet.GetRow(i);
            }
            return dtExcelTable;
        }

        /// <summary>
        /// Reads excel file and coverts the sheet into DataTable.
        /// </summary>
        /// <param name="stream">Excel stream.</param>
        /// <param name="sheetIndex">Index of sheet to read.</param>
        /// <param name="titleIndex">Specifies the title index if any.</param>
        /// <param name="dataIndex">Specifies the exact data index to read.</param>
        /// <returns>DataTable from excel sheet.</returns>
        public static DataTable ExcelToDataTable(this Stream stream, int sheetIndex = 0, int titleIndex = 0, int dataIndex = 1)
        {
            var sh = WorkbookFactory.Create(stream).GetSheetAt(sheetIndex);
            return sh.ExcelToDataTable(titleIndex, dataIndex);
        }

        /// <summary>
        /// Reads excel file and converts all the sheets into a DataSet. A DataSet can have multiple DataTable.
        /// </summary>
        /// <param name="stream">Excel stream.</param>
        /// <returns>DataSet from excel stream.</returns>
        public static DataSet ExcelToDataSet(this Stream stream)
        {
            var workbook = WorkbookFactory.Create(stream);
            var dataset = new DataSet();
            for (var i = 0; i < workbook.NumberOfSheets; i++)
            {
                var sheet = workbook.GetSheetAt(i);
                var dtExcelTable = sheet.ExcelToDataTable();
                dtExcelTable.AcceptChanges();
                dataset.Tables.Add(dtExcelTable);
            }

            return dataset;
        }

        private static IEnumerable<dynamic> ExcelToDynamic(this Stream stream, int sheetIndex = 0, int titleIndex = 0, int dataIndex = 1)
        {
            var entities = new List<dynamic>();
            //var eobj = new ExpandoObject();
            //var entities = (ICollection<KeyValuePair<string, object>>)eobj;

            IWorkbook workbook = WorkbookFactory.Create(stream);  //IWorkbook xls dan xlsx       
            var worksheet = workbook.GetSheetAt(sheetIndex);

            if (worksheet.PhysicalNumberOfRows < 1)
            {
                return entities;
            }

            var cellTitle = worksheet.GetRow(titleIndex).Cells;

            for (var c = dataIndex; c < worksheet.LastRowNum; c++)
            {
                IRow row = worksheet.GetRow(c);

                if (row == null)
                    continue;

                var entity = new ExpandoObject() as IDictionary<string, object>;
                //var dict = new Dictionary<string, object>();
                for (int i = 0; i < cellTitle.Count; i++)
                {
                    var excelTitle = cellTitle[i].StringCellValue.Replace(" ", "");
                    ICell cell = row.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                    var value = GetCellValue(cell);
                    entity.Add(excelTitle, value);
                }

                //var expando = dict.ToExpando();
                entities.Add(entity);
            }

            return entities;
        }

        private static int ToInt32(this object value)
        {
            if (value == null)
                return 0;

            _ = int.TryParse(value.ToString(), out int result);

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type of destination object mapping.</typeparam>
        /// <param name="stream">Excel stream.</param>
        /// <param name="sheetIndex">Index of sheet to read.</param>
        /// <param name="titleIndex">Specifies the title index if any.</param>
        /// <param name="dataIndex">Specifies the exact data index to read.</param>
        /// <returns>A list of object containing the value of the excel stream.</returns>
        public static IEnumerable<T> ExcelToEntity<T>(this Stream stream, int sheetIndex = 0, int titleIndex = 0, int dataIndex = 1) where T : class
        {
            var ttype = typeof(T);
            //if (ttype.FullName == typeof(object).FullName || ttype.FullName == typeof(ExpandoObject).FullName)
            //    return ExcelToDynamic<T>(stream, sheetIndex, titleIndex, dataIndex);

            IWorkbook workbook = WorkbookFactory.Create(stream);  //IWorkbook xls dan xlsx       
            var worksheet = workbook.GetSheetAt(sheetIndex);

            if (worksheet.PhysicalNumberOfRows < 1)
                return Enumerable.Empty<T>();

            var list = new List<T>();

            var cellTitle = worksheet.GetRow(titleIndex).Cells;

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            var attrType = typeof(ExcelColumnAttribute);

            var attributes = Activator.CreateInstance<T>().GetType().GetProperties()
                  .Select(x => new
                  {
                      ColumnIndex = x.GetCustomAttribute(attrType)?
                                    .GetType().GetProperty("Index", flags)?
                                    .GetValue(x.GetCustomAttribute(attrType), null)?.ToInt32(),
                      Property = x
                  }).Where(w => w.ColumnIndex != null).ToList();

            for (var rowIndex = dataIndex; rowIndex < worksheet.LastRowNum + 1; rowIndex++)
            {
                IRow row = worksheet.GetRow(rowIndex);

                if (row == null)
                {
                    //entities.Add(entity);
                    continue;
                }

                T entity = Activator.CreateInstance<T>();

                for (var colIndex = 0; colIndex < cellTitle.Count; colIndex++)
                {
                    var propName = attributes.Where(x => x.ColumnIndex == colIndex).Select(x => x.Property.Name).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(propName))
                        propName = cellTitle[colIndex].StringCellValue?.Replace(" ", "");

                    if (string.IsNullOrWhiteSpace(propName))
                        continue;

                    var prop = entity.GetType().GetProperty(propName, flags);
                    if (prop == null || !prop.CanWrite)
                        continue;

                    ICell cell = row.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                    var value = GetCellValue(cell);
                    var destType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (Nullable.GetUnderlyingType(prop.PropertyType) == null &&
                        string.IsNullOrWhiteSpace(Convert.ToString(value)))
                        continue;

                    if (destType == typeof(string))
                        value = value?.ToString();

                    if (destType == typeof(DateTime))
                    {
                        if (!DateTime.TryParse(Convert.ToString(value), out DateTime dateVal))
                        {
                            value = Convert.ToDateTime(value);
                        }
                        else
                            value = dateVal;
                    }

                    if (destType.IsNumericType())
                    {
                        if (!double.TryParse(Convert.ToString(value), out double doubleVal))
                            continue;
                        else
                            value = doubleVal;
                    }

                    if (destType == typeof(bool))
                    {
                        if (!bool.TryParse(Convert.ToString(value), out bool boolVal))
                            continue;
                        else
                            value = boolVal;
                    }

                    value = Convert.ChangeType(value, destType);
                    prop.SetValue(entity, value, null);
                }

                list.Add(entity);
            }

            return list;
        }

        private static object GetCellValue(ICell cell, string fileExtension = "")
        {
            if (cell == null)
                return null;

            var cellType = cell.CellType;
            if (cell.CellType == CellType.Formula)
                cellType = cell.CachedFormulaResultType;

            switch (cellType)
            {
                case CellType.Error:
                    return cell.ErrorCellValue.ToString();
                case CellType.Unknown:
                    return cell.StringCellValue;
                case CellType.Blank:
                    return null;
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.String:
                    return cell.StringCellValue?.Trim();
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        DateTime date = cell.DateCellValue.Value;
                        //ICellStyle style = cell.CellStyle;
                        // Excel uses lowercase m for month whereas .Net uses uppercase
                        //string format = style.GetDataFormatString().Replace('m', 'M');
                        return date.ToString("yyyy-MM-dd");
                    }
                    else
                        return cell.NumericCellValue;
                case CellType.Formula:
                    try
                    {
                        var result = "";
                        switch (fileExtension)
                        {
                            case ".xlsx":
                                XSSFFormulaEvaluator xss = new(cell.Sheet.Workbook);
                                xss.EvaluateInCell(cell);
                                result = cell.ToString();
                                break;
                            default:
                                HSSFFormulaEvaluator hss = new(cell.Sheet.Workbook);
                                hss.EvaluateInCell(cell);
                                result = cell.ToString();
                                break;

                        }
                        return result;
                    }
                    catch
                    {
                        return cell.NumericCellValue.ToString();
                    }

                default:
                    return cell.StringCellValue;
            }
        }
    }
}
