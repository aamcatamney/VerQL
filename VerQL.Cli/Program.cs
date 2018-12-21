using System;
using System.IO;
using Jil;
using VerQL.Core.Loaders;
using VerQL.Core.Models;
using VerQL.Core.Comparer;
using VerQL.Core.Scripters;

namespace VerQL.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      var DirLoader = new DirectoryLoader(@"C:\Users\AnthonyMcAtamney\source\repos\vf\master\Databases\VisInstruct");
      var DbLoader = new DatabaseLoader("server=.;uid=visualfactory;pwd=nomuda;database=VF_master_VisInstruct;");
      var dbresp = DbLoader.Load();
      var dirresp = DirLoader.Load();
      var com = new DatabaseComparer();
      var cresp = com.Compare(dbresp.Database, dirresp.Database);
      var script = new CompareScripter().ScriptCompareAsFile(cresp);
      File.WriteAllText(@"C:\Junk\script.sql", script);


      //var o = new Options(false, true);
      //File.WriteAllText("SOMEPATH", JSON.Serialize<Database>(resp.Database, o));
    }
  }
}
