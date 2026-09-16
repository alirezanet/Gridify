namespace Gridify;

public class GridifySelectFieldNotMappedException(string field)
   : GridifySelectException($"Field '{field}' is not mapped")
{
   public string Field { get; } = field;
}
