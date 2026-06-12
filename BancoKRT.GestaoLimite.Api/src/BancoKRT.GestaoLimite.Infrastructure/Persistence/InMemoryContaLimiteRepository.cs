using System.Collections.Concurrent;
using BancoKRT.GestaoLimite.Application.Abstractions;
using BancoKRT.GestaoLimite.Domain.Entities;

namespace BancoKRT.GestaoLimite.Infrastructure.Persistence;

/// <summary>
/// Repositório em memória, para rodar a API em Debug sem precisar subir o Docker/DynamoDB.
/// Registrado como Singleton: os dados sobrevivem entre as requisições enquanto a API está no ar
/// (mas são perdidos ao reiniciar). <c>ObterAsync</c> devolve a mesma instância armazenada, então
/// mutações (alterar limite, realizar PIX) refletem direto e <c>SalvarAsync</c> é no-op.
/// </summary>
public class InMemoryContaLimiteRepository : IContaLimiteRepository
{
    private readonly ConcurrentDictionary<string, ContaLimite> _contas = new();

    private static string Chave(string documento, string agencia, string conta) => $"{documento}|{agencia}|{conta}";

    public Task<ContaLimite?> ObterAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
        => Task.FromResult(_contas.GetValueOrDefault(Chave(documento, numeroAgencia, numeroConta)));

    public Task<IReadOnlyList<ContaLimite>> ListarPorDocumentoAsync(string documento, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ContaLimite>>(
            _contas.Values
                .Where(c => c.Documento == documento)
                .OrderBy(c => c.NumeroAgencia).ThenBy(c => c.NumeroConta)
                .ToList());

    public Task<bool> ExisteAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
        => Task.FromResult(_contas.ContainsKey(Chave(documento, numeroAgencia, numeroConta)));

    public Task AdicionarAsync(ContaLimite conta, CancellationToken ct = default)
    {
        _contas[Chave(conta.Documento, conta.NumeroAgencia, conta.NumeroConta)] = conta;
        return Task.CompletedTask;
    }

    public void Remover(ContaLimite conta)
        => _contas.TryRemove(Chave(conta.Documento, conta.NumeroAgencia, conta.NumeroConta), out _);

    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}
