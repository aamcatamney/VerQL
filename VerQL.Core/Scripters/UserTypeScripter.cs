using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class UserTypeScripter
    {
        public string ScriptCreate(UserType userType)
        {
            var sb = new StringBuilder();

            sb.Append($"CREATE TYPE [{userType.Schema}].[{userType.Name}] FROM [{userType.Type}] ");


            if (userType.MaxLength != 0)
            {
                if (userType.MaxLength == -1)
                {
                    sb.Append("(MAX) ");
                }
                else
                {
                    sb.Append($"({userType.MaxLength}) ");
                }
            }

            if (!userType.IsNullable)
            {
                sb.Append("NOT ");
            }

            sb.Append("NULL");

            return sb.ToString().Trim();
        }
    }
}