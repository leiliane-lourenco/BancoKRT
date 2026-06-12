using System.ComponentModel.DataAnnotations;
using BancoKRT.GestaoLimite.Web.Models;

namespace BancoKRT.GestaoLimite.Web.Tests.Models;

public class ViewModelValidationTests
{
    private static IList<ValidationResult> Validar(object model)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), resultados, validateAllProperties: true);
        return resultados;
    }

    [Fact]
    public void CriarLimiteViewModel_vazio_acusa_campos_obrigatorios()
    {
        var res = Validar(new CriarLimiteViewModel());

        Assert.Contains(res, r => r.MemberNames.Contains(nameof(CriarLimiteViewModel.Documento)));
        Assert.Contains(res, r => r.MemberNames.Contains(nameof(CriarLimiteViewModel.NumeroAgencia)));
        Assert.Contains(res, r => r.MemberNames.Contains(nameof(CriarLimiteViewModel.NumeroConta)));
    }

    [Fact]
    public void CriarLimiteViewModel_preenchido_e_valido()
    {
        var res = Validar(new CriarLimiteViewModel
        {
            Documento = "52998224725", NumeroAgencia = "0001", NumeroConta = "111", LimitePix = 1000m, Saldo = 5000m
        });

        Assert.Empty(res);
    }

    [Fact]
    public void CriarLimiteViewModel_limite_negativo_e_invalido()
    {
        var res = Validar(new CriarLimiteViewModel
        {
            Documento = "52998224725", NumeroAgencia = "0001", NumeroConta = "111", LimitePix = -1m, Saldo = 5000m
        });

        Assert.Contains(res, r => r.MemberNames.Contains(nameof(CriarLimiteViewModel.LimitePix)));
    }

    [Fact]
    public void TransacaoPixViewModel_valor_zero_e_invalido()
    {
        var res = Validar(new TransacaoPixViewModel
        {
            Documento = "52998224725", NumeroAgencia = "0001", NumeroConta = "111", Valor = 0m
        });

        Assert.Contains(res, r => r.MemberNames.Contains(nameof(TransacaoPixViewModel.Valor)));
    }

    [Fact]
    public void BuscarContaPixViewModel_sem_documento_e_invalido()
    {
        var res = Validar(new BuscarContaPixViewModel());

        Assert.Contains(res, r => r.MemberNames.Contains(nameof(BuscarContaPixViewModel.Documento)));
    }
}
