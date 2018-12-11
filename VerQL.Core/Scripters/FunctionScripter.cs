using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class FunctionScripter
    {
        public string ScriptCreate(Function function)
        {
            return function.Definition;
        }
    }
}