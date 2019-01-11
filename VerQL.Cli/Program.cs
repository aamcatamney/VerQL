using System;
using System.IO;
using Jil;
using VerQL.Core.Loaders;
using VerQL.Core.Models;
using VerQL.Core.Comparer;
using VerQL.Core.Scripters;
using VerQL.Core.Deployer;
using System.Collections.Generic;

namespace VerQL.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      var dl = new DirectoryLoader(@"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.3_Dev\Databases\VisRecord").Load();
      //var dl = new DirectoryLoader(@"C:\Junk\dbtest\").Load();

      var options = new ScriptingOptions();
      var vars = new Dictionary<string, string>()
      {
        ["VisInstruct"] = "VF_Verql_VisInstruct",
        ["VisLicenses"] = "VF_Verql_VisLicenses",
        ["VisRecord"] = "VF_Verql_VisRecord",
        ["VisRecord_Resources"] = "VF_Verql_VisRecord_Resources",
        ["VisReports"] = "VF_Verql_VisReports",
        ["VisTeam"] = "VF_Verql_VisTeam",
        ["VisLog"] = "VF_Verql_VisLog"
      };

      // var dbl = new DatabaseLoader(@"Server=.;Database=VF_Verql_VisLog;Trusted_Connection=True;").Load();
      var comp = new VerQL.Core.Comparer.DatabaseComparer();
      var resul = comp.Compare(new Database(), dl.Database);

      var script = new VerQL.Core.Scripters.CompareScripter(options, vars).ScriptCompareAsFile(resul);
      File.WriteAllText(@"C:\Junk\dbtest\test.sql", script);

      // DeployDBAsync("Server=.;Database=VF_Verql_VisLog;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisLog", options, vars);
      // DeployDBAsync("Server=.;Database=VF_Verql_VisInstruct;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisInstruct", options, vars);
      // DeployDBAsync("Server=.;Database=VF_Verql_VisTeam;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisTeam", options, vars);
      // DeployDBAsync("Server=.;Database=VF_Verql_VisLicenses;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisLicenses", options, vars);
      DeployDBAsync("Server=.;Database=VF_Verql_VisRecord;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisRecord", options, vars);
      // DeployDBAsync("Server=.;Database=VF_Verql_VisRecord_Resources;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisRecordResources", options, vars);
      // DeployDBAsync("Server=.;Database=VF_Verql_VisReports;Trusted_Connection=True;", @"C:\Users\AnthonyMcAtamney\source\repos\vf\4.18.4_Dev\Databases\VisReports", options, vars);
    }

    static void DeployDBAsync(string ConnString, string SourceDir, ScriptingOptions options, Dictionary<string, string> vars)
    {
      var deploy = new DatabaseDeployer(ConnString, SourceDir, options, vars);
      var tsk = deploy.DeployAsync();
      tsk.Wait();
    }
  }
}
