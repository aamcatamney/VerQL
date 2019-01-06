using System;
using System.IO;
using Jil;
using VerQL.Core.Loaders;
using VerQL.Core.Models;
using VerQL.Core.Comparer;
using VerQL.Core.Scripters;
using VerQL.Core.Deployer;

namespace VerQL.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      var source = "SOME_SOURCE";
      var target = "SOME_TAGRET";
      var dep = new DatabaseDeployer(target, source);
      var tsk = dep.DeployAsync();
      tsk.Wait();
      var result = tsk.Result;
    }
  }
}
