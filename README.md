# Banco KRT — Gestão de Limite de PIX (Monorepo)

Este repositório reúne, num só lugar (**monorepo**), os **dois projetos** que formam o sistema de gestão de limite de PIX:

| Pasta | O que é |
|---|---|
| [BancoKRT.GestaoLimite/](BancoKRT.GestaoLimite/) | **Tela (MVC)** — a parte visual, aberta no navegador. Faz requisições para a API. |
| [BancoKRT.GestaoLimite.Api/](BancoKRT.GestaoLimite.Api/) | **API** — recebe os pedidos, aplica as regras (limite/saldo) e guarda os dados no **DynamoDB**. |

```
Você (navegador)  →  Tela (MVC)  →  API  →  Banco (DynamoDB)
```

## Como rodar e testar

Tudo sobe junto com o **Docker** (Tela + API + Banco). O guia completo, passo a passo e para leigos, está em:

➡️ **[BancoKRT.GestaoLimite.Api/README.md](BancoKRT.GestaoLimite.Api/README.md)**

Resumão para quem tem pressa (com o **Docker Desktop** aberto):

```bash
cd BancoKRT.GestaoLimite.Api
docker compose up -d
```

Depois abra no navegador:

| O que é | Link |
|---|---|
| Tela do sistema (MVC) | http://localhost:5247 |
| Swagger (testar a API) | http://localhost:5180 |

Para parar: `docker compose down` (dentro da pasta `BancoKRT.GestaoLimite.Api`).

## Detalhes técnicos

- **.NET 9** nos dois projetos.
- **API** em camadas (Domain / Application / Infrastructure / Api), persistindo em **DynamoDB** (AWS SDK; em dev usa o **DynamoDB Local** em container).
- **MVC** consome a API por HTTP.
- Orquestração via `docker-compose.yml` (em `BancoKRT.GestaoLimite.Api/`) com 3 serviços: `dynamodb-local`, `api`, `mvc`.
