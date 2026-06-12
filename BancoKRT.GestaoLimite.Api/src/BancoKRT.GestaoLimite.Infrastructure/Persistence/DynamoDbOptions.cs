namespace BancoKRT.GestaoLimite.Infrastructure.Persistence;

/// <summary>Configuração de acesso ao DynamoDB (seção "DynamoDb" do appsettings/variáveis de ambiente).</summary>
public class DynamoDbOptions
{
    /// <summary>
    /// Endpoint do DynamoDB Local (ex.: http://localhost:8000). 
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    public string TableName { get; set; } = "ContasLimite";

    public string AccessKey { get; set; } = "local";
    public string SecretKey { get; set; } = "local";
}
