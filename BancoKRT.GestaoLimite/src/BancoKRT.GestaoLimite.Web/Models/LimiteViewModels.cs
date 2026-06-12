using System.ComponentModel.DataAnnotations;

namespace BancoKRT.GestaoLimite.Web.Models;

public class CriarLimiteViewModel
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

    [Required(ErrorMessage = "Informe o limite diário de PIX.")]
    [Range(0, 999999999999.99, ErrorMessage = "O limite diário de PIX deve ser zero ou positivo.")]
    [Display(Name = "Limite diário de PIX (R$)")]
    public decimal LimitePix { get; set; }

    [Required(ErrorMessage = "Informe o saldo em conta.")]
    [Range(0, 999999999999.99, ErrorMessage = "O saldo deve ser zero ou positivo.")]
    [Display(Name = "Saldo em conta (R$)")]
    public decimal Saldo { get; set; }
}

public class EditarLimiteViewModel
{
    [Display(Name = "Documento (CPF/CNPJ)")]
    public string Documento { get; set; } = string.Empty;

    [Display(Name = "Número da agência")]
    public string NumeroAgencia { get; set; } = string.Empty;

    [Display(Name = "Número da conta")]
    public string NumeroConta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o limite diário de PIX.")]
    [Range(0, 999999999999.99, ErrorMessage = "O limite diário de PIX deve ser zero ou positivo.")]
    [Display(Name = "Novo limite diário de PIX (R$)")]
    public decimal LimitePix { get; set; }
}
