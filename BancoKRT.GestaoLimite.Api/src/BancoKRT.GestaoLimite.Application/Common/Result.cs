namespace BancoKRT.GestaoLimite.Application.Common;

public enum ResultStatus
{
    Sucesso,
    NaoEncontrado,
    Conflito,
    Invalido
}

/// <summary>Resultado de uma operação da camada de aplicação, sem usar exceções para fluxo.</summary>
public class Result
{
    public bool Ok => Status == ResultStatus.Sucesso;
    public ResultStatus Status { get; protected set; }
    public string? Erro { get; protected set; }

    protected Result(ResultStatus status, string? erro)
    {
        Status = status;
        Erro = erro;
    }

    public static Result Sucesso() => new(ResultStatus.Sucesso, null);
    public static Result NaoEncontrado(string erro = "Registro não encontrado.") => new(ResultStatus.NaoEncontrado, erro);
    public static Result Conflito(string erro) => new(ResultStatus.Conflito, erro);
    public static Result Invalido(string erro) => new(ResultStatus.Invalido, erro);
}

public class Result<T> : Result
{
    public T? Valor { get; private set; }

    private Result(ResultStatus status, string? erro, T? valor) : base(status, erro) => Valor = valor;

    public static Result<T> Sucesso(T valor) => new(ResultStatus.Sucesso, null, valor);
    public static new Result<T> NaoEncontrado(string erro = "Registro não encontrado.") => new(ResultStatus.NaoEncontrado, erro, default);
    public static new Result<T> Conflito(string erro) => new(ResultStatus.Conflito, erro, default);
    public static new Result<T> Invalido(string erro) => new(ResultStatus.Invalido, erro, default);
}
