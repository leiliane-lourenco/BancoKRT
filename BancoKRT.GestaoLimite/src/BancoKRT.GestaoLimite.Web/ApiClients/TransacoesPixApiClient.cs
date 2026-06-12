using System.Net.Http.Json;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;

namespace BancoKRT.GestaoLimite.Web.ApiClients;

/// <summary>Cliente HTTP tipado para o recurso de transações PIX da API REST.</summary>
public class TransacoesPixApiClient
{
    private const string BaseRota = "api/v1/transacoes-pix";
    private readonly HttpClient _http;

    public TransacoesPixApiClient(HttpClient http) => _http = http;

    /// <summary>Processa (avalia e efetiva) uma transação PIX.</summary>
    public async Task<ApiResposta<TransacaoPixResultado>> ProcessarAsync(TransacaoPixRequest request, CancellationToken ct = default)
    {
        var resposta = await _http.PostAsJsonAsync(BaseRota, request, ct);
        return await ApiResposta<TransacaoPixResultado>.DeAsync(resposta);
    }
}
