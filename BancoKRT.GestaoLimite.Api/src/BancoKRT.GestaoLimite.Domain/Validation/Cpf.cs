using System.Linq;

namespace BancoKRT.GestaoLimite.Domain.Validation;

public static class Cpf
{
    /// <summary>Remove tudo que não for dígito.</summary>
    public static string Normalizar(string? documento)
        => new string((documento ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Valida um CPF (11 dígitos + dígitos verificadores). Aceita com ou sem máscara.</summary>
    public static bool EhValido(string? documento)
    {
        var cpf = Normalizar(documento);

        if (cpf.Length != 11)
            return false;

        // Rejeita sequências repetidas (000..., 111..., etc.)
        if (cpf.Distinct().Count() == 1)
            return false;

        var numeros = cpf.Select(c => c - '0').ToArray();

        int Digito(int quantidade)
        {
            var soma = 0;
            for (var i = 0; i < quantidade; i++)
                soma += numeros[i] * (quantidade + 1 - i);

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        return Digito(9) == numeros[9] && Digito(10) == numeros[10];
    }
}
