namespace LowlandTech.Core;

public interface IPostItemType
{
    string TypeName { get; }
    string DisplayName { get; }
    string Icon { get; }
    string Description { get; }
}
