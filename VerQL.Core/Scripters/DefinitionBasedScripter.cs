using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Scripters
{
  public class DefinitionBasedScripter
  {
    private readonly Dictionary<string, string> vars;

    public DefinitionBasedScripter(Dictionary<string, string> vars = null)
    {
      this.vars = vars;
    }

    public string ScriptCreate(DefinitionBased definition)
    {
      return definition.ReplaceVars(vars);
    }

    public string ScriptAlter(DefinitionBased definition)
    {
      var def = definition.ReplaceVars(vars);
      var regex = Regex.Match(def, "^(?!--)(CREATE)(?=([^']*'[^']*')*[^']*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      if (regex.Success)
      {
        def = def.Remove(regex.Index, 6);
        def = def.Insert(regex.Index, "alter");
      }
      return def;
    }

    public string ScriptDrop(DefinitionBased definition)
    {
      return $"drop {definition.GetDefinitionBasedTypeName()} [{definition.Schema}].[{definition.Name}]";
    }
  }
}