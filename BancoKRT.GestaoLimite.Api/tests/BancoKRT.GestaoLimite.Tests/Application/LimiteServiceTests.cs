using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Limites;
using BancoKRT.GestaoLimite.Application.Limites.Dtos;
using BancoKRT.GestaoLimite.Tests.Fakes;

namespace BancoKRT.GestaoLimite.Tests.Application;

public class LimiteServiceTests
{
    private const string Cpf = "529.982.247-25";

    private static LimiteService NovoServico() => new(new FakeContaLimiteRepository());

    [Fact]
    public async Task Criar_e_obter_um_limite()
    {
        var limite = NovoServico();

        var criar = await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 1500m, 5000m));
        Assert.True(criar.Ok);
        Assert.Equal("52998224725", criar.Valor!.Documento);
        Assert.Equal(5000m, criar.Valor.Saldo);

        var obter = await limite.ObterAsync(Cpf, "0001", "111");
        Assert.True(obter.Ok);
        Assert.Equal(1500m, obter.Valor!.LimitePix);
    }

    [Fact]
    public async Task Criar_duplicado_retorna_conflito()
    {
        var limite = NovoServico();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 100m, 1000m));

        var dup = await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 200m, 2000m));
        Assert.Equal(ResultStatus.Conflito, dup.Status);
    }

    [Fact]
    public async Task Criar_com_cpf_invalido_retorna_invalido()
    {
        var limite = NovoServico();
        var r = await limite.CriarAsync(new CriarLimiteRequest("111.111.111-11", "0001", "111", 100m, 1000m));
        Assert.Equal(ResultStatus.Invalido, r.Status);
    }

    [Fact]
    public async Task Alterar_limite_existente()
    {
        var limite = NovoServico();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 100m, 1000m));

        var alt = await limite.AlterarAsync(Cpf, "0001", "111", new AlterarLimiteRequest(900m));
        Assert.True(alt.Ok);
        Assert.Equal(900m, alt.Valor!.LimitePix);
    }

    [Fact]
    public async Task Alterar_inexistente_retorna_nao_encontrado()
    {
        var limite = NovoServico();
        var alt = await limite.AlterarAsync(Cpf, "0001", "999", new AlterarLimiteRequest(900m));
        Assert.Equal(ResultStatus.NaoEncontrado, alt.Status);
    }

    [Fact]
    public async Task Remover_existente_e_depois_nao_encontra()
    {
        var limite = NovoServico();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 100m, 1000m));

        var rem = await limite.RemoverAsync(Cpf, "0001", "111");
        Assert.True(rem.Ok);

        var obter = await limite.ObterAsync(Cpf, "0001", "111");
        Assert.Equal(ResultStatus.NaoEncontrado, obter.Status);
    }

    [Fact]
    public async Task Listar_por_documento_retorna_todas_as_contas()
    {
        var limite = NovoServico();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 100m, 1000m));
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0002", "222", 200m, 2000m));

        var lista = await limite.ListarPorDocumentoAsync(Cpf);
        Assert.Equal(2, lista.Count);
    }
}
