using System.Collections.Generic;
using System.Linq;

namespace VerQL.Core.Deployer
{
  public class DeployResponse
  {
    public List<string> Errors { get; set; } = new List<string>();
    public bool Successful
    {
      get { return !this.Errors.Any(); }
    }
  }
}