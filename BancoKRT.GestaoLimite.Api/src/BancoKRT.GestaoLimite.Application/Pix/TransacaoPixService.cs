using System.Collections.Concurrent;
using BancoKRT.GestaoLimite.Application.Abstractions;
using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Pix.Dtos;
using BancoKRT.GestaoLimite.Domain.Entities;
using BancoKRT.GestaoLimite.Domain.Validation;

namespace BancoKRT.GestaoLimite.Application.Pix;

public class TransacaoPixService : ITransacaoPixService
{
    private readonly IContaLimiteRepository _repositorio;

    // Lock por conta: evita gasto duplo de limite em transações simultâneas
    // (o provedor InMemory não oferece transação/lock de banco real).
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public TransacaoPixService(IContaLimiteRepository repositorio) => _repositorio = repositorio;

    public async Task<Result<TransacaoPixResultado>> ProcessarAsync(TransacaoPixRequest request, CancellationToken ct = default)
    {
        var documento = Documento.Normalizar(request.Documento);
        var agencia = (request.NumeroAgencia ?? string.Empty).Trim();
        var conta = (request.NumeroConta ?? string.Empty).Trim();

        if (request.Valor <= 0)
            return Result<TransacaoPixResultado>.Invalido("O valor da transação deve ser maior que zero.");

        var chave = $"{documento}|{agencia}|{conta}";
        var gate = _locks.GetOrAdd(chave, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(ct);
        try
        {
            var contaLimite = await _repositorio.ObterAsync(documento, agencia, conta, ct);
            if (contaLimite is null)
                return Result<TransacaoPixResultado>.NaoEncontrado("Conta não encontrada.");

            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var resultado = contaLimite.RealizarPix(request.Valor, hoje);

            // Só persiste quando a transação é efetivada (recusa de negócio não altera nada).
            if (resultado == ResultadoTransacaoPix.Aprovada)
                await _repositorio.SalvarAsync(ct);

            var limiteDia = contaLimite.LimiteDisponivelNoDia(hoje);
            var saldo = contaLimite.Saldo;

            return Result<TransacaoPixResultado>.Sucesso(resultado switch
            {
                ResultadoTransacaoPix.SaldoInsuficiente => TransacaoPixResultado.SaldoInsuficiente(limiteDia, saldo),
                ResultadoTransacaoPix.LimiteDiarioExcedido => TransacaoPixResultado.LimiteExcedido(limiteDia, saldo),
                _ => TransacaoPixResultado.Aprovado(limiteDia, saldo)
            });
        }
        finally
        {
            gate.Release();
        }
    }
}
