using System;
using System.Collections.Generic;
using System.Linq;
using VerQL.Core.Models;

namespace VerQL.Core.Utils
{
  public static class Helpers
  {
    public static string RemoveSquareBrackets(this string Text)
    {
      return Text.Replace("[", "").Replace("]", "");
    }

    public static string RemoveSquareBracketsNotInQuotes(this string Text)
    {
      var r = "";
      var singleOpen = false;
      var doubleOpen = false;
      foreach (var c in Text)
      {
        if (c == '"') doubleOpen = !doubleOpen;
        else if (c == '\'') singleOpen = !singleOpen;
        else if ((c != '[' && c != ']') || singleOpen || doubleOpen) r += c;
      }
      return r;
    }

    public static string RemoveBrackets(this string Text)
    {
      return Text.Replace("(", "").Replace(")", "");
    }

    public static string RemoveBracketsNotInQuotes(this string Text)
    {
      var r = "";
      var singleOpen = false;
      var doubleOpen = false;
      foreach (var c in Text)
      {
        if (c == '"') doubleOpen = !doubleOpen;
        else if (c == '\'') singleOpen = !singleOpen;
        else if ((c != '(' && c != ')') || singleOpen || doubleOpen) r += c;
      }
      return r;
    }

    public static string GetKey(this Base b)
    {
      return $"[{b.Schema}].[{b.Name}]";
    }

    public static string GetTableKey(this TableBase b)
    {
      return $"[{b.TableSchema}].[{b.TableName}]";
    }


    public static List<string> TrueSplit(this string text, bool keepSeparator = true, char separator = ',')
    {
      var lines = new List<string>();
      var sf = "";
      var quotes = false;
      var squares = 0;
      var brackets = 0;
      foreach (var c in text)
      {
        if (c == '\'') quotes = !quotes;
        else if (c == '[') squares++;
        else if (c == ']') squares--;
        else if (c == '(') brackets++;
        else if (c == ')') brackets--;
        else if (c == separator && !quotes && squares == 0 && brackets == 0)
        {
          lines.Add(sf);
          sf = "";
        }
        if (c != separator || keepSeparator) sf += c;
      }
      sf = sf.Trim();
      if (sf.StartsWith(Convert.ToString(separator))) sf = sf.Substring(1);
      sf = sf.Trim();
      if (!string.IsNullOrEmpty(sf))
      {
        lines.Add(sf);
      }
      return lines;
    }

    public static bool IsNumericType(this Column col)
    {
      return new[] { "bit", "decima", "numeric", "float", "real", "int", "bigint", "smallint", "tinyint", "money", "smallmoney" }.Any(x => x.Equals(col.Type, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> FilterByTable<T>(this IEnumerable<T> tableBases, Table table) where T : TableBase
    {
      return tableBases.Where(c => table.Schema.Equals(c.TableSchema, StringComparison.OrdinalIgnoreCase) && table.Name.Equals(c.TableName, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<Tuple<T, T>> FilterByTable<T>(this IEnumerable<Tuple<T, T>> tableBases, Table table) where T : TableBase
    {
      return tableBases.Where(c => table.Schema.Equals(c.Item1.TableSchema, StringComparison.OrdinalIgnoreCase) && table.Name.Equals(c.Item1.TableName, StringComparison.OrdinalIgnoreCase));
    }

    public static void RemoveDefault(this Column column)
    {
      if (!string.IsNullOrEmpty(column.Type))
      {
        if (column.Type.Equals("float", StringComparison.OrdinalIgnoreCase) && column.MaxLength == 53)
        {
          column.MaxLength = 0;
        }
        else if (column.Type.Equals("ntext", StringComparison.OrdinalIgnoreCase) && column.MaxLength == 8)
        {
          column.MaxLength = 0;
        }
        else if (column.Type.Equals("text", StringComparison.OrdinalIgnoreCase) && column.MaxLength == 16)
        {
          column.MaxLength = 0;
        }
      }
    }

    public static void RemoveDefaults(this IEnumerable<Column> columns)
    {
      foreach (var c in columns)
      {
        c.RemoveDefault();
      }
    }
  }
}