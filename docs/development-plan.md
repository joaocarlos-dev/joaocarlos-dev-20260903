# Plano de desenvolvimento

## Objetivo

Entregar um backend funcional para gestão de usuários, colaboradores e unidades em três dias. O front-end em Angular será iniciado somente após a conclusão e validação integral do backend.

## Acompanhamento

- `[x]` concluído e validado;
- `[ ]` pendente;
- todos os testes devem seguir o padrão AAA: Arrange, Act e Assert;
- o projeto de testes é organizado nas pastas `UnitTests` e `IntegrationTests`.

## Escopo funcional

O backend deverá oferecer:

- [ ] cadastro de usuários com código e login únicos, senha protegida e status ativo ou inativo;
- [ ] atualização somente da senha e do status dos usuários;
- [ ] listagem de usuários com filtro por status;
- [ ] cadastro de colaboradores com código único, nome, usuário e unidade;
- [ ] atualização do nome e da unidade dos colaboradores;
- [ ] remoção lógica e listagem de colaboradores;
- [ ] cadastro e inativação de unidades;
- [ ] bloqueio da inclusão ou transferência de colaboradores para unidades inativas;
- [ ] listagem de unidades com seus colaboradores;
- [ ] autenticação JWT Bearer;
- [ ] documentação e execução das rotas pelo Swagger e por uma coleção HTTP ou Postman;
- [ ] persistência em PostgreSQL executado por Docker;
- [ ] testes unitários e de integração.

## Estado atual do projeto

- [x] projetos API, Application, Domain, Infrastructure e Tests criados;
- [x] dependências entre as camadas configuradas;
- [x] template WeatherForecast removido;
- [x] Dockerfile, PostgreSQL e pipeline de testes configurados pelo Docker Compose;
- [x] estrutura inicial do Domain criada com testes unitários;
- [ ] persistência e migrations configuradas;
- [ ] casos de uso e rotas implementados;
- [ ] autenticação e autorização configuradas;
- [ ] testes de integração implementados.

## Arquitetura

As dependências entre os projetos deverão seguir esta direção:

```text
Front-end
    |
    v
API -----------------> Infrastructure
 |                           |
 v                           v
Application -------------> Domain
```

- [x] Domain não depende de outro projeto.
- [x] Application depende somente de Domain.
- [x] Infrastructure depende de Application e Domain.
- [x] API depende de Application e Infrastructure e atua como composition root.
- [ ] PostgreSQL é acessado exclusivamente pela Infrastructure.

A Application definirá contratos para repositórios, unidade de trabalho, hash de senha e geração de token. A Infrastructure implementará esses contratos. Controllers serão responsáveis apenas pelo protocolo HTTP e delegarão o comportamento aos casos de uso.

## Mensageria

A fila interna de registro de usuários não faz parte dos requisitos obrigatórios e não será usada no fluxo principal. O cadastro precisa confirmar imediatamente as restrições de unicidade e o vínculo com colaboradores. Caso a mensageria seja adicionada posteriormente, será usada apenas para efeitos secundários após a persistência da transação.

## Modelo de domínio

### Entidade base

Uma entidade base auditável atenderá ao requisito de herança e concentrará:

- identificador;
- data de criação;
- data de atualização;
- data de remoção lógica.

### User

- `id`;
- `code`, único;
- `login`, único;
- `password_hash`;
- `status` como Active ou Inactive;
- campos de auditoria.

### Employee

- `id`;
- `code`, único;
- `name`;
- `email`, caso seja mantido como extensão do requisito;
- `user_id`, obrigatório e único;
- `unit_id`, obrigatório;
- campos de auditoria.

### Unit

- `id`;
- `code`, único;
- `name`;
- `status` como Active ou Inactive;
- campos de auditoria.

As tabelas serão nomeadas `users`, `employees` e `units`. Os campos temporais usarão horário com fuso. Uma unidade inativa não poderá receber novos colaboradores nem transferências. A remoção de colaborador será lógica.

## Rotas planejadas

Todas as rotas, exceto o login, exigirão autenticação JWT.

| Método | Rota | Finalidade |
| --- | --- | --- |
| `POST` | `/api/v1/auth/login` | Autenticar um usuário ativo |
| `POST` | `/api/v1/users` | Cadastrar usuário |
| `GET` | `/api/v1/users` | Listar usuários e aceitar filtro por status |
| `GET` | `/api/v1/users/{id}` | Consultar usuário |
| `PATCH` | `/api/v1/users/{id}` | Alterar senha ou status |
| `POST` | `/api/v1/employees` | Cadastrar colaborador |
| `GET` | `/api/v1/employees` | Listar colaboradores |
| `GET` | `/api/v1/employees/{id}` | Consultar colaborador |
| `PATCH` | `/api/v1/employees/{id}` | Alterar nome ou unidade |
| `DELETE` | `/api/v1/employees/{id}` | Remover colaborador logicamente |
| `POST` | `/api/v1/units` | Cadastrar unidade |
| `GET` | `/api/v1/units` | Listar unidades com seus colaboradores |
| `GET` | `/api/v1/units/{id}` | Consultar unidade |
| `PATCH` | `/api/v1/units/{id}` | Alterar nome ou status |

As falhas seguirão o padrão Problem Details:

- `400 Bad Request` para campos ou requisições inválidas;
- `401 Unauthorized` para autenticação ausente ou inválida;
- `404 Not Found` para recursos inexistentes;
- `409 Conflict` para duplicidades, vínculos já utilizados ou unidades inativas;
- `201 Created` para cadastros concluídos;
- `204 No Content` para atualizações e remoções concluídas sem corpo.

## Dia 1: fundação, domínio e persistência

### Estrutura

- [x] normalizar os diretórios dos projetos;
- [x] incluir API, Application, Domain, Infrastructure e Tests na solução;
- [x] organizar o projeto de testes nas pastas `UnitTests` e `IntegrationTests`;
- [x] configurar as referências entre as camadas;
- [x] remover o template WeatherForecast.

### Domain e Application

- [x] criar a entidade base auditável;
- [x] criar User, Employee e Unit;
- [x] criar enums e regras de negócio iniciais;
- [ ] criar DTOs, contratos de repositórios e casos de uso;
- [ ] definir contratos para hash de senha, token e transações.

### Infrastructure e Docker

- [ ] configurar Entity Framework Core e Npgsql;
- [ ] criar DbContext, mappings e repositórios;
- [ ] criar a migration inicial;
- [ ] definir índices, relacionamentos e restrições;
- [x] criar Dockerfile e `docker-compose.yml`;
- [x] configurar PostgreSQL com volume, health check e variáveis de ambiente;
- [ ] preparar um usuário inicial para permitir o primeiro login.

### Critério de conclusão

- [x] solução compilando;
- [x] PostgreSQL iniciado pelo Docker Compose;
- [ ] migration aplicada em banco vazio;
- [ ] API conectando ao banco;
- [x] dependências arquiteturais validadas.

## Dia 2: casos de uso, rotas e autenticação

### Segurança

- [ ] implementar hash seguro de senha;
- [ ] implementar emissão e validação de JWT;
- [ ] impedir login de usuário inativo;
- [ ] proteger todas as rotas administrativas.

### Funcionalidades

- [ ] implementar casos de uso de usuários;
- [ ] implementar casos de uso de unidades;
- [ ] implementar casos de uso de colaboradores;
- [ ] validar unicidade de códigos e login;
- [ ] garantir a relação única entre usuário e colaborador;
- [x] impedir inclusão ou transferência para unidade inativa no Domain;
- [x] implementar remoção lógica de colaboradores no Domain.

### API

- [ ] implementar controllers e rotas versionadas;
- [ ] configurar validação das requisições;
- [ ] configurar tratamento global de erros;
- [ ] configurar Problem Details;
- [ ] configurar Swagger com autenticação Bearer.

### Testes unitários

- [x] testar regras iniciais das entidades usando AAA;
- [x] testar alterações permitidas nos usuários;
- [x] testar bloqueio de unidade inativa;
- [ ] testar vínculo único entre usuário e colaborador;
- [ ] testar os fluxos de sucesso e falha dos casos de uso.

### Critério de conclusão

- [ ] todas as rotas disponíveis no Swagger;
- [ ] autenticação funcionando;
- [ ] regras críticas cobertas por testes unitários;
- [ ] controllers sem lógica de negócio ou persistência;
- [x] compilação e testes unitários atuais passando.

## Dia 3: integração e aceite do backend

### Testes de integração

- [ ] configurar WebApplicationFactory;
- [ ] executar PostgreSQL isolado com Testcontainers;
- [ ] validar migrations e inicialização da API;
- [ ] testar autenticação válida e inválida;
- [ ] testar bloqueio de usuário inativo;
- [ ] testar os fluxos de usuários, unidades e colaboradores;
- [ ] testar filtros, duplicidades e recursos inexistentes;
- [ ] testar unidade inativa e relação única de usuário;
- [ ] testar remoção lógica;
- [ ] testar listagem de unidades com colaboradores;
- [ ] testar acesso sem token.

### Empacotamento e documentação

- [x] validar o Dockerfile da API;
- [x] integrar API e PostgreSQL no Docker Compose;
- [ ] garantir que nenhum segredo seja versionado;
- [ ] criar coleção Postman ou atualizar o arquivo `.http`;
- [ ] documentar execução, migrations, autenticação e testes no README;
- [ ] executar revisão final independente.

### Gate obrigatório

O front-end somente poderá começar quando:

- [x] `dotnet build` terminar sem erros;
- [ ] todos os testes unitários e de integração passarem;
- [x] `docker compose up` iniciar API e PostgreSQL;
- [ ] o Swagger estiver acessível;
- [ ] a migration funcionar em banco vazio;
- [ ] o fluxo completo estiver validado;
- [ ] nenhum segredo estiver versionado;
- [ ] não houver achados materiais pendentes na revisão final.

## Etapa posterior: front-end

Após o gate do backend, o desenvolvimento Angular seguirá esta ordem:

- [ ] estrutura do projeto e configuração dos ambientes;
- [ ] autenticação, interceptor JWT e guards;
- [ ] layout e navegação;
- [ ] gestão de usuários;
- [ ] gestão de unidades;
- [ ] gestão de colaboradores;
- [ ] validações e tratamento de erros;
- [ ] testes e integração final com a API.
