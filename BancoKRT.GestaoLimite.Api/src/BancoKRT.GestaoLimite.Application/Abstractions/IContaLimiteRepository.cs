using BancoKRT.GestaoLimite.Domain.Entities;

namespace BancoKRT.GestaoLimite.Application.Abstractions;

/// <summary>Persistência de <see cref="ContaLimite"/>. Implementada na Infrastructure.</summary>
public interface IContaLimiteRepository
{
    Task<ContaLimite?> ObterAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default);
    Task<IReadOnlyList<ContaLimite>> ListarPorDocumentoAsync(string documento, CancellationToken ct = default);
    Task<bool> ExisteAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default);
    Task AdicionarAsync(ContaLimite conta, CancellationToken ct = default);
    void Remover(ContaLimite conta);
    Task SalvarAsync(CancellationToken ct = default);
}
