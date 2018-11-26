using System;
using System.IO;
using Jil;
using VerQL.Core.Loaders;
using VerQL.Core.Models;

namespace VerQL.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var loader = new DirectoryLoader("SOMEPATH");
            var resp = loader.Load();
            var o = new Options(false, true);
            File.WriteAllText("SOMEPATH", JSON.Serialize<Database>(resp.Database, o));
        }
    }
}
