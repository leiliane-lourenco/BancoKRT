using BancoKRT.GestaoLimite.Domain.Exceptions;
using BancoKRT.GestaoLimite.Domain.Validation;
// Alias evita conflito entre o tipo validador e a propriedade Documento da entidade.
using DocumentoValidador = BancoKRT.GestaoLimite.Domain.Validation.Documento;

namespace BancoKRT.GestaoLimite.Domain.Entities;

public enum ResultadoTransacaoPix
{
    Aprovada,
    SaldoInsuficiente,
    LimiteDiarioExcedido
}

public class ContaLimite
{
    public Guid Id { get; private set; }
    public string Documento { get; private set; } = string.Empty;
    public string NumeroAgencia { get; private set; } = string.Empty;
    public string NumeroConta { get; private set; } = string.Empty;

    public decimal LimitePix { get; private set; }

    public decimal Saldo { get; private set; }

    public decimal TotalPixDia { get; private set; }

    public DateOnly? DataUltimaTransacaoPix { get; private set; }

    private ContaLimite() { }

    public ContaLimite(string documento, string numeroAgencia, string numeroConta, decimal limitePix, decimal saldo)
    {
        documento = DocumentoValidador.Normalizar(documento);
        numeroAgencia = (numeroAgencia ?? string.Empty).Trim();
        numeroConta = (numeroConta ?? string.Empty).Trim();

        if (!DocumentoValidador.EhValido(documento))
            throw new DomainException("Documento (CPF/CNPJ) inválido.");
        if (string.IsNullOrWhiteSpace(numeroAgencia))
            throw new DomainException("Número da agência é obrigatório.");
        if (string.IsNullOrWhiteSpace(numeroConta))
            throw new DomainException("Número da conta é obrigatório.");
        if (limitePix < 0)
            throw new DomainException("O limite PIX não pode ser negativo.");
        if (saldo < 0)
            throw new DomainException("O saldo não pode ser negativo.");

        Id = Guid.NewGuid();
        Documento = documento;
        NumeroAgencia = numeroAgencia;
        NumeroConta = numeroConta;
        LimitePix = limitePix;
        Saldo = saldo;
    }

    
    public static ContaLimite Reconstituir(
        Guid id, string documento, string numeroAgencia, string numeroConta,
        decimal limitePix, decimal saldo, decimal totalPixDia, DateOnly? dataUltimaTransacaoPix)
        => new()
        {
            Id = id,
            Documento = documento,
            NumeroAgencia = numeroAgencia,
            NumeroConta = numeroConta,
            LimitePix = limitePix,
            Saldo = saldo,
            TotalPixDia = totalPixDia,
            DataUltimaTransacaoPix = dataUltimaTransacaoPix
        };

    public void AlterarLimite(decimal novoLimite)
    {
        if (novoLimite < 0)
            throw new DomainException("O limite PIX não pode ser negativo.");

        LimitePix = novoLimite;
    }

    public decimal LimiteDisponivelNoDia(DateOnly data)
        => LimitePix - TotalPixNoDia(data);

    public decimal TotalPixNoDia(DateOnly data)
        => DataUltimaTransacaoPix == data ? TotalPixDia : 0m;
    
    public ResultadoTransacaoPix RealizarPix(decimal valor, DateOnly dataTransacao)
    {
        if (valor <= 0)
            throw new DomainException("O valor da transação deve ser maior que zero.");

        if (valor > Saldo)
            return ResultadoTransacaoPix.SaldoInsuficiente;

        var totalDia = TotalPixNoDia(dataTransacao);
        if (totalDia + valor > LimitePix)
            return ResultadoTransacaoPix.LimiteDiarioExcedido;

        Saldo -= valor;
        TotalPixDia = totalDia + valor;
        DataUltimaTransacaoPix = dataTransacao;
        return ResultadoTransacaoPix.Aprovada;
    }
}
