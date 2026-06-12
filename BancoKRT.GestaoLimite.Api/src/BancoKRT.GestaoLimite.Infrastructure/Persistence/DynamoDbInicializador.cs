using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace BancoKRT.GestaoLimite.Infrastructure.Persistence;

public static class DynamoDbInicializador
{
    public static async Task GarantirTabelaAsync(IAmazonDynamoDB client, string tabela, CancellationToken ct = default)
    {
        var existentes = await client.ListTablesAsync(ct);
        if (existentes.TableNames.Contains(tabela))
            return;

        await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = tabela,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions =
            [
                new AttributeDefinition("Documento", ScalarAttributeType.S),
                new AttributeDefinition("Conta", ScalarAttributeType.S)
            ],
            KeySchema =
            [
                new KeySchemaElement("Documento", KeyType.HASH),
                new KeySchemaElement("Conta", KeyType.RANGE)
            ]
        }, ct);

        // Aguarda a tabela ficar ACTIVE antes de liberar a aplicação.
        for (var tentativa = 0; tentativa < 30; tentativa++)
        {
            var descr = await client.DescribeTableAsync(tabela, ct);
            if (descr.Table.TableStatus == TableStatus.ACTIVE)
                return;
            await Task.Delay(500, ct);
        }
    }
}
