using System.Linq;

namespace BancoKRT.GestaoLimite.Domain.Validation;


public static class Documento
{
    /// <summary>Remove tudo que não for dígito (máscara de CPF/CNPJ).</summary>
    public static string Normalizar(string? documento)
        => new string((documento ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Valida o documento como CPF (11 dígitos) ou CNPJ (14 dígitos).</summary>
    public static bool EhValido(string? documento)
    {
        var digitos = Normalizar(documento);
        return digitos.Length switch
        {
            11 => Cpf.EhValido(digitos),
            14 => Cnpj.EhValido(digitos),
            _ => false
        };
    }
}
