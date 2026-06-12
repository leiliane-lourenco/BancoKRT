using System.Net;
using System.Net.Http.Json;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;

namespace BancoKRT.GestaoLimite.Web.ApiClients;

/// <summary>Cliente HTTP tipado para o recurso de limites/contas da API REST.</summary>
public class LimitesApiClient
{
    private const string BaseRota = "api/v1/limites";
    private readonly HttpClient _http;

    public LimitesApiClient(HttpClient http) => _http = http;

    /// <summary>Lista todas as contas de um documento (CPF/CNPJ).</summary>
    public async Task<IReadOnlyList<LimiteDto>> ListarPorDocumentoAsync(string documento, CancellationToken ct = default)
    {
        var lista = await _http.GetFromJsonAsync<List<LimiteDto>>($"{BaseRota}/{Uri.EscapeDataString(documento)}", ct);
        return lista ?? new List<LimiteDto>();
    }

    /// <summary>Obtém uma conta específica.</summary>
    public async Task<ApiResposta<LimiteDto>> ObterAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
    {
        var resposta = await _http.GetAsync(Rota(documento, numeroAgencia, numeroConta), ct);
        return await ApiResposta<LimiteDto>.DeAsync(resposta);
    }

    /// <summary>Cadastra uma nova conta.</summary>
    public async Task<ApiResposta<LimiteDto>> CriarAsync(CriarLimiteRequest request, CancellationToken ct = default)
    {
        var resposta = await _http.PostAsJsonAsync(BaseRota, request, ct);
        return await ApiResposta<LimiteDto>.DeAsync(resposta);
    }

    /// <summary>Altera o limite diário de PIX de uma conta.</summary>
    public async Task<ApiResposta<LimiteDto>> AlterarAsync(string documento, string numeroAgencia, string numeroConta, AlterarLimiteRequest request, CancellationToken ct = default)
    {
        var resposta = await _http.PutAsJsonAsync(Rota(documento, numeroAgencia, numeroConta), request, ct);
        return await ApiResposta<LimiteDto>.DeAsync(resposta);
    }

    /// <summary>Remove uma conta.</summary>
    public async Task<ApiResposta> RemoverAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
    {
        var resposta = await _http.DeleteAsync(Rota(documento, numeroAgencia, numeroConta), ct);
        return resposta.IsSuccessStatusCode
            ? ApiResposta.Sucesso()
            : ApiResposta.Falha(ApiResposta.MapearStatus(resposta.StatusCode), null);
    }

    private static string Rota(string documento, string agencia, string conta)
        => $"{BaseRota}/{Uri.EscapeDataString(documento)}/{Uri.EscapeDataString(agencia)}/{Uri.EscapeDataString(conta)}";
}
