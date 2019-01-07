using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VerQL.Core.Loaders;
using VerQL.Core.Scripters;

namespace VerQL.Core.Deployer
{
  public class DatabaseDeployer
  {
    private readonly Dictionary<string, string> vars;
    private readonly ScriptingOptions options;
    private readonly string _target;
    private readonly string _source;
    public DatabaseDeployer(string TargetConnectionString, string Source, ScriptingOptions options, Dictionary<string, string> vars = null)
    {
      this._target = TargetConnectionString;
      this._source = Source;
      this.vars = vars ?? new Dictionary<string, string>();
      this.options = options ?? new ScriptingOptions();
    }

    private LoaderResponse GetSource()
    {
      if (Directory.Exists(this._source))
      {
        var dl = new DirectoryLoader(_source);
        return dl.Load();
      }
      else
      {
        var lr = new LoaderResponse();
        lr.Errors.Add("Unknown source type");
        return lr;
      }
    }

    public async Task<DeployResponse> DeployAsync()
    {
      var resp = new DeployResponse();
      var left = new Loaders.DatabaseLoader(_target).Load();
      var right = GetSource();
      if (!left.Successful)
      {
        resp.Errors.AddRange(left.Errors);
      }
      if (!right.Successful)
      {
        resp.Errors.AddRange(right.Errors);
      }
      if (left.Successful && right.Successful)
      {
        var compare = new Comparer.DatabaseComparer(this.vars).Compare(left.Database, right.Database);
        var scripts = new Scripters.CompareScripter(this.options, this.vars).ScriptCompareAsStatments(compare);
        if (scripts != null && scripts.Any())
        {
          using (var conn = new SqlConnection(_target))
          {
            await conn.OpenAsync();
            using (var tran = conn.BeginTransaction())
            {
              try
              {
                foreach (var script in scripts)
                {
                  await conn.ExecuteAsync(script, commandType: CommandType.Text, commandTimeout: 0, transaction: tran);
                }
                tran.Commit();
              }
              catch (Exception er)
              {
                tran.Rollback();
                resp.Errors.Add(er.Message);
              }
            }
          }
        }
      }
      return resp;
    }
  }
}