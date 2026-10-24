namespace LowlandTech.Core.UseCases;

public static class UseCaseIdentity
{
    public static Guid GetId(Type useCaseType)
    {
        var assemblyName = useCaseType.Assembly.GetName().Name ?? string.Empty;
        var typeName = useCaseType.FullName ?? useCaseType.Name;
        return GuidV5.Create(GuidV5.Namespace, $"use-case:{assemblyName}:{typeName}");
    }
}
