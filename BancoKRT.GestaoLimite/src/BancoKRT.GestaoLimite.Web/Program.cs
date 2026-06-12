using System.Globalization;
using BancoKRT.GestaoLimite.Web.ApiClients;

// Cultura pt-BR: formulários e exibições usam vírgula como separador decimal ("1.000,00").
// O model binding interpreta os valores enviados pelos formulários nesse formato.
var culturaPtBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaPtBr;
CultureInfo.DefaultThreadCurrentUICulture = culturaPtBr;

var builder = WebApplication.CreateBuilder(args);

// MVC (Controllers + Views).
builder.Services.AddControllersWithViews();

// Clientes HTTP tipados para a API REST. A URL base vem de ApiSettings:BaseUrl.
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("Configuração 'ApiSettings:BaseUrl' não definida.");

builder.Services.AddHttpClient<LimitesApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<TransacoesPixApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Necessário para os testes de integração (WebApplicationFactory<Program>).
public partial class Program { }
