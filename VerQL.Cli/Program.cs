using System;
using System.IO;
using Jil;
using VerQL.Core.Loaders;
using VerQL.Core.Models;
using VerQL.Core.Comparer;

namespace VerQL.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var DirLoader = new DirectoryLoader(@"C:\Projects\visualfactory\Databases\VisInstruct");
            var DbLoader = new DatabaseLoader("Server=.;Database=VF_VisInstruct;Trusted_Connection=True;");
            var dbresp = DbLoader.Load();
            var dirresp = DirLoader.Load();
            var com = new DatabaseComparer();
            var cresp = com.Compare(dbresp.Database, dirresp.Database);


            //var o = new Options(false, true);
            //File.WriteAllText("SOMEPATH", JSON.Serialize<Database>(resp.Database, o));
        }
    }
}
