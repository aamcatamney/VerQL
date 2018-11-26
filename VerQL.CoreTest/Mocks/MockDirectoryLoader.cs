using VerQL.Core.Loaders;
using VerQL.Core.Models;

namespace VerQL.CoreTest
{
    public class MockDirectoryLoader : DirectoryLoader
    {
        public MockDirectoryLoader() : base("FAKEPATH") { }

        public Table TestProcessTable(string sql)
        {
            return ProcessTable(sql);
        }

        public PrimaryKeyConstraint TestProcessPrimaryKey(string sql)
        {
            return ProcessPrimaryKey(sql);
        }
    }
}