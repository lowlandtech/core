
namespace LowlandTech.Domain.Abstractions;

public interface ITemplateService
{
	Task<List<Template>> ListAsync(CancellationToken ct = default);
	Task<Template?> GetAsync(Guid id, CancellationToken ct = default);
	Task<Template> CreateAsync(Template template, CancellationToken ct = default);
	Task<Template?> UpdateAsync(Template template, CancellationToken ct = default);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

	/// <summary>
	///     Validates a template version by rendering its Content with its Json and comparing to expected Text.
	///     Returns null if version not found. Throws InvalidOperationException if version has no test data.
	/// </summary>
	Task<TemplateValidationResult?> ValidateVersionAsync(Guid versionId, CancellationToken ct = default);
}