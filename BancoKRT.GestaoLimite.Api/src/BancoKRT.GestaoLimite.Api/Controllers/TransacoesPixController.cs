using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Pix;
using BancoKRT.GestaoLimite.Application.Pix.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.GestaoLimite.Api.Controllers;

[ApiController]
[Route("api/v1/transacoes-pix")]
[Produces("application/json")]
public class TransacoesPixController : ControllerBase
{
    private readonly ITransacaoPixService _pixService;

    public TransacoesPixController(ITransacaoPixService pixService) => _pixService = pixService;

    [HttpPost]
    public async Task<IActionResult> Processar([FromBody] TransacaoPixRequest request)
    {
        var resultado = await _pixService.ProcessarAsync(request);
        return resultado.Status switch
        {
            ResultStatus.Sucesso => Ok(resultado.Valor),
            ResultStatus.NaoEncontrado => NotFound(new { erro = resultado.Erro }),
            _ => BadRequest(new { erro = resultado.Erro })
        };
    }
}
