using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Limites;
using BancoKRT.GestaoLimite.Application.Limites.Dtos;
using BancoKRT.GestaoLimite.Application.Pix;
using BancoKRT.GestaoLimite.Application.Pix.Dtos;
using BancoKRT.GestaoLimite.Tests.Fakes;

namespace BancoKRT.GestaoLimite.Tests.Application;

public class TransacaoPixServiceTests
{
    private const string Cpf = "529.982.247-25";

    private static (LimiteService limite, TransacaoPixService pix) CriarServicos()
    {
        var repo = new FakeContaLimiteRepository();
        return (new LimiteService(repo), new TransacaoPixService(repo));
    }

    [Fact]
    public async Task Pix_dentro_do_limite_e_do_saldo_aprova_e_debita_o_saldo()
    {
        var (limite, pix) = CriarServicos();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 1000m, 5000m));

        var r = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 300m));

        Assert.True(r.Ok);
        Assert.True(r.Valor!.Aprovada);
        Assert.Equal("Transferência enviada com Sucesso!", r.Valor.Status);
        Assert.Equal(700m, r.Valor.LimiteDisponivel); // 1000 - 300
        Assert.Equal(4700m, r.Valor.Saldo);           // 5000 - 300

        // confirma que o débito foi persistido
        var obter = await limite.ObterAsync(Cpf, "0001", "111");
        Assert.Equal(4700m, obter.Valor!.Saldo);
    }

    [Fact]
    public async Task Apos_pix_a_consulta_da_conta_reflete_saldo_e_limite_disponivel()
    {
        var (limite, pix) = CriarServicos();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 1000m, 5000m));

        await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 300m));

        var obter = await limite.ObterAsync(Cpf, "0001", "111");
        Assert.Equal(4700m, obter.Valor!.Saldo);            // saldo abateu
        Assert.Equal(700m, obter.Valor.LimiteDisponivel);   // limite disponível abateu (1000 - 300)
        Assert.Equal(1000m, obter.Valor.LimitePix);         // teto diário continua igual
    }

    [Fact]
    public async Task Pix_acima_do_saldo_nega_e_nao_debita()
    {
        var (limite, pix) = CriarServicos();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 1000m, 200m));

        var r = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 300m));

        Assert.True(r.Ok);
        Assert.False(r.Valor!.Aprovada);
        Assert.Equal("Saldo insuficiente para realizar a transferência.", r.Valor.Status);
        Assert.Equal(200m, r.Valor.Saldo); // intacto
    }

    [Fact]
    public async Task Pix_acima_do_limite_diario_nega_e_nao_debita()
    {
        var (limite, pix) = CriarServicos();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 100m, 5000m));

        var r = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 150m));

        Assert.True(r.Ok);
        Assert.False(r.Valor!.Aprovada);
        Assert.Contains("Limite diário", r.Valor.Status);
        Assert.Equal(5000m, r.Valor.Saldo); // intacto
    }

    [Fact]
    public async Task Dois_pix_no_dia_somam_e_o_segundo_pode_estourar_o_limite()
    {
        var (limite, pix) = CriarServicos();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 1000m, 5000m));

        var primeiro = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 700m));
        Assert.True(primeiro.Valor!.Aprovada);

        // 700 + 400 = 1100 > 1000 -> recusado por limite diário
        var segundo = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 400m));
        Assert.False(segundo.Valor!.Aprovada);
        Assert.Contains("Limite diário", segundo.Valor.Status);
        Assert.Equal(300m, segundo.Valor.LimiteDisponivel); // só os 700 contaram
    }

    [Fact]
    public async Task Pix_em_conta_inexistente_retorna_nao_encontrado()
    {
        var (_, pix) = CriarServicos();
        var r = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 50m));
        Assert.Equal(ResultStatus.NaoEncontrado, r.Status);
    }

    [Fact]
    public async Task Pix_com_valor_invalido_retorna_invalido()
    {
        var (limite, pix) = CriarServicos();
        await limite.CriarAsync(new CriarLimiteRequest(Cpf, "0001", "111", 100m, 5000m));

        var r = await pix.ProcessarAsync(new TransacaoPixRequest(Cpf, "0001", "111", 0m));
        Assert.Equal(ResultStatus.Invalido, r.Status);
    }
}
