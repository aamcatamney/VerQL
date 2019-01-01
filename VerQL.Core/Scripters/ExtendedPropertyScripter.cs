using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class ExtendedPropertyScripter
  {
    public string ScriptCreate(ExtendedProperty ep)
    {
      var sb = new StringBuilder();
      sb.Append($"EXECUTE sp_addextendedproperty @name = N'{ep.Name}', @value = N'{ep.Value}', @level0type = N'{ep.Level0Type}', @level0name = N'{ep.Level0Name}'");

      if (!string.IsNullOrEmpty(ep.Level1Type))
      {
        sb.Append($", @level1type = N'{ep.Level1Type}', @level1name = N'{ep.Level1Name}'");
      }

      if (!string.IsNullOrEmpty(ep.Level2Type))
      {
        sb.Append($", @level2type = N'{ep.Level2Type}', @level2name = N'{ep.Level2Name}'");
      }
      sb.Append(";");
      return sb.ToString().Trim();
    }

    public string ScriptDrop(ExtendedProperty ep)
    {
      var sb = new StringBuilder();
      sb.Append($"EXECUTE sp_dropextendedproperty @name = N'{ep.Name}', @level0type = N'{ep.Level0Type}', @level0name = N'{ep.Level0Name}'");

      if (!string.IsNullOrEmpty(ep.Level1Type))
      {
        sb.Append($", @level1type = N'{ep.Level1Type}', @level1name = N'{ep.Level1Name}'");
      }

      if (!string.IsNullOrEmpty(ep.Level2Type))
      {
        sb.Append($", @level2type = N'{ep.Level2Type}', @level2name = N'{ep.Level2Name}'");
      }
      sb.Append(";");
      return sb.ToString().Trim();
    }
  }
}