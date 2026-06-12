namespace BancoKRT.GestaoLimite.Web.ApiClients.Models;

/// <summary>Corpo enviado para cadastrar uma conta (POST /api/v1/limites).</summary>
public record CriarLimiteRequest(string Documento, string NumeroAgencia, string NumeroConta, decimal LimitePix, decimal Saldo);

/// <summary>Corpo enviado para alterar o limite diário (PUT /api/v1/limites/...).</summary>
public record AlterarLimiteRequest(decimal LimitePix);

/// <summary>Representação de leitura de uma conta retornada pela API.
/// LimitePix = teto diário; LimiteDisponivel = quanto ainda resta hoje.</summary>
public record LimiteDto(Guid Id, string Documento, string NumeroAgencia, string NumeroConta, decimal LimitePix, decimal Saldo, decimal LimiteDisponivel = 0);
