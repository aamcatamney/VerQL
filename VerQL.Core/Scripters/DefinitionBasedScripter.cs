using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class DefinitionBasedScripter
  {
    public string ScriptCreate(DefinitionBased definition)
    {
      return definition.Definition;
    }

    public string ScriptAlter(DefinitionBased definition)
    {
      var def = definition.Definition;
      var regex = Regex.Match(def, "^(?!--)(CREATE)(?=([^']*'[^']*')*[^']*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      if (regex.Success)
      {
        def = def.Remove(regex.Index, 6);
        def = def.Insert(regex.Index, "alter");
      }
      return def;
    }
  }
}