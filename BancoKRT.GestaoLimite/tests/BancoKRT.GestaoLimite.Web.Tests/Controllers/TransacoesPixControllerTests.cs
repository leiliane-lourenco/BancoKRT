using System.Net;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;
using BancoKRT.GestaoLimite.Web.Controllers;
using BancoKRT.GestaoLimite.Web.Models;
using BancoKRT.GestaoLimite.Web.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.GestaoLimite.Web.Tests.Controllers;

public class TransacoesPixControllerTests
{
    private const string Cpf = "52998224725";

    private static LimiteDto Conta(string ag = "0001", string conta = "111")
        => new(Guid.NewGuid(), Cpf, ag, conta, 1000m, 5000m, LimiteDisponivel: 600m);

    private static TransacoesPixController Criar(
        Func<HttpRequestMessage, HttpResponseMessage>? limites = null,
        Func<HttpRequestMessage, HttpResponseMessage>? pix = null)
    {
        var controller = new TransacoesPixController(
            Harness.Pix(pix ?? (_ => Harness.Json(HttpStatusCode.OK))),
            Harness.Limites(limites ?? (_ => Harness.Json(HttpStatusCode.OK))));
        return Harness.Wire(controller);
    }

    [Fact]
    public async Task Buscar_sem_contas_redireciona_para_o_cadastro()
    {
        var c = Criar(limites: _ => Harness.Json(HttpStatusCode.OK, new List<LimiteDto>()));

        var r = await c.Buscar(new BuscarContaPixViewModel { Documento = Cpf });

        var redirect = Assert.IsType<RedirectToActionResult>(r);
        Assert.Equal("Create", redirect.ActionName);
        Assert.Equal("Limites", redirect.ControllerName);
    }

    [Fact]
    public async Task Buscar_com_contas_redireciona_para_a_listagem_de_Limites()
    {
        var c = Criar(limites: _ => Harness.Json(HttpStatusCode.OK, new List<LimiteDto> { Conta() }));

        var r = await c.Buscar(new BuscarContaPixViewModel { Documento = Cpf });

        var redirect = Assert.IsType<RedirectToActionResult>(r);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Limites", redirect.ControllerName);
        Assert.Equal(Cpf, redirect.RouteValues!["documento"]);
    }

    [Fact]
    public async Task Buscar_com_model_invalido_volta_para_a_view_Simular()
    {
        var c = Criar();
        c.ModelState.AddModelError("Documento", "obrigatório");

        var r = await c.Buscar(new BuscarContaPixViewModel());

        var view = Assert.IsType<ViewResult>(r);
        Assert.Equal("Simular", view.ViewName);
    }

    [Fact]
    public async Task Transferir_get_de_conta_inexistente_redireciona_para_o_cadastro()
    {
        var c = Criar(limites: _ => Harness.Json(HttpStatusCode.NotFound, new { erro = "Conta não encontrada." }));

        var r = await c.Transferir(Cpf, "0001", "111");

        var redirect = Assert.IsType<RedirectToActionResult>(r);
        Assert.Equal("Create", redirect.ActionName);
        Assert.Equal("Limites", redirect.ControllerName);
    }

    [Fact]
    public async Task Transferir_get_de_conta_existente_retorna_view_com_saldo_e_limite()
    {
        var c = Criar(limites: _ => Harness.Json(HttpStatusCode.OK, Conta("0001", "111")));

        var r = await c.Transferir(Cpf, "0001", "111");

        var view = Assert.IsType<ViewResult>(r);
        var model = Assert.IsType<TransacaoPixViewModel>(view.Model);
        Assert.Equal(Cpf, model.Documento);
        Assert.Equal(600m, model.LimiteConta);
        Assert.Equal(5000m, model.SaldoConta);
    }
}
