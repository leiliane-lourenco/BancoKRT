namespace BancoKRT.GestaoLimite.Web.ApiClients.Models;

/// <summary>Corpo enviado para processar um PIX (POST /api/v1/transacoes-pix).</summary>
public record TransacaoPixRequest(string Documento, string NumeroAgencia, string NumeroConta, decimal Valor);

/// <summary>Resultado de uma transação PIX retornado pela API.</summary>
public record TransacaoPixResultado(bool Aprovada, string Status, decimal LimiteDisponivel, decimal Saldo, string Mensagem);
