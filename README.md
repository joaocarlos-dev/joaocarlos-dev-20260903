# Sistema de Gestão de Colaboradores e Unidades

Sistema em desenvolvimento com API REST em .NET 8 e portal Angular 22 para gestão de usuários, colaboradores e unidades.

## Pré-requisitos

- .NET SDK 8, para desenvolvimento e execução dos testes;
- Node.js 24.15 ou superior na linha 24.x e npm, para desenvolvimento e verificação do front-end;
- Docker Desktop com Docker Compose, para a execução completa;
- uma extensão de cliente HTTP que suporte arquivos `.http`, opcional.

## Execução com Docker Compose

Copie `.env.example` para `.env` e substitua todos os valores marcados para troca, especialmente `POSTGRES_PASSWORD`, `JWT_SIGNING_KEY` e `INITIAL_USER_PASSWORD`. O `.env` é ignorado pelo Git e não deve ser versionado.

Suba os serviços:

```powershell
docker compose up -d --build
```

O Compose inicia o PostgreSQL, executa os testes do backend, inicia a API, executa os gates do front-end e somente então inicia o portal. A API fica disponível em `http://localhost:8080`, o health check em `http://localhost:8080/health`, o Swagger em `http://localhost:8080/swagger` e o portal em `http://localhost:4200`.

O container do portal está integrado ao pipeline. A fundação, o design system, o layout responsivo e a autenticação estão concluídos; as operações administrativas serão adicionadas nas Etapas 4 a 7 do plano do front-end.

Confira o estado de serviços ativos e encerrados:

```powershell
docker compose ps -a
```

Os serviços `tests` e `frontend-tests` devem aparecer encerrados com código `0`; API e frontend devem aparecer saudáveis. Consulte a saída completa dos gates com:

```powershell
docker compose logs tests frontend-tests
```

Para acompanhar a subida e os health checks em tempo real:

```powershell
docker compose logs -f postgres tests api frontend-tests frontend
```

Se `tests` terminar com código diferente de `0`, a API não será iniciada. Se a API não ficar saudável ou `frontend-tests` falhar, o frontend não será iniciado.

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

As rotas anônimas são o login e o health check operacional:

```text
POST /api/v1/auth/login
GET /health
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

### Front-end

Com a API disponível em `http://localhost:8080`, execute a partir de `frontend`:

```powershell
npm ci
npm start
```

O servidor de desenvolvimento publica o portal em `http://localhost:4200` e encaminha `/api` para a API. Para executar lint, testes unitários e build de produção em sequência:

```powershell
npm run verify:ci
```

O gate completo também executa um smoke test somente leitura contra a API. Defina `SMOKE_LOGIN` e `SMOKE_PASSWORD` com as credenciais do usuário inicial antes da execução local.

## Estrutura

```text
backend/src/
  EmployeeManagmentSystem.API/             API, controllers e configuração HTTP
  EmployeeManagmentSystem.Application/    commands, queries, handlers e validators
  EmployeeManagmentSystem.Domain/          entidades e invariantes
  EmployeeManagmentSystem.Infrastructure/  EF Core, PostgreSQL e services
  EmployeeManagmentSystem.Tests/           testes unitários e de integração
frontend/                                  portal Angular, testes e proxy local
docs/                                       plano e coleção HTTP
```
