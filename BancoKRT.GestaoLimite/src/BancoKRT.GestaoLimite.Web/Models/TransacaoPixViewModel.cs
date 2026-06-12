using System.ComponentModel.DataAnnotations;
using BancoKRT.GestaoLimite.Web.ApiClients.Models;

namespace BancoKRT.GestaoLimite.Web.Models;

public class BuscarContaPixViewModel
{
    [Required(ErrorMessage = "Informe o documento (CPF ou CNPJ).")]
    [Display(Name = "Documento (CPF/CNPJ)")]
    public string Documento { get; set; } = string.Empty;
}

public class SelecionarContaPixViewModel
{
    public string Documento { get; set; } = string.Empty;
    public IReadOnlyList<LimiteDto> Contas { get; set; } = new List<LimiteDto>();
}

public class TransacaoPixViewModel
{
    [Required(ErrorMessage = "Informe o documento (CPF ou CNPJ).")]
    [Display(Name = "Documento (CPF/CNPJ)")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o número da agência.")]
    [Display(Name = "Número da agência")]
    public string NumeroAgencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o número da conta.")]
    [Display(Name = "Número da conta")]
    public string NumeroConta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor da transação.")]
    [Range(0.01, 999999999999.99, ErrorMessage = "O valor deve ser maior que zero.")]
    [Display(Name = "Valor da transação (R$)")]
    public decimal Valor { get; set; }

    public decimal? LimiteConta { get; set; }

    public decimal? SaldoConta { get; set; }

    public bool Processado { get; set; }
    public bool Aprovada { get; set; }
    public string? Status { get; set; }
    public decimal? LimiteDisponivel { get; set; }
    public decimal? Saldo { get; set; }
    public string? Mensagem { get; set; }
}
