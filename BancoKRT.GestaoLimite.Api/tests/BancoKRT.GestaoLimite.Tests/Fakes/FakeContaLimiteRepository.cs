using BancoKRT.GestaoLimite.Application.Abstractions;
using BancoKRT.GestaoLimite.Domain.Entities;

namespace BancoKRT.GestaoLimite.Tests.Fakes;

/// <summary>
/// Implementação em memória de <see cref="IContaLimiteRepository"/> para os testes de unidade
/// dos serviços — sem precisar de DynamoDB. Guarda as contas num dicionário pela chave de negócio
/// (documento + agência + conta); <c>ObterAsync</c> devolve a mesma instância armazenada, então
/// mutações (alterar limite, realizar PIX) refletem direto, e <c>SalvarAsync</c> é no-op.
/// </summary>
public class FakeContaLimiteRepository : IContaLimiteRepository
{
    private readonly Dictionary<string, ContaLimite> _contas = new();

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
        => _contas.Remove(Chave(conta.Documento, conta.NumeroAgencia, conta.NumeroConta));

    public Task SalvarAsync(CancellationToken ct = default) => Task.CompletedTask;
}
