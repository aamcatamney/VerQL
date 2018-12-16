using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class DefinitionBasedScripter
    {
        public string ScriptCreate(DefinitionBased definition)
        {
            return definition.Definition;
        }
    }
}