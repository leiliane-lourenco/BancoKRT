namespace BancoKRT.GestaoLimite.Application.Limites.Dtos;

/// <summary>Dados para cadastrar um novo registro de conta (req. 2.1): limite diário de PIX + saldo.</summary>
public record CriarLimiteRequest(string Documento, string NumeroAgencia, string NumeroConta, decimal LimitePix, decimal Saldo);

/// <summary>Dados para alterar o limite diário de PIX de uma conta existente (req. 2.3).</summary>
public record AlterarLimiteRequest(decimal LimitePix);

/// <summary>
/// Representação de leitura de uma conta. <see cref="LimitePix"/> é o teto diário (fixo);
/// <see cref="LimiteDisponivel"/> é o quanto ainda resta hoje (teto − PIX já feitos no dia).
/// </summary>
public record LimiteDto(Guid Id, string Documento, string NumeroAgencia, string NumeroConta, decimal LimitePix, decimal Saldo, decimal LimiteDisponivel);
