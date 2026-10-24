namespace LowlandTech.Core;

public interface IDataContractValidator
{
    Task<DataContractValidationResult> ValidateAsync(Guid containerItemId, CancellationToken ct = default);
    Task PreFillAsync(Guid containerItemId, CancellationToken ct = default);
}

public record DataContractValidationResult(bool IsValid, IReadOnlyList<DataContractViolation> Violations);
public record DataContractViolation(string RuleName, string ViolationType, string Message);
