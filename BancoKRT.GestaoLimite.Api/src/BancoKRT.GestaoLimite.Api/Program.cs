using System.Globalization;
using Amazon.DynamoDBv2;
using BancoKRT.GestaoLimite.Infrastructure;
using BancoKRT.GestaoLimite.Infrastructure.Persistence;

// Cultura pt-BR para formatação consistente (a API troca números em JSON, sempre invariante).
var culturaPtBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaPtBr;
CultureInfo.DefaultThreadCurrentUICulture = culturaPtBr;

var builder = WebApplication.CreateBuilder(args);

// Controllers da API REST.
builder.Services.AddControllers();

// Swagger / OpenAPI: documentação e tela interativa para testar os endpoints.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Banco KRT - Gestão de Limite", Version = "v1" });

    // Inclui os comentários XML (resumos/descrições) na tela do Swagger.
    var xml = Path.Combine(AppContext.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
        c.IncludeXmlComments(xml);
});

// Persistência + serviços de aplicação (Limite e PIX).
// UseInMemoryDatabase=true (definido no launchSettings em Debug) roda sem Docker/DynamoDB.
// Config do DynamoDB na seção "DynamoDb" (appsettings ou variáveis de ambiente DynamoDb__*).
var usarBancoEmMemoria = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
var dynamoOptions = builder.Configuration.GetSection("DynamoDb").Get<DynamoDbOptions>() ?? new DynamoDbOptions();
builder.Services.AddGestaoLimite(dynamoOptions, usarBancoEmMemoria);

var app = builder.Build();

// Só cria a tabela quando estiver usando DynamoDB (o banco em memória não precisa).
if (!usarBancoEmMemoria)
{
    using var scope = app.Services.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    await DynamoDbInicializador.GarantirTabelaAsync(client, dynamoOptions.TableName);
}

// Swagger habilitado em todos os ambientes para facilitar os testes manuais.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banco KRT - Gestão de Limite v1");
    // Swagger na raiz (http://localhost:5180/) -> abre direto ao iniciar.
    c.RoutePrefix = string.Empty;
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

// Necessário para os testes de integração (WebApplicationFactory<Program>).
public partial class Program { }
