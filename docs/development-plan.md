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
- [ ] garantir que PostgreSQL e serviços externos sejam acessados exclusivamente pela Infrastructure.

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

- [ ] implementar `UserService` e `UserServiceDependencyInjection`;
- [ ] implementar `EmployeeService` e `EmployeeServiceDependencyInjection`;
- [ ] implementar `UnitService` e `UnitServiceDependencyInjection`;
- [ ] implementar serviço de hash de senha com sua extensão de DI;
- [ ] implementar serviço de geração de JWT com sua extensão de DI;
- [ ] implementar unidade de trabalho e configuração de persistência;
- [ ] chamar as extensões dos services somente pela configuração central da API.

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
- [ ] mover as configurações atuais para suas respectivas subpastas;
- [ ] organizar os namespaces de configuração conforme as subpastas;
- [ ] registrar os services da Infrastructure em `Configurations/DependencyInjection`;
- [ ] manter configuração e middleware de Swagger em `Configurations/Swagger`;
- [ ] manter configuração de autenticação e validação JWT em `Configurations/Jwt`;
- [ ] manter composição do pipeline HTTP em `Configurations/Pipeline`;
- [ ] criar controllers finos, organizados por recurso.

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
- [x] 33 testes unitários passando;
- [x] testes atuais organizados em `UnitTests` e seguindo AAA.

### Ainda não entregue ponta a ponta

- [ ] status inicial ativo ou inativo no cadastro de usuário;
- [ ] services da Infrastructure;
- [ ] Entity Framework Core e provider Npgsql;
- [ ] DbContext, mappings e migrations;
- [ ] controllers e rotas HTTP;
- [ ] emissão real de JWT e hash seguro de senha;
- [ ] tratamento global de erros e Problem Details;
- [ ] autorização das rotas administrativas;
- [ ] testes de integração;
- [ ] coleção HTTP ou Postman;
- [ ] documentação operacional no README.

## Etapa 1 — Reorganização final da API

Objetivo: consolidar o padrão estrutural antes da implementação das rotas.

- [ ] criar `Configurations/DependencyInjection`;
- [ ] criar `Configurations/Jwt`;
- [ ] criar `Configurations/Swagger`;
- [ ] criar `Configurations/Pipeline`;
- [ ] mover as configurações existentes e ajustar namespaces;
- [ ] adicionar status ao `CreateUserCommand`, ao Domain e ao validator correspondente;
- [ ] testar criação de usuário ativo e inativo;
- [ ] manter no `Program.cs` somente criação do builder, registro central, construção e pipeline central;
- [ ] validar registro do MediatR, FluentValidation, autenticação e Swagger;
- [ ] atualizar testes de configuração da API.

Critério de conclusão:

- [ ] `Program.cs` permanece mínimo;
- [ ] nenhuma configuração transversal fica diretamente no `Program.cs`;
- [ ] solução compila e todos os testes unitários passam.

## Etapa 2 — Persistência e services da Infrastructure

Objetivo: implementar todos os contratos da Application com PostgreSQL.

### Persistência

- [ ] adicionar Entity Framework Core e Npgsql;
- [ ] criar `EmployeeManagementDbContext`;
- [ ] implementar `IUnitOfWork`;
- [ ] mapear `User`, `Employee` e `Unit`;
- [ ] usar nomes de tabelas `users`, `employees` e `units`;
- [ ] usar tipos temporais com fuso;
- [ ] configurar remoção lógica de colaboradores nas consultas;
- [ ] criar migration inicial;
- [ ] validar a migration em banco vazio.

### Restrições no banco

- [ ] índice único para código de usuário;
- [ ] índice único para login normalizado;
- [ ] índice único para código de colaborador;
- [ ] índice único para código de unidade;
- [ ] relacionamento obrigatório e único entre usuário e colaborador;
- [ ] relacionamento obrigatório entre colaborador e unidade;
- [ ] chaves estrangeiras e comportamento de exclusão definidos explicitamente.

### Services

- [ ] `UserService` implementa `IUserRepository`;
- [ ] `EmployeeService` implementa `IEmployeeRepository`;
- [ ] `UnitService` implementa `IUnitRepository`;
- [ ] cada service fica em pasta própria;
- [ ] cada arquivo de service termina com `{ServiceName}DependencyInjection`;
- [ ] as extensões registram somente os contratos pertencentes ao service;
- [ ] as extensões são chamadas pela configuração de DI da API.

Critério de conclusão:

- [ ] todos os contratos de persistência possuem implementação;
- [ ] migration aplica e reverte sem erro em banco vazio;
- [ ] API inicia conectada ao PostgreSQL;
- [ ] restrições de unicidade existem também no banco.

## Etapa 3 — Segurança e autenticação

Objetivo: implementar e registrar os mecanismos de credencial e token que serão expostos pela API na etapa seguinte.

- [ ] implementar hash seguro de senha;
- [ ] implementar verificação de senha;
- [ ] implementar geração de JWT com identificador e login do usuário;
- [ ] configurar emissor, audiência, chave e expiração por ambiente;
- [ ] impedir inicialização ou emissão de token sem configuração segura;
- [ ] preparar usuário inicial sem versionar senha ou hash sensível;
- [ ] testar unitariamente hash, verificação de senha e geração de token;
- [ ] testar unitariamente credenciais válidas, inválidas e usuário inativo.

Critério de conclusão:

- [ ] implementações de `IPasswordHasher` e `ITokenService` estão registradas pela API;
- [ ] tokens gerados atendem às regras de validação configuradas;
- [ ] configuração insegura ou ausente impede emissão de token.

## Etapa 4 — Features HTTP

Objetivo: expor as operações da Application por controllers finos.

| Recurso | Método e rota | Mensagem | Application | Infrastructure | API | Integração |
| --- | --- | --- | --- | --- | --- | --- |
| Autenticação | `POST /api/v1/auth/login` | `AuthenticateQuery` | [x] | [ ] | [ ] | [ ] |
| Usuários | `POST /api/v1/users` | `CreateUserCommand` | [ ] | [ ] | [ ] | [ ] |
| Usuários | `GET /api/v1/users` | `GetUsersQuery` | [x] | [ ] | [ ] | [ ] |
| Usuários | `GET /api/v1/users/{id}` | `GetUserQuery` | [x] | [ ] | [ ] | [ ] |
| Usuários | `PATCH /api/v1/users/{id}` | `UpdateUserCommand` | [x] | [ ] | [ ] | [ ] |
| Colaboradores | `POST /api/v1/employees` | `CreateEmployeeCommand` | [x] | [ ] | [ ] | [ ] |
| Colaboradores | `GET /api/v1/employees` | `GetEmployeesQuery` | [x] | [ ] | [ ] | [ ] |
| Colaboradores | `GET /api/v1/employees/{id}` | `GetEmployeeQuery` | [x] | [ ] | [ ] | [ ] |
| Colaboradores | `PATCH /api/v1/employees/{id}` | `UpdateEmployeeCommand` | [x] | [ ] | [ ] | [ ] |
| Colaboradores | `DELETE /api/v1/employees/{id}` | `DeleteEmployeeCommand` | [x] | [ ] | [ ] | [ ] |
| Unidades | `POST /api/v1/units` | `CreateUnitCommand` | [x] | [ ] | [ ] | [ ] |
| Unidades | `GET /api/v1/units` | `GetUnitsQuery` | [x] | [ ] | [ ] | [ ] |
| Unidades | `GET /api/v1/units/{id}` | `GetUnitQuery` | [x] | [ ] | [ ] | [ ] |
| Unidades | `PATCH /api/v1/units/{id}` | `UpdateUnitCommand` | [x] | [ ] | [ ] | [ ] |

Regras funcionais a validar ponta a ponta:

- [ ] cadastro de usuário aceita status inicial ativo ou inativo;
- [ ] código e login de usuário únicos;
- [ ] atualização de usuário limitada a senha e status;
- [ ] filtro de usuários por status;
- [ ] código de colaborador único;
- [ ] exatamente um usuário por colaborador;
- [ ] atualização de colaborador limitada a nome e unidade;
- [ ] remoção lógica e exclusão dos removidos nas listagens;
- [ ] código de unidade único;
- [ ] unidade inativa não recebe inclusão nem transferência;
- [ ] listagem de unidades inclui seus colaboradores não removidos.

Regras de acesso HTTP:

- [ ] criar `AuthController` com login anônimo;
- [ ] manter o login como única rota anônima;
- [ ] proteger todos os controllers administrativos com autenticação Bearer;
- [ ] login válido retorna token utilizável;
- [ ] login inválido não revela se usuário ou senha falhou;
- [ ] usuário inativo não recebe token;
- [ ] rota protegida rejeita acesso sem token ou com token inválido.

## Etapa 5 — Contrato HTTP e tratamento de erros

Objetivo: padronizar respostas, falhas e documentação das rotas.

- [ ] configurar versionamento em `/api/v1`;
- [ ] mapear validações do FluentValidation para `400 Bad Request`;
- [ ] mapear autenticação inválida para `401 Unauthorized`;
- [ ] mapear recursos inexistentes para `404 Not Found`;
- [ ] mapear duplicidades e conflitos de regra para `409 Conflict`;
- [ ] retornar `201 Created` nos cadastros;
- [ ] retornar `204 No Content` nas atualizações e remoção;
- [ ] criar tratamento global de exceções;
- [ ] responder falhas com Problem Details;
- [ ] documentar autenticação Bearer e respostas no Swagger;
- [ ] garantir que o Swagger esteja acessível no ambiente definido para avaliação.

Critério de conclusão:

- [ ] controllers não possuem lógica de negócio nem persistência;
- [ ] todas as rotas aparecem no Swagger;
- [ ] códigos HTTP e Problem Details estão consistentes.

## Etapa 6 — Testes automatizados

### Testes unitários

- [x] testar entidades e invariantes atuais do Domain;
- [x] testar pipeline de validação;
- [x] testar fluxos principais já cobertos de autenticação, usuários, unidades e colaboradores;
- [ ] cobrir todos os handlers em sucesso e recurso inexistente;
- [ ] cobrir todos os validators com entradas válidas e inválidas;
- [ ] cobrir duplicidades, filtros, atualizações, transferência e remoção lógica;
- [ ] manter todos os testes no padrão AAA.

### Testes de integração

- [ ] criar estrutura `IntegrationTests`;
- [ ] configurar `WebApplicationFactory`;
- [ ] permitir que a fixture selecione o banco por configuração;
- [ ] usar PostgreSQL isolado com Testcontainers na execução local ou CI com daemon Docker disponível;
- [ ] reutilizar o serviço PostgreSQL do Compose quando os testes rodarem dentro do container `tests`;
- [ ] não iniciar Testcontainers de dentro do container `tests`;
- [ ] testar aplicação das migrations;
- [ ] testar autenticação válida, inválida e usuário inativo;
- [ ] testar acesso anônimo às rotas protegidas;
- [ ] testar todos os endpoints de usuários;
- [ ] testar todos os endpoints de colaboradores;
- [ ] testar todos os endpoints de unidades;
- [ ] testar filtros, duplicidades e recursos inexistentes;
- [ ] testar unidade inativa, vínculo único e remoção lógica;
- [ ] validar formato de Problem Details.

Critério de conclusão:

- [ ] testes unitários e de integração passam localmente;
- [ ] testes passam no estágio `tests` do Docker Compose;
- [ ] execução local ou CI valida o modo Testcontainers;
- [ ] execução pelo Compose valida o modo de conexão externa;
- [ ] banco de integração é isolado e reproduzível.

## Etapa 7 — Docker, documentação e aceite

- [x] Dockerfile da API criado;
- [x] PostgreSQL configurado no Docker Compose com health check e volume;
- [x] API condicionada ao sucesso do container de testes;
- [ ] integrar execução automática das migrations na inicialização ou implantação;
- [ ] fornecer configurações seguras para JWT sem versionar segredos;
- [ ] validar subida completa com `docker compose up --build`;
- [ ] executar uma requisição real de login e uma rota protegida;
- [ ] criar coleção Postman ou arquivo `.http` com todos os fluxos;
- [ ] documentar configuração, migrations, execução e testes no README;
- [ ] executar revisão final independente.

## Gate obrigatório do backend

O backend estará concluído somente quando:

- [x] a solução compila atualmente sem erros ou avisos;
- [ ] todos os testes unitários e de integração passarem;
- [ ] a migration funcionar em PostgreSQL vazio;
- [ ] API, testes e PostgreSQL subirem pelo Docker Compose;
- [ ] todas as rotas estiverem disponíveis e documentadas no Swagger;
- [ ] autenticação e autorização funcionarem ponta a ponta;
- [ ] todas as regras funcionais estiverem validadas pela API;
- [ ] nenhum segredo estiver versionado;
- [ ] README e coleção de requisições estiverem completos;
- [ ] não houver achados materiais pendentes na revisão final.

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
