using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using VerQL.Core.Loaders;

namespace VerQL.Core.Deployer
{
  public class DatabaseDeployer
  {
    private readonly string _target;
    private readonly string _source;
    public DatabaseDeployer(string TargetConnectionString, string Source)
    {
      this._target = TargetConnectionString;
      this._source = Source;
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
        var compare = new Comparer.DatabaseComparer().Compare(left.Database, right.Database);
        var scripts = new Scripters.CompareScripter().ScriptCompareAsStatments(compare);
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
      return resp;
    }
  }
}