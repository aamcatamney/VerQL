using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class ViewScripter
    {
        public string ScriptCreate(View view)
        {
            return view.Definition;
        }
    }
}