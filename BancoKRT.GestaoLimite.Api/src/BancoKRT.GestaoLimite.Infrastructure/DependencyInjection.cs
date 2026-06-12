using Amazon.DynamoDBv2;
using Amazon.Runtime;
using BancoKRT.GestaoLimite.Application.Abstractions;
using BancoKRT.GestaoLimite.Application.Limites;
using BancoKRT.GestaoLimite.Application.Pix;
using BancoKRT.GestaoLimite.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BancoKRT.GestaoLimite.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra a persistência (DynamoDB ou banco em memória), o repositório e os serviços de
    /// aplicação (Limite e PIX). Com <paramref name="usarBancoEmMemoria"/> = true, roda sem
    /// DynamoDB/Docker — útil para depurar localmente.
    /// </summary>
    public static IServiceCollection AddGestaoLimite(this IServiceCollection services, DynamoDbOptions options, bool usarBancoEmMemoria = false)
    {
        services.AddScoped<ILimiteService, LimiteService>();
        services.AddScoped<ITransacaoPixService, TransacaoPixService>();

        if (usarBancoEmMemoria)
        {
            // Singleton: mantém os dados em memória enquanto a API está no ar (sem Docker).
            services.AddSingleton<IContaLimiteRepository, InMemoryContaLimiteRepository>();
            return services;
        }

        services.AddSingleton(options);

        services.AddSingleton<IAmazonDynamoDB>(_ =>
        {
            var config = new AmazonDynamoDBConfig();

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                // DynamoDB Local: endpoint fixo + região apenas para assinatura.
                config.ServiceURL = options.ServiceUrl;
                config.AuthenticationRegion = options.Region;
            }
            else
            {
                // AWS real: resolve o endpoint pela região.
                config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(options.Region);
            }

            var credenciais = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
            return new AmazonDynamoDBClient(credenciais, config);
        });

        services.AddScoped<IContaLimiteRepository, ContaLimiteRepository>();

        return services;
    }
}
