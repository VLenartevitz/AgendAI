# AgendAI

Monorepo com **Blazor** (frontend) e **ASP.NET Core API** (backend), preparado para rodar com Docker.

## Estrutura

```
AgendAI/
├── src/
│   ├── AgendAI.Web/     # Blazor Web App (Server)
│   └── AgendAI.Api/     # API REST ASP.NET Core
├── AgendAI.sln
├── docker-compose.yml
└── README.md
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (opcional, para rodar em containers)

## Desenvolvimento local (sem Docker)

1. Restaurar e rodar a solution:

   ```bash
   dotnet restore
   dotnet build
   ```

2. Rodar a API (ex.: porta 5000):

   ```bash
   dotnet run --project src/AgendAI.Api
   ```

3. Rodar o Blazor (ex.: porta 5001), em outro terminal:

   ```bash
   dotnet run --project src/AgendAI.Web
   ```

4. Abrir no navegador:
   - Blazor: https://localhost:5001 (ou a porta exibida no terminal)
   - API: https://localhost:5000 (ou a porta exibida no terminal)
   - **PostgreSQL:** localhost:5432 (`database=agendai`, `user=agendai`, `password=agendai123`)
	- 
## Rodar com Docker

Na raiz do repositório:

```bash
docker compose up --build
```

- **Blazor (Web):** http://localhost:5001  
- **API:** http://localhost:5000  

Para rodar em segundo plano:

```bash
docker compose up -d --build
```

Para parar:

```bash
docker compose down
```

## Projetos

| Projeto        | Descrição                    | Porta (Docker) |
|----------------|-----------------------------|----------------|
| **AgendAI.Web** | Blazor Server (UI)          | 5001           |
| **AgendAI.Api** | API REST (controllers)      | 5000           |
| **PostgreSQL**  | Banco de dados              | 5432           |

## Licença

Ver [LICENSE](LICENSE).
