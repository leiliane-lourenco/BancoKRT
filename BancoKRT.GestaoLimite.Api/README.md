# Banco KRT — Gestão de Limite de PIX

Sistema simples para **cadastrar contas, definir um limite diário de PIX e fazer transferências**.

Ele é formado por **duas partes** que trabalham juntas:

- **Tela (MVC)** — a parte visual, que você abre no navegador e usa com botões e formulários.
- **API** — o "motor" que recebe os pedidos da tela, aplica as regras (limite, saldo) e **guarda os dados no banco DynamoDB**.

Você não precisa instalar .NET, banco de dados nem nada técnico. **Tudo roda com o Docker** — um programa que sobe o
sistema inteiro pronto para usar.

---

## Como funciona (visão simples)

```
Você (navegador)  →  Tela (MVC)  →  API  →  Banco (DynamoDB)
```

O Docker sobe essas **3 peças de uma vez só**. Você só abre o navegador e usa.

---

## Antes de começar (faça uma vez)

1. **Instale o Docker Desktop**
   Baixe e instale daqui: https://www.docker.com/products/docker-desktop/
   Depois de instalar, **abra o Docker Desktop** e espere ele mostrar **"Engine running"** (o ícone da baleia fica estável).
   Sem o Docker aberto, nada funciona.

2. **Confira que as duas pastas estão lado a lado** dentro do monorepo:
   ```
   C:\Projetos\BancoKRT.GestaoLimite.Monorepo\BancoKRT.GestaoLimite        (a Tela / MVC)
   C:\Projetos\BancoKRT.GestaoLimite.Monorepo\BancoKRT.GestaoLimite.Api    (a API — onde fica este README)
   ```
   As duas precisam estar juntas, porque o Docker monta as duas a partir dessas pastas.

3. **Não use o Visual Studio ao mesmo tempo que o Docker.**
   Se você estiver com os projetos rodando no Visual Studio (botão verde de Run/Debug), **pare-os antes**.
   Os dois brigam pelas mesmas "portas" e dá confusão (veja a seção de Problemas Comuns no fim).

---

## Passo a passo para ligar o sistema

1. Abra o **Docker Desktop** e espere ficar como "Engine running".

2. Abra um terminal (PowerShell) **na pasta da API**. Um jeito fácil: abra a pasta
   `C:\Projetos\BancoKRT.GestaoLimite.Monorepo\BancoKRT.GestaoLimite.Api` no Explorer, clique na barra de endereço, digite `powershell` e aperte Enter.

3. Rode este comando:
   ```bash
   docker compose up -d
   ```

   > ⏳ **Na primeira vez demora alguns minutos** (ele baixa e monta tudo). Nas próximas vezes sobe em segundos.

4. Pronto! Para conferir se subiu, rode:
   ```bash
   docker compose ps
   ```
   Você deve ver **3 itens com o status "Up"** (gestaolimite-dynamodb, gestaolimite-api, gestaolimite-mvc).
   Ou simplesmente abra os links abaixo.

---

## Links para abrir no navegador

| O que é | Link | Para que serve |
|---|---|---|
| **Tela do sistema (MVC)** | http://localhost:5247 | Usar pelo visual: cadastrar, pesquisar, transferir |
| **Swagger (testar a API)** | http://localhost:5180 | Testar a API direto, sem a tela |
| Banco (DynamoDB) | http://localhost:8000 | Só o banco — **não tem tela**, pode ignorar |

---

## Como testar pela TELA (MVC)

1. Abra **http://localhost:5247**.
2. Clique em **Cadastro** e preencha uma conta, por exemplo:
   - Documento (CPF): `111.444.777-35`
   - Agência: `0001`
   - Conta: `123456`
   - Limite diário de PIX: `1000`
   - Saldo: `5000`

   Clique em salvar.
3. Use a **Pesquisa** e digite o documento (`111.444.777-35`). A conta cadastrada vai aparecer.
4. Clique em **Transferir**, informe um valor (ex.: `300`) e confirme.
   Veja o **saldo e o limite diminuírem**. Se tentar passar do limite do dia, o sistema recusa e explica o motivo.

---

## Como testar pelo SWAGGER

O Swagger é uma telinha que lista os comandos da API e deixa você testá-los direto.

> ⚠️ **Importante:** primeiro **cadastre a conta**, depois faça a transferência. Se transferir sem cadastrar antes,
> a API responde **404 (conta não encontrada)**.

1. Abra **http://localhost:5180**.

2. **Cadastrar a conta** — procure **`POST /api/v1/limites`**, clique nele, depois em **"Try it out"**,
   cole o JSON abaixo no campo e clique em **"Execute"**. Deve responder **201** (criado):
   ```json
   {
     "documento": "52998224725",
     "numeroAgencia": "0001",
     "numeroConta": "123456",
     "limitePix": 1000,
     "saldo": 5000
   }
   ```

3. **Fazer a transferência (PIX)** — procure **`POST /api/v1/transacoes-pix`**, clique em **"Try it out"**,
   cole o JSON abaixo (mesmo documento/agência/conta + o valor) e clique em **"Execute"**. Deve responder **200**:
   ```json
   {
     "documento": "52998224725",
     "numeroAgencia": "0001",
     "numeroConta": "123456",
     "valor": 300
   }
   ```
   Na resposta, o campo `aprovada` diz se passou (`true`) ou foi recusada (`false`), com o saldo e o limite atualizados.

---

## Os dados não somem (DynamoDB)

Os dados ficam guardados de verdade. Para provar: com o sistema no ar, reinicie só a API e busque a conta de novo —
ela continua lá.

```bash
docker compose restart api
```

(Antes, numa versão antiga, os dados sumiam toda vez que a API reiniciava. Agora não somem mais.)

---

## Comandos do dia a dia

| O que você quer | Comando |
|---|---|
| **Ligar** (rápido, código sem mudanças) | `docker compose up -d` |
| **Ligar reconstruindo** (depois de mudar o código) | `docker compose up --build -d` |
| Ver se está no ar | `docker compose ps` |
| Ver o que está acontecendo (logs) | `docker compose logs -f` |
| **Desligar** | `docker compose down` |

> Você **não precisa desligar** entre um teste e outro — pode deixar ligado. Ao desligar com `docker compose down`,
> os dados ficam salvos e voltam quando você ligar de novo.
>
> O `--build` só é necessário quando o **código C# muda**. Para só usar/testar, `docker compose up -d` basta.

---

## Problemas comuns

- **O link não abre / "não foi possível conectar"**
  → O Docker Desktop está aberto e "running"? Rode `docker compose ps` e veja se os 3 estão "Up".
  Na primeira vez, espere terminar de montar (demora alguns minutos).

- **A tela redireciona sozinha para `https://localhost:7117` ou aparece erro de "porta em uso"**
  → Você está com os projetos rodando no **Visual Studio** ao mesmo tempo que o Docker. Pare o debug do Visual Studio
  (botão vermelho de Stop) e tente de novo. Use **ou** o Visual Studio **ou** o Docker — nunca os dois juntos.

- **A primeira execução está muito lenta**
  → É normal só na primeira vez (ele baixa as imagens e compila). Depois fica rápido.

- **No Swagger, a transferência dá erro 404**
  → Você precisa **cadastrar a conta primeiro** (`POST /api/v1/limites`) e usar o **mesmo** documento/agência/conta
  na transferência.

---

## Detalhes técnicos (para quem é da área)

- **API**: ASP.NET (.NET 9), arquitetura em camadas (Domain / Application / Infrastructure / Api).
- **Persistência**: DynamoDB (AWS SDK). Em desenvolvimento usa o **DynamoDB Local** em container.
- **MVC**: ASP.NET MVC; consome a API por HTTP (`ApiSettings:BaseUrl`).
- **Orquestração**: `docker-compose.yml` com 3 serviços — `dynamodb-local`, `api`, `mvc`.

| Serviço | Porta no seu PC | Porta interna |
|---|---|---|
| MVC | 5247 | 8080 |
| API / Swagger | 5180 | 8080 |
| DynamoDB Local | 8000 | 8000 |

Dentro da rede do Docker, o MVC fala com a API por `http://api:8080` e a API fala com o banco por
`http://dynamodb-local:8000` (configurado por variáveis de ambiente no `docker-compose.yml`).
