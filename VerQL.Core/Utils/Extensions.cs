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

    public static string SqlTrimWhiteSpace(this string Sql)
    {
      var trimmed = "";
      var singleQuotes = false;
      var doubleQuotes = false;
      var lastCharSpace = false;
      var openSquares = 0;
      foreach (var c in Sql)
      {
        if (c == '\'')
        {
          singleQuotes = !singleQuotes;
        }
        else if (c == '"')
        {
          doubleQuotes = !doubleQuotes;
        }
        else if (c == '[')
        {
          if (!singleQuotes && !doubleQuotes)
          {
            openSquares++;
          }
        }
        else if (c == ']')
        {
          if (!singleQuotes && !doubleQuotes)
          {
            openSquares--;
          }
        }

        if (singleQuotes ||
            doubleQuotes ||
            openSquares > 0 ||
          ((c != ' ' && c != '\t') || !lastCharSpace))
        {
          trimmed += c;
        }

        lastCharSpace = c == ' ' || c == '\t';
      }
      return trimmed;
    }

    public static string SqlTrimLines(this string Sql)
    {
      return string.Join("\r\n", Sql.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
    }

    public static string GetKey(this Base b)
    {
      return $"[{b.Schema}].[{b.Name}]";
    }

    public static string GetTableKey(this TableBase b)
    {
      return $"[{b.TableSchema}].[{b.TableName}]";
    }

    public static string TrimGoAndSemi(this string text)
    {
      var r = text.Trim();
      for (int i = 0; i < 4; i++)
      {
        if (r.EndsWith(";")) r = r.Remove(r.Length - 1).Trim();
        if (r.EndsWith("GO", StringComparison.OrdinalIgnoreCase)) r = r.Remove(r.Length - 2).Trim();
      }
      return r;
    }

    public static string RemoveNAndQuotes(this string text)
    {
      var r = text.Trim();
      if (r.StartsWith("N")) r = r.Remove(0, 1).Trim();
      if (r.StartsWith("'")) r = r.Remove(0, 1).Trim();
      if (r.EndsWith("'")) r = r.Remove(r.Length - 1).Trim();
      return r;
    }

    public static List<string> TrueSplit(this string text, char[] separators, bool keepSeparator = true)
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
        else if (separators.Contains(c) && !quotes && squares == 0 && brackets == 0)
        {
          if (!string.IsNullOrEmpty(sf))
          {
            lines.Add(sf);
            sf = "";
          }
        }
        if (!separators.Contains(c) || keepSeparator || quotes || squares > 0 || brackets > 0) sf += c;
      }
      sf = sf.Trim();
      if (sf.Length > 0 && separators.Contains(sf[0])) sf = sf.Substring(1);
      {
        sf = sf.Trim();
      }
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

    public static string ReplaceVars(this DefinitionBased definition, Dictionary<string, string> vars)
    {
      var result = definition.Definition;
      if (!string.IsNullOrEmpty(result) && vars != null)
      {
        foreach (var v in vars)
        {
          var wv = $"$({v.Key})";
          while (result.IndexOf(wv, StringComparison.OrdinalIgnoreCase) > -1)
          {
            var index = result.IndexOf(wv, StringComparison.OrdinalIgnoreCase);
            result = result.Insert(index, v.Value);
            result = result.Remove(index + v.Value.Length, wv.Length);
          }
        }
      }
      return result;
    }

    public static string GetDefinitionBasedTypeName(this DefinitionBased definition)
    {
      return definition.GetType().Name;
    }
  }
}