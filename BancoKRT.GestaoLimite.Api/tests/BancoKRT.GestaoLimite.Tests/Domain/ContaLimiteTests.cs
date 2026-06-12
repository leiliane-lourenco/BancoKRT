using BancoKRT.GestaoLimite.Domain.Entities;
using BancoKRT.GestaoLimite.Domain.Exceptions;

namespace BancoKRT.GestaoLimite.Tests.Domain;

public class ContaLimiteTests
{
    private static readonly DateOnly Hoje = new(2026, 6, 11);

    private static ContaLimite NovaConta(decimal limite = 1000m, decimal saldo = 5000m)
        => new("529.982.247-25", "0001", "12345-6", limite, saldo); // CPF válido

    [Fact]
    public void Deve_criar_conta_normalizando_o_cpf()
    {
        var conta = NovaConta();
        Assert.Equal("52998224725", conta.Documento);
        Assert.Equal(1000m, conta.LimitePix);
        Assert.Equal(5000m, conta.Saldo);
    }

    [Fact]
    public void Deve_rejeitar_cpf_invalido()
    {
        var ex = Assert.Throws<DomainException>(() => new ContaLimite("111.111.111-11", "0001", "1", 100m, 0m));
        Assert.Contains("CPF", ex.Message);
    }

    [Fact]
    public void Deve_rejeitar_limite_negativo_na_criacao()
        => Assert.Throws<DomainException>(() => new ContaLimite("529.982.247-25", "0001", "1", -1m, 0m));

    [Fact]
    public void Deve_rejeitar_saldo_negativo_na_criacao()
        => Assert.Throws<DomainException>(() => new ContaLimite("529.982.247-25", "0001", "1", 100m, -1m));

    [Fact]
    public void Pix_dentro_do_limite_e_do_saldo_aprova_debita_saldo_e_consome_limite_do_dia()
    {
        var conta = NovaConta(limite: 1000m, saldo: 5000m);

        var r = conta.RealizarPix(300m, Hoje);

        Assert.Equal(ResultadoTransacaoPix.Aprovada, r);
        Assert.Equal(4700m, conta.Saldo);                       // saldo é debitado
        Assert.Equal(1000m, conta.LimitePix);                   // teto diário não muda
        Assert.Equal(700m, conta.LimiteDisponivelNoDia(Hoje));  // 1000 - 300
    }

    [Fact]
    public void Pix_acima_do_saldo_retorna_saldo_insuficiente_e_nao_debita()
    {
        var conta = NovaConta(limite: 1000m, saldo: 200m);

        var r = conta.RealizarPix(300m, Hoje);

        Assert.Equal(ResultadoTransacaoPix.SaldoInsuficiente, r);
        Assert.Equal(200m, conta.Saldo);
    }

    [Fact]
    public void Pix_acima_do_limite_diario_retorna_excedido_e_nao_debita()
    {
        var conta = NovaConta(limite: 100m, saldo: 5000m);

        var r = conta.RealizarPix(150m, Hoje);

        Assert.Equal(ResultadoTransacaoPix.LimiteDiarioExcedido, r);
        Assert.Equal(5000m, conta.Saldo);
    }

    [Fact]
    public void Pix_com_valor_igual_ao_limite_aprova_e_zera_o_disponivel_do_dia()
    {
        var conta = NovaConta(limite: 100m, saldo: 5000m);

        var r = conta.RealizarPix(100m, Hoje);

        Assert.Equal(ResultadoTransacaoPix.Aprovada, r);
        Assert.Equal(0m, conta.LimiteDisponivelNoDia(Hoje));
    }

    [Fact]
    public void Dois_pix_no_mesmo_dia_somam_no_limite_diario()
    {
        var conta = NovaConta(limite: 1000m, saldo: 5000m);

        Assert.Equal(ResultadoTransacaoPix.Aprovada, conta.RealizarPix(700m, Hoje));
        // 700 + 400 = 1100 > 1000 -> excede o limite do dia
        Assert.Equal(ResultadoTransacaoPix.LimiteDiarioExcedido, conta.RealizarPix(400m, Hoje));
        Assert.Equal(300m, conta.LimiteDisponivelNoDia(Hoje)); // só os 700 contam
    }

    [Fact]
    public void Limite_diario_reinicia_no_dia_seguinte()
    {
        var conta = NovaConta(limite: 1000m, saldo: 5000m);
        conta.RealizarPix(1000m, Hoje);                         // consome todo o limite de hoje
        Assert.Equal(0m, conta.LimiteDisponivelNoDia(Hoje));

        var amanha = Hoje.AddDays(1);
        Assert.Equal(1000m, conta.LimiteDisponivelNoDia(amanha));            // reinicia
        Assert.Equal(ResultadoTransacaoPix.Aprovada, conta.RealizarPix(500m, amanha));
    }

    [Fact]
    public void Pix_com_valor_invalido_lanca()
    {
        var conta = NovaConta();
        Assert.Throws<DomainException>(() => conta.RealizarPix(0m, Hoje));
        Assert.Throws<DomainException>(() => conta.RealizarPix(-5m, Hoje));
    }

    [Fact]
    public void AlterarLimite_atualiza_e_rejeita_negativo()
    {
        var conta = NovaConta(100m);
        conta.AlterarLimite(2500m);
        Assert.Equal(2500m, conta.LimitePix);
        Assert.Throws<DomainException>(() => conta.AlterarLimite(-1m));
    }
}
