using System.Net;
using System.Net.Http.Json;
using BancoKRT.GestaoLimite.Web.ApiClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BancoKRT.GestaoLimite.Web.Tests.TestSupport;

/// <summary>HttpMessageHandler que responde via função — simula a API REST sem rede.</summary>
public sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(responder(request));
}

/// <summary>Atalhos para montar clients de API, controllers e respostas HTTP nos testes.</summary>
public static class Harness
{
    public static HttpClient Http(Func<HttpRequestMessage, HttpResponseMessage> responder)
        => new(new StubHttpMessageHandler(responder)) { BaseAddress = new Uri("http://localhost/") };

    public static LimitesApiClient Limites(Func<HttpRequestMessage, HttpResponseMessage> responder)
        => new(Http(responder));

    public static TransacoesPixApiClient Pix(Func<HttpRequestMessage, HttpResponseMessage> responder)
        => new(Http(responder));

    /// <summary>Resposta HTTP com corpo JSON (camelCase, igual ao da API real).</summary>
    public static HttpResponseMessage Json(HttpStatusCode code, object? body = null)
    {
        var msg = new HttpResponseMessage(code);
        if (body is not null) msg.Content = JsonContent.Create(body);
        return msg;
    }

    /// <summary>Liga o contexto mínimo (HttpContext, TempData, ViewData) para testar o controller.</summary>
    public static T Wire<T>(T controller) where T : Controller
    {
        var http = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, new FakeTempDataProvider());
        controller.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), controller.ModelState);
        return controller;
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
