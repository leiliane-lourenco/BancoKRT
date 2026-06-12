using BancoKRT.GestaoLimite.Application.Abstractions;
using BancoKRT.GestaoLimite.Application.Common;
using BancoKRT.GestaoLimite.Application.Limites.Dtos;
using BancoKRT.GestaoLimite.Domain.Entities;
using BancoKRT.GestaoLimite.Domain.Exceptions;
using BancoKRT.GestaoLimite.Domain.Validation;

namespace BancoKRT.GestaoLimite.Application.Limites;

public class LimiteService : ILimiteService
{
    private readonly IContaLimiteRepository _repositorio;

    public LimiteService(IContaLimiteRepository repositorio) => _repositorio = repositorio;

    public async Task<Result<LimiteDto>> CriarAsync(CriarLimiteRequest request, CancellationToken ct = default)
    {
        var documento = Documento.Normalizar(request.Documento);
        var agencia = (request.NumeroAgencia ?? string.Empty).Trim();
        var conta = (request.NumeroConta ?? string.Empty).Trim();

        if (await _repositorio.ExisteAsync(documento, agencia, conta, ct))
            return Result<LimiteDto>.Conflito("Já existe um registro de limite para esta conta (documento + agência + conta).");

        try
        {
            var entidade = new ContaLimite(documento, agencia, conta, request.LimitePix, request.Saldo);
            await _repositorio.AdicionarAsync(entidade, ct);
            await _repositorio.SalvarAsync(ct);
            return Result<LimiteDto>.Sucesso(Mapear(entidade));
        }
        catch (DomainException ex)
        {
            return Result<LimiteDto>.Invalido(ex.Message);
        }
    }

    public async Task<Result<LimiteDto>> ObterAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
    {
        var entidade = await _repositorio.ObterAsync(Documento.Normalizar(documento), (numeroAgencia ?? "").Trim(), (numeroConta ?? "").Trim(), ct);
        return entidade is null
            ? Result<LimiteDto>.NaoEncontrado("Conta não encontrada.")
            : Result<LimiteDto>.Sucesso(Mapear(entidade));
    }

    public async Task<IReadOnlyList<LimiteDto>> ListarPorDocumentoAsync(string documento, CancellationToken ct = default)
    {
        var lista = await _repositorio.ListarPorDocumentoAsync(Documento.Normalizar(documento), ct);
        return lista.Select(Mapear).ToList();
    }

    public async Task<Result<LimiteDto>> AlterarAsync(string documento, string numeroAgencia, string numeroConta, AlterarLimiteRequest request, CancellationToken ct = default)
    {
        var entidade = await _repositorio.ObterAsync(Documento.Normalizar(documento), (numeroAgencia ?? "").Trim(), (numeroConta ?? "").Trim(), ct);
        if (entidade is null)
            return Result<LimiteDto>.NaoEncontrado("Conta não encontrada.");

        try
        {
            entidade.AlterarLimite(request.LimitePix);
            await _repositorio.SalvarAsync(ct);
            return Result<LimiteDto>.Sucesso(Mapear(entidade));
        }
        catch (DomainException ex)
        {
            return Result<LimiteDto>.Invalido(ex.Message);
        }
    }

    public async Task<Result> RemoverAsync(string documento, string numeroAgencia, string numeroConta, CancellationToken ct = default)
    {
        var entidade = await _repositorio.ObterAsync(Documento.Normalizar(documento), (numeroAgencia ?? "").Trim(), (numeroConta ?? "").Trim(), ct);
        if (entidade is null)
            return Result.NaoEncontrado("Conta não encontrada.");

        _repositorio.Remover(entidade);
        await _repositorio.SalvarAsync(ct);
        return Result.Sucesso();
    }

    private static LimiteDto Mapear(ContaLimite c)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        return new(c.Id, c.Documento, c.NumeroAgencia, c.NumeroConta, c.LimitePix, c.Saldo, c.LimiteDisponivelNoDia(hoje));
    }
}
