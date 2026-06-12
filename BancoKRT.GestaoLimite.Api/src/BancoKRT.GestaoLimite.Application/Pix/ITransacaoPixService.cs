using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Pix.Dtos;

namespace BancoKRT.GestaoLimite.Application.Pix;

public interface ITransacaoPixService
{
    /// <summary>
    /// Avalia a transação PIX contra o limite da conta. Aprova e debita quando houver limite;
    /// nega sem consumir quando o valor exceder. Retorna NaoEncontrado se a conta não existir.
    /// </summary>
    Task<Result<TransacaoPixResultado>> ProcessarAsync(TransacaoPixRequest request, CancellationToken ct = default);
}
