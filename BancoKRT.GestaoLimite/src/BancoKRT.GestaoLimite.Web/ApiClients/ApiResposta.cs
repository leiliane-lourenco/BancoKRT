using System.Net;
using System.Net.Http.Json;

namespace BancoKRT.GestaoLimite.Web.ApiClients;

/// <summary>Status de negócio derivado do código HTTP retornado pela API.</summary>
public enum ApiStatus
{
    Sucesso,
    NaoEncontrado,
    Conflito,
    Invalido,
    Erro
}

/// <summary>Resultado de uma chamada à API, sem valor de retorno.</summary>
public class ApiResposta
{
    public ApiStatus Status { get; protected set; }
    public string? Erro { get; protected set; }
    public bool Ok => Status == ApiStatus.Sucesso;

    public static ApiResposta Sucesso() => new() { Status = ApiStatus.Sucesso };
    public static ApiResposta Falha(ApiStatus status, string? erro) => new() { Status = status, Erro = erro };

    /// <summary>Mapeia um código HTTP de erro para o status de negócio correspondente.</summary>
    public static ApiStatus MapearStatus(HttpStatusCode codigo) => codigo switch
    {
        HttpStatusCode.NotFound => ApiStatus.NaoEncontrado,
        HttpStatusCode.Conflict => ApiStatus.Conflito,
        HttpStatusCode.BadRequest => ApiStatus.Invalido,
        _ => ApiStatus.Erro
    };

    /// <summary>Lê o corpo de erro padrão da API ({ "erro": "..." }).</summary>
    protected static async Task<string?> LerErroAsync(HttpResponseMessage resposta)
    {
        try
        {
            var corpo = await resposta.Content.ReadFromJsonAsync<ErroResposta>();
            return corpo?.Erro;
        }
        catch
        {
            return null;
        }
    }

    private sealed record ErroResposta(string? Erro);
}

/// <summary>Resultado de uma chamada à API com valor de retorno.</summary>
public class ApiResposta<T> : ApiResposta
{
    public T? Valor { get; private set; }

    public static ApiResposta<T> Sucesso(T? valor) => new() { Status = ApiStatus.Sucesso, Valor = valor };
    public static new ApiResposta<T> Falha(ApiStatus status, string? erro) => new() { Status = status, Erro = erro };

    /// <summary>Constrói a resposta a partir de uma <see cref="HttpResponseMessage"/>.</summary>
    public static async Task<ApiResposta<T>> DeAsync(HttpResponseMessage resposta)
    {
        if (resposta.IsSuccessStatusCode)
        {
            var valor = await resposta.Content.ReadFromJsonAsync<T>();
            return Sucesso(valor);
        }

        return Falha(MapearStatus(resposta.StatusCode), await LerErroAsync(resposta));
    }
}
