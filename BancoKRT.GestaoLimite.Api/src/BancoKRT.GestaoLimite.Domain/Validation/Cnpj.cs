using System.Linq;

namespace BancoKRT.GestaoLimite.Domain.Validation;

public static class Cnpj
{
    /// <summary>Remove tudo que não for dígito.</summary>
    public static string Normalizar(string? documento)
        => new string((documento ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Valida um CNPJ (14 dígitos + dígitos verificadores). Aceita com ou sem máscara.</summary>
    public static bool EhValido(string? documento)
    {
        var cnpj = Normalizar(documento);

        if (cnpj.Length != 14)
            return false;

        // Rejeita sequências repetidas (000..., 111..., etc.)
        if (cnpj.Distinct().Count() == 1)
            return false;

        var numeros = cnpj.Select(c => c - '0').ToArray();

        int Digito(int quantidade)
        {
            // Pesos do cálculo do CNPJ (variam conforme o dígito calculado).
            var pesos = quantidade == 12
                ? new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }
                : new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var soma = 0;
            for (var i = 0; i < quantidade; i++)
                soma += numeros[i] * pesos[i];

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        return Digito(12) == numeros[12] && Digito(13) == numeros[13];
    }
}
