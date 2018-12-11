using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class ProcedureScripter
    {
        public string ScriptCreate(Procedure procedure)
        {
            return procedure.Definition;
        }
    }
}