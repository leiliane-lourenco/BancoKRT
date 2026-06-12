using System.Globalization;
using BancoKRT.GestaoLimite.Web.ApiClients;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;
using BancoKRT.GestaoLimite.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.GestaoLimite.Web.Controllers;

public class TransacoesPixController : Controller
{
    private readonly TransacoesPixApiClient _pixApi;
    private readonly LimitesApiClient _limitesApi;

    public TransacoesPixController(TransacoesPixApiClient pixApi, LimitesApiClient limitesApi)
    {
        _pixApi = pixApi;
        _limitesApi = limitesApi;
    }

    [HttpGet]
    public IActionResult Simular() => View(new BuscarContaPixViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Buscar(BuscarContaPixViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Simular", model);

        var contas = await _limitesApi.ListarPorDocumentoAsync(model.Documento);

        if (contas.Count == 0)
        {
            TempData["Aviso"] = "Conta não encontrada. Cadastre uma nova conta..";
            return RedirectToAction("Create", "Limites", new { documento = model.Documento });
        }

        return RedirectToAction("Index", "Limites", new { documento = model.Documento });
    }

    [HttpGet]
    public async Task<IActionResult> Transferir(string documento, string numeroAgencia, string numeroConta)
    {
        var resultado = await _limitesApi.ObterAsync(documento, numeroAgencia, numeroConta);
        if (!resultado.Ok)
        {
            TempData["Aviso"] = "Conta não encontrada. Cadastre uma nova conta..";
            return RedirectToAction("Create", "Limites", new { documento, numeroAgencia, numeroConta });
        }

        var conta = resultado.Valor!;
        var model = new TransacaoPixViewModel
        {
            Documento = conta.Documento,
            NumeroAgencia = conta.NumeroAgencia,
            NumeroConta = conta.NumeroConta,
            LimiteConta = conta.LimitePix,
            SaldoConta = conta.Saldo
        };

        if (TempData["pix_status"] is string status)
        {
            model.Processado = true;
            model.Aprovada = TempData["pix_aprovada"] is true;
            model.Status = status;
            model.LimiteDisponivel = ParseDecimal(TempData["pix_limitedia"]);
            model.Saldo = ParseDecimal(TempData["pix_saldo"]);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transferir(TransacaoPixViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _pixApi.ProcessarAsync(
            new TransacaoPixRequest(model.Documento, model.NumeroAgencia, model.NumeroConta, model.Valor));

        if (resultado.Status == ApiStatus.NaoEncontrado)
        {
            TempData["Aviso"] = resultado.Erro ?? "Conta não encontrada. Cadastre uma nova conta..";
            return RedirectToAction("Create", "Limites", new
            {
                documento = model.Documento,
                numeroAgencia = model.NumeroAgencia,
                numeroConta = model.NumeroConta
            });
        }

        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Erro ?? "Não foi possível processar a transação.");
            return View(model);
        }

        var pix = resultado.Valor!;

        TempData["pix_status"] = pix.Status;
        TempData["pix_aprovada"] = pix.Aprovada;
        TempData["pix_limitedia"] = pix.LimiteDisponivel.ToString(CultureInfo.InvariantCulture);
        TempData["pix_saldo"] = pix.Saldo.ToString(CultureInfo.InvariantCulture);
        return RedirectToAction(nameof(Transferir), new
        {
            documento = model.Documento,
            numeroAgencia = model.NumeroAgencia,
            numeroConta = model.NumeroConta
        });
    }

    private static decimal? ParseDecimal(object? valor) =>
        valor is string s && decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
            ? d
            : null;
}
