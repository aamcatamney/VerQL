namespace VerQL.Core.Scripters
{
  public class ScriptingOptions
  {
    public bool BlockOnPossibleDataLoss { get; set; } = true;
    public bool DropIndexesNotInSource { get; set; } = false;
    public bool DropViewsNotInSource { get; set; } = false;
    public bool DropProceduresNotInSource { get; set; } = false;
    public bool DropSchemaNotInSource { get; set; } = false;
  }
}