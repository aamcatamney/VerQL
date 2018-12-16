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

        public Procedure TestProcessProcedure(string sql)
        {
            return ProcessDefinitionBased<Procedure>(new Procedure(), "procedure", sql);
        }

        public Function TestProcessFunction(string sql)
        {
            return ProcessDefinitionBased<Function>(new Function(), "function", sql);
        }
    }
}