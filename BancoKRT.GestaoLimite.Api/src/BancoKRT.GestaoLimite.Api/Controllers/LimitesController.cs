using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Limites;
using BancoKRT.GestaoLimite.Application.Limites.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.GestaoLimite.Api.Controllers;

[ApiController]
[Route("api/v1/limites")]
[Produces("application/json")]
public class LimitesController : ControllerBase
{
    private readonly ILimiteService _limiteService;

    public LimitesController(ILimiteService limiteService) => _limiteService = limiteService;

    /// <summary>Cadastra uma nova conta com saldo e limite diário de PIX.</summary>  
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarLimiteRequest request)
    {
        var resultado = await _limiteService.CriarAsync(request);
        return resultado.Status switch
        {
            ResultStatus.Sucesso => CreatedAtAction(nameof(Obter),
                new { documento = resultado.Valor!.Documento, agencia = resultado.Valor.NumeroAgencia, conta = resultado.Valor.NumeroConta },
                resultado.Valor),
            ResultStatus.Conflito => Conflict(new { erro = resultado.Erro }),
            _ => BadRequest(new { erro = resultado.Erro })
        };
    }

    /// <summary>Lista todas as contas de um documento (CPF/CNPJ).</summary>
    [HttpGet("{documento}")]
    public async Task<IActionResult> ListarPorDocumento(string documento)
        => Ok(await _limiteService.ListarPorDocumentoAsync(documento));

    /// <summary>Obtém uma conta específica (documento + agência + conta).</summary>
    [HttpGet("{documento}/{agencia}/{conta}")]
    public async Task<IActionResult> Obter(string documento, string agencia, string conta)
    {
        var resultado = await _limiteService.ObterAsync(documento, agencia, conta);
        return resultado.Ok ? Ok(resultado.Valor) : NotFound(new { erro = resultado.Erro });
    }

    /// <summary>Altera o limite diário de PIX de uma conta existente.</summary>   
    [HttpPut("{documento}/{agencia}/{conta}")]
    public async Task<IActionResult> Alterar(string documento, string agencia, string conta, [FromBody] AlterarLimiteRequest request)
    {
        var resultado = await _limiteService.AlterarAsync(documento, agencia, conta, request);
        return resultado.Status switch
        {
            ResultStatus.Sucesso => Ok(resultado.Valor),
            ResultStatus.NaoEncontrado => NotFound(new { erro = resultado.Erro }),
            _ => BadRequest(new { erro = resultado.Erro })
        };
    }

    /// <summary>Remove uma conta (documento + agência + conta).</summary>
    [HttpDelete("{documento}/{agencia}/{conta}")]
    public async Task<IActionResult> Remover(string documento, string agencia, string conta)
    {
        var resultado = await _limiteService.RemoverAsync(documento, agencia, conta);
        return resultado.Ok ? NoContent() : NotFound(new { erro = resultado.Erro });
    }
}
