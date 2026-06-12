# Banco KRT — Sistema de Gestão de Limite PIX

Sistema para o analista de fraudes do Banco KRT **cadastrar e gerir o limite PIX** das contas e **avaliar transações PIX** contra esse limite.

- Aplicação **ASP.NET Core MVC** (C#, .NET 9) com **telas Razor** para o analista.
- **API REST JSON** (`/api/v1`) equivalente, para integração/automação.
- Persistência em **EF Core InMemory** (não requer banco externo — os dados são reiniciados a cada execução).

## Requisitos atendidos
| Req | Descrição | Onde |
|-----|-----------|------|
| 2.1 | Cadastrar (documento/CPF, agência, conta, limite PIX) — campos obrigatórios | Tela `Limites/Create` · `POST /api/v1/limites` |
| 2.2 | Buscar informações de limite de uma conta | Tela `Limites/Index` (busca por CPF) · `GET /api/v1/limites/{cpf}/{agencia}/{conta}` |
| 2.3 | Alterar o limite PIX | Tela `Limites/Edit` · `PUT /api/v1/limites/{cpf}/{agencia}/{conta}` |
| 2.4 | Remover um registro | Tela `Limites/Delete` · `DELETE /api/v1/limites/{cpf}/{agencia}/{conta}` |
| 2.5 | Avaliar transação PIX: aprova e debita se houver limite; **nega sem consumir** se exceder | Tela `TransacoesPix/Simular` · `POST /api/v1/transacoes-pix` |

A conta é identificada por **CPF + agência + conta** (chave de negócio única).

## Estrutura (Clean Architecture enxuta)
```
src/
  BancoKRT.GestaoLimite.Domain          # Entidade ContaLimite + regras (Debitar/AlterarLimite) + validação de CPF
  BancoKRT.GestaoLimite.Application      # Serviços (Limite e PIX), DTOs, Result, abstração de repositório
  BancoKRT.GestaoLimite.Infrastructure   # EF Core InMemory (DbContext + repositório) + DI
  BancoKRT.GestaoLimite.Web              # MVC: Controllers + Views + API JSON
tests/
  BancoKRT.GestaoLimite.Tests            # xUnit: domínio, serviços e integração (WebApplicationFactory)
```

## Como executar
Pré-requisito: **.NET SDK 9**.

### Visual Studio
Abra `BancoKRT.GestaoLimite.sln`, defina `BancoKRT.GestaoLimite.Web` como projeto de inicialização e execute (F5).

### Linha de comando
```bash
dotnet build
dotnet run --project src/BancoKRT.GestaoLimite.Web
```
Acesse no navegador a URL exibida no console (ex.: `https://localhost:xxxx`):
- **Telas do analista:** Início → *Cadastrar*, *Buscar/gerenciar*, *Simular PIX*.

## Como rodar os testes
```bash
dotnet test
```
Cobre: regra de débito do domínio (incl. *negada não consome limite*), CRUD dos serviços e os endpoints da API (aprovada debita / negada mantém / 404 / 409).

## Regra de negócio do PIX (2.5)
1. Localiza a conta por CPF + agência + conta (não encontrada → 404 na API / mensagem na tela).
2. `valor <= limite` (e `valor > 0`) → **APROVADA**: debita e persiste o novo limite.
3. `valor > limite` → **NEGADA**: o limite **não** é alterado.
4. Transações simultâneas usam *lock por conta* para evitar gasto duplo de limite (o provedor InMemory não oferece transação de banco real).

## Observações de implementação
- Validação de documento — CPF (11 dígitos) ou CNPJ (14 dígitos) com dígitos verificadores — no domínio e nas telas (`[Documento]`). A máscara é removida ao persistir/consultar.
- A API usa JSON camelCase; coleção de exemplos em [`requests.http`](./requests.http).
- Cultura invariante para garantir o parsing correto de valores decimais dos formulários.
