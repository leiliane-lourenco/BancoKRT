using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Limites.Dtos;

namespace BancoKRT.GestaoLimite.Application.Limites;

public interface ILimiteService
{
    Task<Result<LimiteDto>> CriarAsync(CriarLimiteRequest request, CancellationToken ct = default);
    Task<Result<LimiteDto>> ObterAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default);
    Task<IReadOnlyList<LimiteDto>> ListarPorDocumentoAsync(string documento, CancellationToken ct = default);
    Task<Result<LimiteDto>> AlterarAsync(string documento, string numeroAgencia, string numeroConta, AlterarLimiteRequest request, CancellationToken ct = default);
    Task<Result> RemoverAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default);
}
