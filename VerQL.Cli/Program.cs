using System;
using VerQL.Core.Loaders;

namespace VerQL.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var loader = new DirectoryLoader("SOME_PATH");
            loader.Load();
        }
    }
}
