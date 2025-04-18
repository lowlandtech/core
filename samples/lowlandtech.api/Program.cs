using Lamar;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("plugins.json", optional: false, reloadOnChange: true);
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Host.UseLamar((context, services) =>
{
    services.AddLogging();
    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    // Load plugins from file
    services.AddPlugins();
    // Register specific plugin
    services.AddPlugin<Plugin3>();
    services.For<EntityService>().Use<EntityService>();
});

var app = builder.Build();
var scope = app.Services.CreateScope();
var container = scope.ServiceProvider.GetRequiredService<IContainer>();
container.UsePlugins(); // Configure plugins

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/plugins", () =>
{
    var plugins = scope.ServiceProvider.GetServices<IPlugin>(); ;
    return plugins;
})
.WithName("GetPlugins")
.WithOpenApi();

app.Run();
public partial class Program { }
