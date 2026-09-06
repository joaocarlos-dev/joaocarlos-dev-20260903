# Plano de desenvolvimento do backend

## Objetivo

Entregar uma API funcional para gestão de usuários, colaboradores e unidades, com autenticação JWT, persistência PostgreSQL, documentação Swagger e cobertura por testes unitários e de integração.

O front-end em Angular somente será iniciado depois que o gate de aceite do backend estiver integralmente concluído.

## Como acompanhar

- `[x]` concluído e validado;
- `[ ]` pendente;
- um item estrutural concluído não significa que a feature correspondente está disponível pela API;
- todos os testes devem seguir o padrão AAA: Arrange, Act e Assert;
- testes devem ser organizados em `UnitTests` e `IntegrationTests`.

## Arquitetura definida

O projeto seguirá organização em camadas, CQRS com MediatR, validação com FluentValidation e uma adaptação de MVC na API.

```text
API ----------------------> Application
 |                              |
 |                              v
 +--------> Infrastructure --> Domain
```

- [x] Domain não depende de outros projetos.
- [x] Application depende somente de Domain.
- [x] Infrastructure depende de Application e Domain.
- [x] API depende de Application e Infrastructure e atua como composition root.
- [x] garantir que PostgreSQL e serviços externos sejam acessados exclusivamente pela Infrastructure.

### Domain

Responsável por entidades, estados, invariantes e regras que precisam permanecer válidas independentemente da entrada da API ou da persistência.

- [x] entidade base auditável com identificador e datas de criação, atualização e remoção lógica;
- [x] entidades `User`, `Employee` e `Unit`;
- [x] status ativo e inativo;
- [x] bloqueio de autenticação de usuário inativo;
- [x] bloqueio de inclusão e transferência para unidade inativa;
- [x] remoção lógica de colaboradores;
- [x] testes unitários das regras atuais.

### Application

Responsável pela orquestração das operações, sem acesso direto a banco, HTTP ou implementações de segurança.

```text
Application/
|-- Abstractions/
|-- Behaviors/
|-- Commands/
|   `-- Recurso/
|       `-- Acao/
|           |-- AcaoCommand.cs
|           `-- AcaoCommandValidator.cs
|-- Queries/
|   `-- Recurso/
|       `-- Acao/
|           |-- AcaoQuery.cs
|           `-- AcaoQueryValidator.cs
|-- Common/
`-- DTOs/
```

Regras obrigatórias:

- [x] escritas representadas por commands;
- [x] leituras representadas por queries;
- [x] cada ação possui uma pasta própria;
- [x] command ou query e seu handler permanecem no mesmo arquivo;
- [x] cada validator permanece em arquivo separado;
- [x] handlers enviados pelo MediatR;
- [x] validators executados pelo pipeline do FluentValidation antes do handler;
- [x] contratos de repositórios, unidade de trabalho, hash de senha e token definidos;
- [x] DTOs de saída definidos;
- [x] não utilizar arquivos, pastas ou classes `UseCase` ou `UseCases`.

### Infrastructure

Responsável por PostgreSQL, Entity Framework Core e implementações dos contratos da Application.

Cada implementação será um service em sua própria pasta. A extensão de DI permanecerá no final do mesmo arquivo do service.

```text
Infrastructure/
|-- Persistence/
|   |-- EmployeeManagementDbContext.cs
|   |-- Configurations/
|   `-- Migrations/
`-- Services/
    |-- Users/
    |   `-- UserService.cs
    |-- Employees/
    |   `-- EmployeeService.cs
    |-- Units/
    |   `-- UnitService.cs
    `-- Security/
        |-- PasswordHasherService.cs
        `-- TokenService.cs
```

Padrão obrigatório para cada service:

```text
UserService.cs
|-- UserService
`-- UserServiceDependencyInjection
```

- [x] implementar `UserService` e `UserServiceDependencyInjection`;
- [x] implementar `EmployeeService` e `EmployeeServiceDependencyInjection`;
- [x] implementar `UnitService` e `UnitServiceDependencyInjection`;
- [x] implementar serviço de hash de senha com sua extensão de DI;
- [x] implementar serviço de geração de JWT com sua extensão de DI;
- [x] implementar unidade de trabalho e configuração de persistência;
- [x] chamar as extensões dos services somente pela configuração central da API.

### API e MVC adaptado

A API será a camada de apresentação. Os controllers representam o papel de Controller; commands, queries, entidades e DTOs representam o Model; as respostas JSON representam a View adaptada para uma API HTTP.

Os controllers devem apenas:

- receber e interpretar dados HTTP;
- construir commands ou queries;
- enviar a mensagem pelo MediatR;
- converter o resultado no status HTTP adequado.

Controllers não podem conter regras de negócio, consultas ao banco ou implementações de serviços.

```text
API/
|-- Configurations/
|   |-- DependencyInjection/
|   |   `-- DependencyInjectionConfiguration.cs
|   |-- Jwt/
|   |   `-- JwtConfiguration.cs
|   |-- Swagger/
|   |   `-- SwaggerConfiguration.cs
|   `-- Pipeline/
|       `-- ApiConfiguration.cs
|-- Controllers/
|   |-- AuthController.cs
|   |-- UsersController.cs
|   |-- EmployeesController.cs
|   `-- UnitsController.cs
`-- Program.cs
```

- [x] manter o `Program.cs` com bootstrap mínimo;
- [x] centralizar configurações fora do `Program.cs`;
- [x] mover as configurações atuais para suas respectivas subpastas;
- [x] organizar os namespaces de configuração conforme as subpastas;
- [x] registrar os services da Infrastructure em `Configurations/DependencyInjection`;
- [x] manter configuração e middleware de Swagger em `Configurations/Swagger`;
- [x] manter configuração de autenticação e validação JWT em `Configurations/Jwt`;
- [x] manter composição do pipeline HTTP em `Configurations/Pipeline`;
- [x] criar controllers finos, organizados por recurso.

## Estado atual validado

### Concluído

- [x] solução com API, Application, Domain, Infrastructure e Tests;
- [x] referências entre camadas configuradas;
- [x] template WeatherForecast removido;
- [x] Domain e invariantes iniciais implementados;
- [x] commands, queries, handlers e validators estruturados por ação;
- [x] contratos e DTOs da Application definidos;
- [x] MediatR e pipeline do FluentValidation registrados;
- [x] configuração inicial de JWT Bearer existente na API;
- [x] configuração inicial do Swagger com esquema Bearer existente na API;
- [x] Dockerfile multi-stage com etapa de testes;
- [x] Docker Compose com PostgreSQL, testes como gate e API;
- [x] todos os testes unitários atuais passando;
- [x] testes atuais organizados em `UnitTests` e seguindo AAA.

### Ainda não entregue ponta a ponta

- [x] services de persistência da Infrastructure;
- [x] Entity Framework Core e provider Npgsql;
- [x] DbContext, mappings e migration inicial;
- [x] controllers e rotas HTTP;
- [x] emissão real de JWT e hash seguro de senha;
- [x] tratamento global de erros e Problem Details;
- [x] autorização das rotas administrativas;
- [x] testes de integração;
- [x] coleção HTTP ou Postman;
- [x] documentação operacional no README.

## Etapa 1 — Reorganização final da API

Objetivo: consolidar o padrão estrutural antes da implementação das rotas.

- [x] criar `Configurations/DependencyInjection`;
- [x] criar `Configurations/Jwt`;
- [x] criar `Configurations/Swagger`;
- [x] criar `Configurations/Pipeline`;
- [x] mover as configurações existentes e ajustar namespaces;
- [x] adicionar status ao `CreateUserCommand`, ao Domain e ao validator correspondente;
- [x] testar criação de usuário ativo e inativo;
- [x] manter no `Program.cs` somente criação do builder, registro central, construção e pipeline central;
- [x] validar registro do MediatR, FluentValidation, autenticação e Swagger;
- [x] atualizar testes de configuração da API.

Critério de conclusão:

- [x] `Program.cs` permanece mínimo;
- [x] nenhuma configuração transversal fica diretamente no `Program.cs`;
- [x] solução compila e todos os testes unitários passam.

## Etapa 2 — Persistência e services da Infrastructure

Objetivo: implementar todos os contratos da Application com PostgreSQL.

### Persistência

- [x] adicionar Entity Framework Core e Npgsql;
- [x] criar `EmployeeManagementDbContext`;
- [x] implementar `IUnitOfWork`;
- [x] mapear `User`, `Employee` e `Unit`;
- [x] usar nomes de tabelas `users`, `employees` e `units`;
- [x] usar tipos temporais com fuso;
- [x] configurar remoção lógica de colaboradores nas consultas;
- [x] criar migration inicial;
- [x] validar a migration em banco vazio.

### Restrições no banco

- [x] índice único para código de usuário;
- [x] índice único case-insensitive para login;
- [x] índice único para código de colaborador;
- [x] índice único para código de unidade;
- [x] relacionamento obrigatório e único entre usuário e colaborador;
- [x] relacionamento obrigatório entre colaborador e unidade;
- [x] chaves estrangeiras e comportamento de exclusão definidos explicitamente.

### Services

- [x] `UserService` implementa `IUserRepository`;
- [x] `EmployeeService` implementa `IEmployeeRepository`;
- [x] `UnitService` implementa `IUnitRepository`;
- [x] cada service fica em pasta própria;
- [x] cada arquivo de service termina com `{ServiceName}DependencyInjection`;
- [x] as extensões registram somente os contratos pertencentes ao service;
- [x] as extensões são chamadas pela configuração de DI da API.

Critério de conclusão:

- [x] todos os contratos de persistência possuem implementação;
- [x] migration aplica e reverte sem erro em banco vazio;
- [x] API inicia com provider PostgreSQL configurado e banco saudável no Compose;
- [x] restrições de unicidade existem também no banco.

## Etapa 3 — Segurança e autenticação

Objetivo: implementar e registrar os mecanismos de credencial e token que serão expostos pela API na etapa seguinte.

- [x] implementar hash seguro de senha;
- [x] implementar verificação de senha;
- [x] implementar geração de JWT com identificador e login do usuário;
- [x] configurar emissor, audiência, chave e expiração por ambiente;
- [x] impedir inicialização ou emissão de token sem configuração segura;
- [x] preparar usuário inicial sem versionar senha ou hash sensível;
- [x] testar unitariamente hash, verificação de senha e geração de token;
- [x] testar unitariamente credenciais válidas, inválidas e usuário inativo.

Critério de conclusão:

- [x] implementações de `IPasswordHasher` e `ITokenService` estão registradas pela API;
- [x] tokens gerados atendem às regras de validação configuradas;
- [x] configuração insegura ou ausente impede emissão de token.

## Etapa 4 — Features HTTP

Objetivo: expor as operações da Application por controllers finos.

| Recurso | Método e rota | Mensagem | Application | Infrastructure | API | Integração |
| --- | --- | --- | --- | --- | --- | --- |
| Autenticação | `POST /api/v1/auth/login` | `AuthenticateQuery` | [x] | [x] | [x] | [x] |
| Usuários | `POST /api/v1/users` | `CreateUserCommand` | [x] | [x] | [x] | [x] |
| Usuários | `GET /api/v1/users` | `GetUsersQuery` | [x] | [x] | [x] | [x] |
| Usuários | `GET /api/v1/users/{id}` | `GetUserQuery` | [x] | [x] | [x] | [x] |
| Usuários | `PATCH /api/v1/users/{id}` | `UpdateUserCommand` | [x] | [x] | [x] | [x] |
| Colaboradores | `POST /api/v1/employees` | `CreateEmployeeCommand` | [x] | [x] | [x] | [x] |
| Colaboradores | `GET /api/v1/employees` | `GetEmployeesQuery` | [x] | [x] | [x] | [x] |
| Colaboradores | `GET /api/v1/employees/{id}` | `GetEmployeeQuery` | [x] | [x] | [x] | [x] |
| Colaboradores | `PATCH /api/v1/employees/{id}` | `UpdateEmployeeCommand` | [x] | [x] | [x] | [x] |
| Colaboradores | `DELETE /api/v1/employees/{id}` | `DeleteEmployeeCommand` | [x] | [x] | [x] | [x] |
| Unidades | `POST /api/v1/units` | `CreateUnitCommand` | [x] | [x] | [x] | [x] |
| Unidades | `GET /api/v1/units` | `GetUnitsQuery` | [x] | [x] | [x] | [x] |
| Unidades | `GET /api/v1/units/{id}` | `GetUnitQuery` | [x] | [x] | [x] | [x] |
| Unidades | `PATCH /api/v1/units/{id}` | `UpdateUnitCommand` | [x] | [x] | [x] | [x] |

Regras funcionais a validar ponta a ponta:

- [x] cadastro de usuário aceita status inicial ativo ou inativo;
- [x] código e login de usuário únicos;
- [x] atualização de usuário limitada a senha e status;
- [x] filtro de usuários por status;
- [x] código de colaborador único;
- [x] exatamente um usuário por colaborador;
- [x] atualização de colaborador limitada a nome e unidade;
- [x] remoção lógica e exclusão dos removidos nas listagens;
- [x] código de unidade único;
- [x] unidade inativa não recebe inclusão nem transferência;
- [x] listagem de unidades inclui seus colaboradores não removidos.

Regras de acesso HTTP:

- [x] criar `AuthController` com login anônimo;
- [x] manter o login como única rota anônima;
- [x] proteger todos os controllers administrativos com autenticação Bearer;
- [x] login válido retorna token utilizável;
- [x] login inválido não revela se usuário ou senha falhou;
- [x] usuário inativo não recebe token;
- [x] rota protegida rejeita acesso sem token ou com token inválido.

## Etapa 5 — Contrato HTTP e tratamento de erros

Objetivo: padronizar respostas, falhas e documentação das rotas.

- [x] configurar versionamento em `/api/v1`;
- [x] mapear validações do FluentValidation para `400 Bad Request`;
- [x] mapear autenticação inválida para `401 Unauthorized`;
- [x] mapear recursos inexistentes para `404 Not Found`;
- [x] mapear duplicidades e conflitos de regra para `409 Conflict`;
- [x] retornar `201 Created` nos cadastros;
- [x] retornar `204 No Content` nas atualizações e remoção;
- [x] criar tratamento global de exceções;
- [x] responder falhas com Problem Details;
- [x] documentar autenticação Bearer e respostas no Swagger;
- [x] garantir que o Swagger esteja acessível no ambiente definido para avaliação.

Critério de conclusão:

- [x] controllers não possuem lógica de negócio nem persistência;
- [x] todas as rotas aparecem no Swagger;
- [x] códigos HTTP e Problem Details estão consistentes.

## Etapa 6 — Testes automatizados

### Testes unitários

- [x] testar entidades e invariantes atuais do Domain;
- [x] testar pipeline de validação;
- [x] testar fluxos principais já cobertos de autenticação, usuários, unidades e colaboradores;
- [x] cobrir todos os handlers em sucesso e recurso inexistente;
- [x] cobrir todos os validators com entradas válidas e inválidas;
- [x] cobrir duplicidades, filtros, atualizações, transferência e remoção lógica;
- [x] manter todos os testes atuais no padrão AAA.

### Testes de integração

- [x] criar estrutura `IntegrationTests`;
- [x] configurar `WebApplicationFactory`;
- [x] permitir que a fixture selecione o banco por configuração;
- [x] usar PostgreSQL isolado com Testcontainers na execução local ou CI com daemon Docker disponível;
- [x] reutilizar o serviço PostgreSQL do Compose quando os testes rodarem dentro do container `tests`;
- [x] não iniciar Testcontainers de dentro do container `tests`;
- [x] testar aplicação das migrations;
- [x] testar autenticação válida, inválida e usuário inativo;
- [x] testar acesso anônimo às rotas protegidas;
- [x] testar todos os endpoints de usuários;
- [x] testar todos os endpoints de colaboradores;
- [x] testar todos os endpoints de unidades;
- [x] testar filtros, duplicidades e recursos inexistentes;
- [x] testar unidade inativa, vínculo único e remoção lógica;
- [x] validar formato de Problem Details.

Critério de conclusão:

- [x] testes unitários e de integração passam localmente;
- [x] testes passam no estágio `tests` do Docker Compose;
- [x] execução local ou CI valida o modo Testcontainers;
- [x] execução pelo Compose valida o modo de conexão externa;
- [x] banco de integração é isolado e reproduzível.

## Etapa 7 — Docker, documentação e aceite

- [x] Dockerfile da API criado;
- [x] PostgreSQL configurado no Docker Compose com health check e volume;
- [x] API condicionada ao sucesso do container de testes;
- [x] integrar execução automática das migrations na inicialização ou implantação;
- [x] fornecer configurações seguras para JWT sem versionar segredos;
- [x] validar subida completa com `docker compose up --build`;
- [ ] executar uma requisição real de login e uma rota protegida;
- [x] criar coleção Postman ou arquivo `.http` com todos os fluxos;
- [x] documentar configuração, migrations, execução e testes no README;
- [x] executar revisão final independente.

## Gate obrigatório do backend

O backend estará concluído somente quando:

- [x] a solução compila atualmente sem erros ou avisos;
- [x] todos os testes unitários e de integração passarem;
- [x] a migration funcionar em PostgreSQL vazio;
- [x] API, testes e PostgreSQL subirem pelo Docker Compose;
- [x] todas as rotas estiverem disponíveis e documentadas no Swagger;
- [x] autenticação e autorização funcionarem ponta a ponta;
- [x] todas as regras funcionais estiverem validadas pela API;
- [x] nenhum segredo estiver versionado;
- [x] README e coleção de requisições estiverem completos;
- [x] não houver achados materiais pendentes na revisão final.

## Etapa posterior — Front-end

Após o gate do backend:

- [ ] criar estrutura e configurações do Angular;
- [ ] implementar autenticação, interceptor JWT e guards;
- [ ] implementar layout e navegação;
- [ ] implementar gestão de usuários;
- [ ] implementar gestão de unidades;
- [ ] implementar gestão de colaboradores;
- [ ] implementar validações e tratamento de erros;
- [ ] integrar e testar todo o portal com a API.
