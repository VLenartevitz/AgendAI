# AgendAI

Monorepo com **Blazor** (frontend) e **ASP.NET Core API** (backend), preparado para rodar com Docker.

## Estrutura

```text
AgendAI/
├── src/
│   ├── AgendAI.Web/     # Blazor Web App (Server)
│   └── AgendAI.Api/     # API REST ASP.NET Core
├── tests/
├── AgendAI.sln
├── docker-compose.yml
└── README.md
```

## Pré-requisitos

- .NET 10 SDK
- Docker (opcional, para rodar em containers)

## Desenvolvimento local (sem Docker)

1. Restaurar e compilar:

   ```bash
   dotnet restore
   dotnet build
   ```

2. Rodar a API:

   ```bash
   dotnet run --project src/AgendAI.Api
   ```

3. Rodar o Blazor (em outro terminal):

   ```bash
   dotnet run --project src/AgendAI.Web
   ```

4. Abrir no navegador (use as portas exibidas no terminal):
   - Web: `https://localhost:<porta>`
   - API: `https://localhost:<porta>`
   - PostgreSQL (Docker): `localhost:5432` (`database=agendai`, `user=agendai`, `password=agendai123`)

## Rodar com Docker

Na raiz do repositório:

```bash
docker compose up --build
```

- Web: `http://localhost:5001`
- API: `http://localhost:5000`

Para rodar em segundo plano:

```bash
docker compose up -d --build
```

Para parar:

```bash
docker compose down
```

## Login/Logout (Web)

- O login atual valida as credenciais na API e guarda o usuário logado em memória no `AgendAI.Web` (Blazor Server) via `UserSessionState`.
- O logout é feito pelo botão **Sair** no topo, que limpa o `UserSessionState` e redireciona para `/login`.

## Projetos

| Projeto         | Descrição                  | Porta (Docker) |
|-----------------|----------------------------|----------------|
| **AgendAI.Web** | Blazor Server (UI)         | 5001           |
| **AgendAI.Api** | API REST (controllers)     | 5000           |
| **PostgreSQL**  | Banco de dados             | 5432           |

## Licença

Ver `LICENSE`.

