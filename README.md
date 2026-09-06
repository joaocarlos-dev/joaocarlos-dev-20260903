# Sistema de Gestão de Colaboradores e Unidades

API REST em .NET 8 para gestão de usuários, colaboradores e unidades, com CQRS/MediatR, FluentValidation, PostgreSQL, Entity Framework Core, JWT Bearer e Swagger.

## Pré-requisitos

- .NET SDK 8, para desenvolvimento e execução dos testes;
- Docker Desktop com Docker Compose, para a execução completa;
- uma extensão de cliente HTTP que suporte arquivos `.http`, opcional.

## Execução com Docker Compose

Copie `.env.example` para `.env` e substitua todos os valores marcados para troca, especialmente `POSTGRES_PASSWORD`, `JWT_SIGNING_KEY` e `INITIAL_USER_PASSWORD`. O `.env` é ignorado pelo Git e não deve ser versionado.

Suba os serviços:

```powershell
docker compose up --build
```

O Compose inicia o PostgreSQL, aguarda o health check, executa a suíte de testes no serviço `tests` e somente então inicia a API. A API fica disponível em `http://localhost:8080` e o Swagger em `http://localhost:8080/swagger`.

As migrations são aplicadas automaticamente durante a inicialização da API. O usuário inicial também é criado na primeira inicialização quando `INITIAL_USER_CODE`, `INITIAL_USER_LOGIN` e `INITIAL_USER_PASSWORD` estão configurados.

Para encerrar os serviços:

```powershell
docker compose down
```

Para remover também o volume local do PostgreSQL:

```powershell
docker compose down -v
```

## Configuração

Em desenvolvimento local, forneça `ConnectionStrings__DefaultConnection` e as configurações `Jwt__Issuer`, `Jwt__Audience` e `Jwt__SigningKey` por variáveis de ambiente ou por um arquivo de configuração local não versionado. A chave JWT deve ter pelo menos 32 caracteres. A API falha ao iniciar quando a conexão ou a configuração JWT obrigatória está ausente ou insegura.

O `appsettings.Local.json` é um arquivo local ignorado pelo Git e não é necessário para a execução via Compose. Segredos e senhas devem ser fornecidos pelo ambiente de execução, User Secrets ou um gerenciador de segredos.

## Rotas principais

O login é a única rota anônima:

```text
POST /api/v1/auth/login
```

Todas as demais rotas exigem `Authorization: Bearer <token>`:

```text
POST/PATCH/GET /api/v1/users
POST/PATCH/GET/DELETE /api/v1/employees
POST/PATCH/GET /api/v1/units
```

Os detalhes de payloads, respostas e códigos HTTP estão no Swagger. O arquivo [docs/api.http](docs/api.http) contém um fluxo encadeado para clientes compatíveis com variáveis de resposta, cobrindo Swagger, login, CRUD disponível, remoção lógica e rejeição de rota protegida sem token.

## Testes

Execute todos os testes a partir de `backend/src`:

```powershell
dotnet test EmployeeManagmentSystem.slnx
```

Os testes de integração usam Testcontainers quando Docker está disponível. No Compose, eles se conectam ao serviço PostgreSQL externo por `ConnectionStrings__DefaultConnection` e não iniciam outro container.

## Estrutura

```text
backend/src/
  EmployeeManagmentSystem.API/             API, controllers e configuração HTTP
  EmployeeManagmentSystem.Application/    commands, queries, handlers e validators
  EmployeeManagmentSystem.Domain/          entidades e invariantes
  EmployeeManagmentSystem.Infrastructure/  EF Core, PostgreSQL e services
  EmployeeManagmentSystem.Tests/           testes unitários e de integração
docs/                                       plano e coleção HTTP
```
