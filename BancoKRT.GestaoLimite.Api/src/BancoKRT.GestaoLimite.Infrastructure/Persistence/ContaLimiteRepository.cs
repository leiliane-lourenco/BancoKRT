using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BancoKRT.GestaoLimite.Application.Abstractions;
using BancoKRT.GestaoLimite.Domain.Entities;

namespace BancoKRT.GestaoLimite.Infrastructure.Persistence;

public class ContaLimiteRepository : IContaLimiteRepository
{
    private readonly IAmazonDynamoDB _client;
    private readonly string _tabela;

    private readonly Dictionary<string, ContaLimite> _rastreadas = new();
    private readonly HashSet<string> _removidas = new();

    public ContaLimiteRepository(IAmazonDynamoDB client, DynamoDbOptions options)
    {
        _client = client;
        _tabela = options.TableName;
    }

    public async Task<ContaLimite?> ObterAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
    {
        var resposta = await _client.GetItemAsync(new GetItemRequest
        {
            TableName = _tabela,
            Key = Chave(documento, numeroAgencia, numeroConta)
        }, ct);

        if (resposta.Item is null || resposta.Item.Count == 0)
            return null;

        var entidade = Mapear(resposta.Item);
        Rastrear(entidade);
        return entidade;
    }

    public async Task<IReadOnlyList<ContaLimite>> ListarPorDocumentoAsync(string documento, CancellationToken ct = default)
    {
        var resposta = await _client.QueryAsync(new QueryRequest
        {
            TableName = _tabela,
            KeyConditionExpression = "Documento = :doc",
            ExpressionAttributeValues = new() { [":doc"] = new AttributeValue { S = documento } }
        }, ct);

        var contas = resposta.Items.Select(Mapear).ToList();
        foreach (var c in contas)
            Rastrear(c);
        return contas;
    }

    public async Task<bool> ExisteAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
    {
        var resposta = await _client.GetItemAsync(new GetItemRequest
        {
            TableName = _tabela,
            Key = Chave(documento, numeroAgencia, numeroConta),
            ProjectionExpression = "Documento" // só a chave, mais leve
        }, ct);

        return resposta.Item is { Count: > 0 };
    }

    public Task AdicionarAsync(ContaLimite conta, CancellationToken ct = default)
    {
        Rastrear(conta);
        return Task.CompletedTask;
    }

    public void Remover(ContaLimite conta)
    {
        var chave = ChaveRastreio(conta.Documento, conta.NumeroAgencia, conta.NumeroConta);
        _rastreadas.Remove(chave);
        _removidas.Add(chave);
    }

    public async Task SalvarAsync(CancellationToken ct = default)
    {
        foreach (var conta in _rastreadas.Values)
        {
            await _client.PutItemAsync(new PutItemRequest
            {
                TableName = _tabela,
                Item = ParaItem(conta)
            }, ct);
        }

        foreach (var chave in _removidas)
        {
            var (documento, agencia, conta) = DesmembrarChave(chave);
            await _client.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = _tabela,
                Key = Chave(documento, agencia, conta)
            }, ct);
        }

        _removidas.Clear();
    }

    // ----- rastreio (unit-of-work) -----

    private void Rastrear(ContaLimite conta)
        => _rastreadas[ChaveRastreio(conta.Documento, conta.NumeroAgencia, conta.NumeroConta)] = conta;

    private static string ChaveRastreio(string documento, string agencia, string conta)
        => $"{documento}|{agencia}|{conta}";

    private static (string documento, string agencia, string conta) DesmembrarChave(string chave)
    {
        var partes = chave.Split('|');
        return (partes[0], partes[1], partes[2]);
    }

    // ----- mapeamento DynamoDB <-> domínio -----

    private static string Sk(string agencia, string conta) => $"{agencia}#{conta}";

    private static Dictionary<string, AttributeValue> Chave(string documento, string agencia, string conta) => new()
    {
        ["Documento"] = new AttributeValue { S = documento },
        ["Conta"] = new AttributeValue { S = Sk(agencia, conta) }
    };

    private static Dictionary<string, AttributeValue> ParaItem(ContaLimite c)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Documento"] = new AttributeValue { S = c.Documento },
            ["Conta"] = new AttributeValue { S = Sk(c.NumeroAgencia, c.NumeroConta) },
            ["Id"] = new AttributeValue { S = c.Id.ToString() },
            ["NumeroAgencia"] = new AttributeValue { S = c.NumeroAgencia },
            ["NumeroConta"] = new AttributeValue { S = c.NumeroConta },
            ["LimitePix"] = Num(c.LimitePix),
            ["Saldo"] = Num(c.Saldo),
            ["TotalPixDia"] = Num(c.TotalPixDia)
        };

        if (c.DataUltimaTransacaoPix is DateOnly data)
            item["DataUltimaTransacaoPix"] = new AttributeValue { S = data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };

        return item;
    }

    private static ContaLimite Mapear(Dictionary<string, AttributeValue> item)
    {
        DateOnly? dataUltima = item.TryGetValue("DataUltimaTransacaoPix", out var d) && d.S is not null
            ? DateOnly.ParseExact(d.S, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;

        return ContaLimite.Reconstituir(
            id: Guid.Parse(item["Id"].S),
            documento: item["Documento"].S,
            numeroAgencia: item["NumeroAgencia"].S,
            numeroConta: item["NumeroConta"].S,
            limitePix: Dec(item["LimitePix"]),
            saldo: Dec(item["Saldo"]),
            totalPixDia: Dec(item["TotalPixDia"]),
            dataUltimaTransacaoPix: dataUltima);
    }

    // Números no DynamoDB trafegam como string; cultura invariante para não virar vírgula (pt-BR).
    private static AttributeValue Num(decimal valor) => new() { N = valor.ToString(CultureInfo.InvariantCulture) };
    private static decimal Dec(AttributeValue av) => decimal.Parse(av.N, CultureInfo.InvariantCulture);
}
