namespace BancoKRT.GestaoLimite.Application.Pix.Dtos;


public record TransacaoPixRequest(string Documento, string NumeroAgencia, string NumeroConta, decimal Valor);

/// <param name="Aprovada">Indica se a transação foi efetivada.</param>
/// <param name="Status">Mensagem de resultado exibida ao usuário (sucesso ou motivo da recusa).</param>
/// <param name="LimiteDisponivel">Limite de PIX ainda disponível no dia.</param>
/// <param name="Saldo">Saldo da conta após a avaliação.</param>
public record TransacaoPixResultado(bool Aprovada, string Status, decimal LimiteDisponivel, decimal Saldo, string Mensagem)
{
    public static TransacaoPixResultado Aprovado(decimal limiteDisponivelDia, decimal saldo) =>
        new(true, "Transferência enviada com Sucesso!", limiteDisponivelDia, saldo, "");

    public static TransacaoPixResultado SaldoInsuficiente(decimal limiteDisponivelDia, decimal saldo) =>
        new(false, "Saldo insuficiente para realizar a transferência.", limiteDisponivelDia, saldo, "");

    public static TransacaoPixResultado LimiteExcedido(decimal limiteDisponivelDia, decimal saldo) =>
        new(false, "Limite diário de PIX excedido. O valor ultrapassa o limite diário.", limiteDisponivelDia, saldo, "");
}
