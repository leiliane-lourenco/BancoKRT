using BancoKRT.GestaoLimite.Web.ApiClients;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;
using BancoKRT.GestaoLimite.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.GestaoLimite.Web.Controllers;

public class LimitesController : Controller
{
    private readonly LimitesApiClient _api;

    public LimitesController(LimitesApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index(string? documento)
    {
        ViewBag.Documento = documento;
        if (string.IsNullOrWhiteSpace(documento))
            return View(new List<LimiteDto>());

        var lista = await _api.ListarPorDocumentoAsync(documento);
        if (lista.Count == 0)
            ViewBag.Aviso = "Nenhuma conta encontrada para o documento informado.";

        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string documento, string numeroAgencia, string numeroConta)
    {
        var resultado = await _api.ObterAsync(documento, numeroAgencia, numeroConta);
        if (!resultado.Ok)
            return NotFound(resultado.Erro);

        return View(resultado.Valor);
    }

    [HttpGet]
    public IActionResult Create(string? documento, string? numeroAgencia, string? numeroConta)
        => View(new CriarLimiteViewModel
        {
            Documento = documento ?? string.Empty,
            NumeroAgencia = numeroAgencia ?? string.Empty,
            NumeroConta = numeroConta ?? string.Empty
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarLimiteViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _api.CriarAsync(
            new CriarLimiteRequest(model.Documento, model.NumeroAgencia, model.NumeroConta, model.LimitePix, model.Saldo));

        if (resultado.Ok)
        {
            TempData["Sucesso"] = "Cadastro de limite realizado com sucesso.";
            return RedirectToAction(nameof(Index), new { documento = resultado.Valor!.Documento });
        }

        ModelState.AddModelError(string.Empty, resultado.Erro ?? "Não foi possível cadastrar.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string documento, string numeroAgencia, string numeroConta)
    {
        var resultado = await _api.ObterAsync(documento, numeroAgencia, numeroConta);
        if (!resultado.Ok)
            return NotFound(resultado.Erro);

        var dto = resultado.Valor!;
        return View(new EditarLimiteViewModel
        {
            Documento = dto.Documento,
            NumeroAgencia = dto.NumeroAgencia,
            NumeroConta = dto.NumeroConta,
            LimitePix = dto.LimitePix
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarLimiteViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _api.AlterarAsync(
            model.Documento, model.NumeroAgencia, model.NumeroConta, new AlterarLimiteRequest(model.LimitePix));

        if (resultado.Ok)
        {
            TempData["Sucesso"] = "Limite alterado com sucesso.";
            return RedirectToAction(nameof(Index), new { documento = model.Documento });
        }

        if (resultado.Status == ApiStatus.NaoEncontrado)
            return NotFound(resultado.Erro);

        ModelState.AddModelError(string.Empty, resultado.Erro ?? "Não foi possível alterar.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string documento, string numeroAgencia, string numeroConta)
    {
        var resultado = await _api.ObterAsync(documento, numeroAgencia, numeroConta);
        if (!resultado.Ok)
            return NotFound(resultado.Erro);

        return View(resultado.Valor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string documento, string numeroAgencia, string numeroConta)
    {
        var resultado = await _api.RemoverAsync(documento, numeroAgencia, numeroConta);
        if (resultado.Status == ApiStatus.NaoEncontrado)
            return NotFound(resultado.Erro);

        TempData["Sucesso"] = "Registro removido com sucesso.";
        return RedirectToAction(nameof(Index), new { documento });
    }
}
