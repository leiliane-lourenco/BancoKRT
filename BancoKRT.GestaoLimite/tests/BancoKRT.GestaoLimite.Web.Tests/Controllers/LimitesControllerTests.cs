using System.Collections;
using System.Net;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;
using BancoKRT.GestaoLimite.Web.Controllers;
using BancoKRT.GestaoLimite.Web.Models;
using BancoKRT.GestaoLimite.Web.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.GestaoLimite.Web.Tests.Controllers;

public class LimitesControllerTests
{
    private const string Cpf = "52998224725";

    private static LimiteDto Conta(string ag = "0001", string conta = "111")
        => new(Guid.NewGuid(), Cpf, ag, conta, 1000m, 5000m);

    private static LimitesController Criar(Func<HttpRequestMessage, HttpResponseMessage> responder)
        => Harness.Wire(new LimitesController(Harness.Limites(responder)));

    private static CriarLimiteViewModel ModeloValido() =>
        new() { Documento = Cpf, NumeroAgencia = "0001", NumeroConta = "111", LimitePix = 1000m, Saldo = 5000m };

    [Fact]
    public async Task Index_sem_documento_nao_chama_a_api_e_retorna_lista_vazia()
    {
        var c = Criar(_ => throw new InvalidOperationException("não deveria chamar a API"));

        var r = await c.Index(documento: null);

        var view = Assert.IsType<ViewResult>(r);
        Assert.Empty((IEnumerable)view.Model!);
    }

    [Fact]
    public async Task Index_com_documento_retorna_as_contas_encontradas()
    {
        var c = Criar(_ => Harness.Json(HttpStatusCode.OK, new List<LimiteDto> { Conta("0001", "111"), Conta("0002", "222") }));

        var r = await c.Index(Cpf);

        var view = Assert.IsType<ViewResult>(r);
        var lista = Assert.IsAssignableFrom<IReadOnlyList<LimiteDto>>(view.Model);
        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public async Task Create_post_com_model_invalido_retorna_a_mesma_view()
    {
        var c = Criar(_ => Harness.Json(HttpStatusCode.OK));
        c.ModelState.AddModelError("Documento", "obrigatório");

        var r = await c.Create(new CriarLimiteViewModel());

        Assert.IsType<ViewResult>(r);
    }

    [Fact]
    public async Task Create_post_com_sucesso_redireciona_para_o_Index()
    {
        var c = Criar(_ => Harness.Json(HttpStatusCode.Created, Conta()));

        var r = await c.Create(ModeloValido());

        var redirect = Assert.IsType<RedirectToActionResult>(r);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(Cpf, redirect.RouteValues!["documento"]);
        Assert.NotNull(c.TempData["Sucesso"]);
    }

    [Fact]
    public async Task Create_post_com_conflito_retorna_view_com_erro_no_modelstate()
    {
        var c = Criar(_ => Harness.Json(HttpStatusCode.Conflict, new { erro = "Já existe." }));

        var r = await c.Create(ModeloValido());

        Assert.IsType<ViewResult>(r);
        Assert.False(c.ModelState.IsValid);
    }

    [Fact]
    public async Task Edit_post_de_conta_inexistente_retorna_NotFound()
    {
        var c = Criar(_ => Harness.Json(HttpStatusCode.NotFound, new { erro = "não encontrada" }));

        var r = await c.Edit(new EditarLimiteViewModel
        {
            Documento = Cpf, NumeroAgencia = "0001", NumeroConta = "999", LimitePix = 900m
        });

        Assert.IsType<NotFoundObjectResult>(r);
    }
}
