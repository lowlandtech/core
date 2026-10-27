
using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Plugins;

/// <summary>
/// A plugin's contribution of one or more features. Plugins implement this interface to
/// declare which service contracts they expose.
/// SPEC2050 US03.
/// </summary>
public interface IFeaturePackage
{
    string PluginId { get; }
    void Register(IFeatureRegistrationBuilder builder);
}

/// <summary>
/// Builder API used inside <see cref="IFeaturePackage.Register"/> to declare features,
/// endpoint groups, background jobs, and MCP tools.
/// </summary>
public interface IFeatureRegistrationBuilder
{
    /// <summary>
    /// Add a feature binding the contract <typeparamref name="TContract"/> to the
    /// implementation <typeparamref name="TImplementation"/>.
    /// </summary>
    [RequiresUnreferencedCode(
        "Registering a feature binds the contract and implementation types into the service container and inspects the declared event subscriber types by reflection, so those types are not statically reachable and cannot be preserved by the trimmer.")]
    IFeatureRegistrationBuilder AddFeature<TContract, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(Action<FeatureBuilder>? configure = null)
        where TContract : class
        where TImplementation : class, TContract;

    /// <summary>Register an endpoint group contributed by the given feature.</summary>
    IFeatureRegistrationBuilder MapEndpoints<TGroup>(string featureId)
        where TGroup : EndpointGroup, new();

    /// <summary>Register a background job (IHostedService) contributed by the given feature.</summary>
    IFeatureRegistrationBuilder RegisterBackgroundJob<TJob>(string jobId, string featureId, string? displayName = null)
        where TJob : class;

    /// <summary>Register an MCP tool contributed by the given feature.</summary>
    IFeatureRegistrationBuilder RegisterTool<TImpl>(string toolName, string featureId, string? description = null)
        where TImpl : class;
}

/// <summary>Mutable builder collected during <see cref="IFeatureRegistrationBuilder.AddFeature{TContract,TImplementation}"/>.</summary>
public sealed class FeatureBuilder
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? SpecPath { get; set; }
    public string[] RequiredPermissions { get; set; } = [];
    public string[] RequiredRoles { get; set; } = [];
    public string[] RequiredPolicies { get; set; } = [];
    public string[] RequiredAuthenticationSchemes { get; set; } = [];
    public bool EnabledByDefault { get; set; } = true;
    public EdgeTypeDescriptor? EdgeType { get; set; }
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? SettingsType { get; set; }
    public PageTypeDescriptor[] Pages { get; set; } = [];
    public BlockTypeDescriptor[] Blocks { get; set; } = [];
    public Type[] BackgroundJobTypes { get; set; } = [];
    public Type[] EventSubscriberTypes { get; set; } = [];
    public Type[] McpToolTypes { get; set; } = [];
}

/// <summary>Default <see cref="IFeatureRegistrationBuilder"/> writing into an <see cref="IFeatureRegistry"/>.</summary>
public sealed class FeatureRegistrationBuilder(
    IFeatureRegistry registry,
    string pluginId,
    IPageTypeRegistry? pageRegistry = null,
    IBlockTypeRegistry? blockRegistry = null,
    IEndpointGroupRegistry? endpointRegistry = null,
    IJobRegistry? jobRegistry = null,
    IMcpToolRegistry? toolRegistry = null,
    IEventRouter? eventRouter = null,
    IPluginEdgeTypeRegistry? edgeTypeRegistry = null,
    IServiceCollection? services = null) : IFeatureRegistrationBuilder
{
    private static readonly Type EventHandlerOpenType = typeof(IEventHandler<>);

    [RequiresUnreferencedCode(
        "Registering a feature binds the contract and implementation types into the service container and inspects the declared event subscriber types by reflection, so those types are not statically reachable and cannot be preserved by the trimmer.")]
    public IFeatureRegistrationBuilder AddFeature<TContract, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(Action<FeatureBuilder>? configure = null)
        where TContract : class
        where TImplementation : class, TContract
    {
        var builder = new FeatureBuilder();
        configure?.Invoke(builder);

        if (string.IsNullOrWhiteSpace(builder.Id))
            throw new ArgumentException(
                $"Feature for contract '{typeof(TContract).FullName}' must declare an Id in its configuration.",
                nameof(configure));

        // SPEC2050 US03 UAC05 — bind TContract → TImplementation in DI so the contract
        // is resolvable from IServiceProvider after the host builds the container.
        services?.AddTransient(typeof(TContract), typeof(TImplementation));

        registry.Register(new FeatureDescriptor
        {
            Id = builder.Id,
            DisplayName = builder.DisplayName,
            Description = builder.Description,
            SpecPath = builder.SpecPath,
            PluginId = pluginId,
            ContractType = typeof(TContract),
            ImplementationType = typeof(TImplementation),
            RequiredPermissions = builder.RequiredPermissions,
            RequiredRoles = builder.RequiredRoles,
            RequiredPolicies = builder.RequiredPolicies,
            RequiredAuthenticationSchemes = builder.RequiredAuthenticationSchemes,
            EnabledByDefault = builder.EnabledByDefault,
            EdgeType = builder.EdgeType,
            SettingsType = builder.SettingsType,
            Pages = builder.Pages,
            Blocks = builder.Blocks,
            BackgroundJobTypes = builder.BackgroundJobTypes,
            EventSubscriberTypes = builder.EventSubscriberTypes,
            McpToolTypes = builder.McpToolTypes,
            IsEnabled = builder.EnabledByDefault,
        });

        if (builder.EdgeType is not null)
        {
            edgeTypeRegistry?.Register(builder.EdgeType with
            {
                FeatureId = string.IsNullOrWhiteSpace(builder.EdgeType.FeatureId) ? builder.Id : builder.EdgeType.FeatureId,
            });
        }

        foreach (var page in builder.Pages)
        {
            pageRegistry?.Register(page with
            {
                FeatureId = string.IsNullOrWhiteSpace(page.FeatureId) ? builder.Id : page.FeatureId,
            });
        }

        foreach (var block in builder.Blocks)
        {
            blockRegistry?.Register(block with
            {
                FeatureId = string.IsNullOrWhiteSpace(block.FeatureId) ? builder.Id : block.FeatureId,
            });
        }

        if (eventRouter is not null)
        {
            foreach (var handlerType in builder.EventSubscriberTypes)
            {
                foreach (var iface in handlerType.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    if (iface.GetGenericTypeDefinition() != EventHandlerOpenType) continue;

                    var eventType = iface.GetGenericArguments()[0];
                    // Declare the event type on behalf of the subscriber so Subscribe does not throw.
                    eventRouter.DeclareEvent(eventType);
                    eventRouter.Subscribe(new EventSubscriptionDescriptor(
                        FeatureId: builder.Id,
                        EventType: eventType,
                        HandlerType: handlerType));
                }
            }
        }

        return this;
    }

    public IFeatureRegistrationBuilder MapEndpoints<TGroup>(string featureId)
        where TGroup : EndpointGroup, new()
    {
        if (endpointRegistry is null) return this;
        var group = new TGroup();
        endpointRegistry.Register(new EndpointGroupDescriptor(
            FeatureId: featureId,
            GroupType: typeof(TGroup),
            GroupPrefix: group.GroupPrefix,
            Tags: group.Tags,
            RequiredAuthenticationScheme: group.RequiredAuthenticationScheme,
            RequiredPolicy: group.RequiredPolicy));
        return this;
    }

    public IFeatureRegistrationBuilder RegisterBackgroundJob<TJob>(string jobId, string featureId, string? displayName = null)
        where TJob : class
    {
        jobRegistry?.Register(new BackgroundJobDescriptor(jobId, featureId, typeof(TJob), displayName));
        return this;
    }

    public IFeatureRegistrationBuilder RegisterTool<TImpl>(string toolName, string featureId, string? description = null)
        where TImpl : class
    {
        toolRegistry?.Register(new McpToolDescriptor(toolName, featureId, typeof(TImpl), description));
        return this;
    }
}
