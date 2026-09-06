# Plano de desenvolvimento do front-end

## Objetivo

Entregar um portal Angular para autenticação e gestão de usuários, unidades e colaboradores sobre a API existente. O projeto será criado em `frontend/`, na raiz e na mesma camada de `backend/`.

```text
.
|-- backend/
|-- frontend/
|-- docs/
|-- docker-compose.yml
`-- README.md
```

O fluxo obrigatório de uma subida limpa será:

```text
PostgreSQL saudável
        |
        v
testes do backend aprovados
        |
        v
API iniciada e saudável
        |
        v
testes do frontend aprovados
        |
        v
frontend iniciado e saudável
```

Se qualquer gate falhar, os serviços que dependem dele devem permanecer sem iniciar.

## Estado atual analisado

### Backend entregue

- [x] solução .NET 8 com API, Application, Domain, Infrastructure e Tests;
- [x] CQRS com MediatR e FluentValidation;
- [x] PostgreSQL com Entity Framework Core, mappings, índices e migration inicial;
- [x] autenticação JWT Bearer, hash de senha e usuário administrativo inicial;
- [x] Problem Details, Swagger e coleção HTTP;
- [x] endpoints de usuários, unidades e colaboradores;
- [x] testes unitários e de integração estruturados;
- [x] Compose com `postgres -> tests -> api`;
- [x] histórico de implementação organizado por fundação, domínio, Application, persistência, segurança, HTTP, testes e fechamento operacional.

### Evolução registrada no repositório

1. A solução base foi reorganizada em `backend/src`, com os projetos e referências entre camadas.
2. O template `WeatherForecast` e classes vazias foram removidos.
3. O primeiro pipeline Docker criou PostgreSQL, estágio de testes e API condicionada ao sucesso dos testes.
4. O Domain recebeu entidades auditáveis, status, invariantes, remoção lógica e seus testes.
5. A Application recebeu commands, queries, handlers, DTOs, abstrações, MediatR e pipeline de validação.
6. Commands, queries e validators foram separados por recurso e ação, conforme o padrão arquitetural definido.
7. A API foi reduzida a composition root e suas configurações foram separadas em DI, JWT, Swagger e pipeline.
8. A Infrastructure recebeu EF Core, Npgsql, DbContext, mappings, migration, repositórios e unidade de trabalho.
9. Segurança recebeu hash, verificação de senha, emissão de JWT, validação de configuração e usuário inicial.
10. Controllers expuseram autenticação e as operações administrativas em `/api/v1`.
11. Problem Details e os códigos HTTP foram padronizados.
12. A suíte foi ampliada com testes unitários e integração real com PostgreSQL.
13. O fluxo local do Docker e a compatibilidade da migration inicial foram corrigidos.
14. README, variáveis de ambiente, plano e coleção HTTP foram finalizados.

A Etapa 0 regularizou a divergência do plano do backend, validou uma requisição real de login e rota protegida e criou a prontidão verificável exigida pelo novo gate.

### Contrato disponível para o portal

| Área | Operações | Limites atuais |
| --- | --- | --- |
| Autenticação | login | Retorna somente `accessToken`; não há refresh token, perfil ou papéis. |
| Usuários | criar, listar, detalhar e atualizar | A atualização permite apenas senha e status; não há exclusão. |
| Unidades | criar, listar, detalhar e atualizar | A atualização permite nome e status; o detalhe inclui colaboradores. |
| Colaboradores | criar, listar, detalhar, atualizar e remover | A remoção é lógica; a atualização permite nome e unidade. |

Regras que o front-end deve refletir sem substituir a validação da API:

- status `0` significa inativo e status `1` significa ativo;
- código possui no máximo 50 caracteres;
- login possui no máximo 100 caracteres;
- nome possui no máximo 150 caracteres;
- senha possui no mínimo 8 caracteres;
- colaborador exige usuário e unidade;
- um usuário pode estar vinculado a apenas um colaborador;
- unidade inativa não recebe inclusão nem transferência de colaborador;
- updates parciais vazios são inválidos;
- a API usa `400`, `401`, `404`, `409` e `500` com Problem Details.

### Ajustes da Etapa 0

- [x] adicionar endpoint anônimo de health check à API;
- [x] adicionar health check ao serviço `api` no Compose;
- [x] corrigir em `docs/api.http` o texto “List active users”, pois a requisição usa `status=0`, que representa inativo;
- [x] executar login real e rota protegida, item anteriormente pendente na Etapa 7 do backend;
- [x] repetir a validação completa em uma máquina com Docker disponível;
- [x] ampliar `.gitignore` para `.angular/`, `dist/` e cobertura do front-end.

Na validação final da Etapa 0, os 96 testes unitários passaram localmente e os 113 testes da suíte completa passaram no serviço `tests` do Compose. Em uma subida limpa, PostgreSQL e API terminaram saudáveis, o gate terminou com código `0`, o login real retornou um token utilizável, a rota protegida respondeu `200` com o token e `401` sem ele.

## Decisões técnicas

- Angular 22, na versão estável mais recente da linha `22.x` no momento da implementação;
- Angular CLI 22 e dependências fixadas em `package-lock.json`;
- Node.js 24 em versão compatível com Angular 22;
- TypeScript estrito;
- componentes standalone;
- rotas lazy por feature;
- formulários reativos tipados;
- `HttpClient` com interceptors funcionais;
- Signals para estado local e derivado;
- RxJS para composição assíncrona;
- Vitest pelo builder oficial do Angular;
- SCSS com tokens CSS;

Referências:

- [versões e suporte do Angular](https://angular.dev/reference/releases);
- [compatibilidade de Node.js e TypeScript](https://angular.dev/reference/versions);
- [guia oficial de estrutura](https://angular.dev/style-guide);
- [configuração oficial de testes](https://angular.dev/guide/testing).

## Arquitetura proposta

```text
frontend/
|-- src/
|   |-- app/
|   |   |-- core/
|   |   |   |-- api/
|   |   |   |-- auth/
|   |   |   `-- errors/
|   |   |-- layout/
|   |   |-- shared/
|   |   |-- features/
|   |   |   |-- dashboard/
|   |   |   |-- users/
|   |   |   |-- units/
|   |   |   `-- employees/
|   |   |-- app.config.ts
|   |   `-- app.routes.ts
|   |-- environments/
|   |-- styles/
|   |-- index.html
|   |-- main.ts
|   `-- styles.scss
|-- nginx/
|   `-- default.conf
|-- Dockerfile
|-- angular.json
|-- package.json
|-- package-lock.json
|-- proxy.conf.json
`-- tsconfig.json
```

Diretrizes:

- organizar código por feature e manter testes junto ao código testado;
- componentes cuidam da apresentação e delegam HTTP e transformação de dados;
- `core` contém apenas responsabilidades globais de instância única;
- `shared` contém apenas peças realmente reutilizadas;
- modelos TypeScript espelham o contrato HTTP, não entidades internas do backend;
- URLs partem de `/api/v1`, sem host fixo no bundle;
- nenhum segredo será armazenado no projeto Angular.

## Experiência e identidade visual

O portal terá aparência administrativa minimalista, responsiva e sóbria.

- fundo neutro e superfícies brancas;
- texto grafite e uma cor primária azul acinzentada;
- cores semânticas moderadas;
- bordas finas como principal separação visual;
- sombras leves apenas quando necessárias;
- cantos entre 6 e 8 pixels;
- tipografia do sistema, sem fonte remota;
- nenhum gradiente, neon ou animação decorativa;
- foco visível, contraste adequado, labels persistentes e navegação por teclado.

| Token | Valor inicial |
| --- | --- |
| Fundo | `#f6f7f8` |
| Superfície | `#ffffff` |
| Texto | `#1f2933` |
| Texto secundário | `#667085` |
| Borda | `#d9dee5` |
| Primária | `#3f5f7a` |
| Sucesso | `#3f6f5a` |
| Alerta | `#8a6a2f` |
| Erro | `#9b3a3a` |

O login será centralizado em um painel simples. A área autenticada terá navegação lateral compacta no desktop e recolhível no mobile, cabeçalho e conteúdo de largura controlada. Tabelas deverão se adaptar a telas pequenas e toda consulta terá estados de carregamento, vazio, erro e sucesso.

## Rotas e funcionalidades

| Rota | Acesso | Entrega |
| --- | --- | --- |
| `/login` | público | login e redirecionamento após sucesso |
| `/dashboard` | autenticado | totais derivados das listagens existentes |
| `/usuarios` | autenticado | listagem e filtros |
| `/usuarios/novo` | autenticado | criação |
| `/usuarios/:id/editar` | autenticado | senha e/ou status |
| `/unidades` | autenticado | listagem e filtros locais |
| `/unidades/nova` | autenticado | criação |
| `/unidades/:id` | autenticado | detalhe e colaboradores |
| `/unidades/:id/editar` | autenticado | nome e/ou status |
| `/colaboradores` | autenticado | listagem, filtros e remoção lógica |
| `/colaboradores/novo` | autenticado | criação com usuário e unidade ativa |
| `/colaboradores/:id` | autenticado | detalhe |
| `/colaboradores/:id/editar` | autenticado | nome e/ou transferência |
| `**` | qualquer | página não encontrada |

As listagens atuais não possuem paginação, busca ou ordenação no servidor. A primeira versão fará essas operações no cliente e exibirá o total carregado. Paginação server-side exige extensão explícita da API.

Ao criar colaborador, o portal cruzará usuários e colaboradores para esconder usuários já vinculados. A API continuará sendo a autoridade e um `409` tratará condições de corrida.

## Autenticação e segurança

- manter token somente durante a sessão do navegador;
- anexar Bearer apenas a chamadas para `/api`;
- proteger rotas privadas e preservar a rota pretendida;
- redirecionar usuário autenticado para o dashboard ao acessar `/login`;
- interpretar expiração do JWT;
- limpar a sessão e voltar ao login ao receber `401`;
- nunca registrar token ou senha;
- manter autorização na API, pois guard é apenas proteção de navegação;
- não simular refresh token enquanto o backend não oferecer esse contrato.

## Integração HTTP e erros

Serão criados contratos tipados para DTOs, requests de criação e PATCH, enum `EntityStatus`, `ProblemDetails` e `ValidationProblemDetails`.

| Status | Comportamento |
| --- | --- |
| `400` | associar erros aos campos e exibir resumo para erro geral |
| `401` | encerrar sessão; no login, mostrar credenciais inválidas |
| `404` | informar ausência do registro e oferecer retorno à lista |
| `409` | mostrar conflito sem apagar o formulário |
| `500` | mostrar erro genérico e `traceId` para suporte |

O mapeamento de nomes de campos será case-insensitive. Payloads PATCH conterão somente campos alterados e nunca enviarão objeto vazio.

## Estratégia de testes

### Unidade e componentes

- clientes HTTP e contratos;
- sessão, interceptor e guards;
- tratamento de `401` e Problem Details;
- validadores e montagem de PATCH;
- formulários de todas as features;
- carregamento, vazio, erro e sucesso;
- filtros, confirmações e acessibilidade essencial.

### Integração do front-end

- login, redirecionamento e navegação protegida;
- criação, edição e remoção lógica com limite HTTP simulado;
- conflitos, validação, `404` e sessão expirada;
- smoke test somente leitura contra a API real do Compose, cobrindo health, login e rota protegida.

O smoke test receberá as credenciais iniciais por ambiente e não gravará dados. Testes de navegador que alterem dados só poderão existir com banco efêmero dedicado; nunca usarão o banco principal.

Scripts previstos:

```text
npm run lint
npm run test:ci
npm run build
npm run verify:ci
```

`verify:ci` executará lint, todos os testes, smoke test e build de produção, propagando qualquer código de falha.

## Docker e gate obrigatório

O `frontend/Dockerfile` será multi-stage:

- `dependencies`: `npm ci`;
- `test`: fontes e dependências, com `npm run verify:ci` como entrada;
- `build`: bundle de produção;
- `final`: Nginx mínimo com bundle e configuração.

O Compose pode construir imagens antes de iniciar containers. A garantia exigida é que o container público do front-end não inicie até a execução atual de `frontend-tests` terminar com sucesso.

| Serviço | Dependências | Gate |
| --- | --- | --- |
| `postgres` | nenhuma | PostgreSQL saudável |
| `tests` | `postgres` saudável | `dotnet test` retorna `0` |
| `api` | `postgres` e `tests` aprovado | health da API responde |
| `frontend-tests` | `api` saudável | `npm run verify:ci` retorna `0` |
| `frontend` | `api` e `frontend-tests` aprovado | health do Nginx responde |

Configuração obrigatória:

- `tests` e `frontend-tests` com `restart: "no"`;
- `api` usa `service_completed_successfully` para `tests`;
- `frontend-tests` usa `service_healthy` para `api`;
- `frontend` usa `service_completed_successfully` para `frontend-tests`;
- frontend publicado inicialmente em `http://localhost:4200`;
- Nginx encaminha `/api/*` a `http://api:8080/api/*`;
- Nginx faz fallback para `index.html`;
- desenvolvimento local usa proxy de `/api` para `http://localhost:8080`;
- não é necessário abrir CORS enquanto os dois ambientes usam proxy same-origin.

Com `docker compose up -d --build`:

- falha no banco impede ambos os testes e aplicações;
- falha no backend impede API, testes do frontend e frontend;
- falha na API impede testes do frontend e frontend;
- falha no frontend impede o container público do frontend;
- sucesso deixa PostgreSQL, API e frontend saudáveis e os gates encerrados com código `0`.

## Etapas de implementação

### Etapa 0 — Regularização do gate atual

- [x] corrigir inconsistência em `docs/api.http`;
- [x] criar health endpoint e health check da API;
- [x] executar login real e rota protegida;
- [x] validar toda a suíte do backend com Docker;
- [x] documentar inspeção dos logs dos testes.

Critério concluído: `postgres -> tests -> api saudável` funciona em subida limpa.

### Etapa 1 — Fundação Angular

- [x] criar `frontend/` com Angular 22, standalone, routing, SCSS e strict;
- [x] fixar dependências e lockfile;
- [x] configurar lint, testes, build e proxy;
- [x] remover demonstração do template;
- [x] atualizar `.gitignore` e README.

Critério: lint, teste inicial e build passam localmente.

### Etapa 2 — Design system e layout

- [x] implementar tokens e estilos-base;
- [x] implementar shell e navegação responsiva;
- [x] criar componentes compartilhados mínimos;
- [x] criar estados de carregamento, vazio, erro e 404;
- [x] validar teclado, contraste e breakpoints;
- [x] garantir ausência de gradientes e cores chamativas.

Critério: shell responsivo, acessível e consistente.

Na validação da Etapa 2, lint, 8 testes e build de produção passaram. O shell foi inspecionado em 1440 × 900 e 390 × 844, sem rolagem horizontal ou erros no console; navegação, menu móvel, foco, regiões semânticas e página 404 foram verificados.

### Etapa 3 — Autenticação

- [x] implementar cliente de login e sessão;
- [x] implementar interceptor e guards;
- [x] implementar tela de login, expiração e logout local;
- [x] cobrir sucesso, erro e redirecionamentos.

Critério: token válido abre o portal; token inválido ou expirado volta ao login.

Na validação da Etapa 3, lint, 22 testes e build de produção passaram. O login foi inspecionado em desktop e 390 × 844, sem rolagem horizontal ou erros no console; redirecionamento da rota protegida, validação acessível e aviso de sessão expirada foram verificados.

### Etapa 4 — Usuários

- [ ] listar e filtrar usuários;
- [ ] criar usuário ativo ou inativo;
- [ ] atualizar senha e/ou status;
- [ ] impedir PATCH vazio;
- [ ] tratar duplicidades;
- [ ] cobrir fluxos por testes.

Critério: todas as operações de usuários da API funcionam no portal.

### Etapa 5 — Unidades

- [ ] listar e criar unidades;
- [ ] exibir detalhe com colaboradores;
- [ ] atualizar nome e/ou status;
- [ ] tratar PATCH vazio, duplicidade e `404`;
- [ ] cobrir fluxos por testes.

Critério: todas as operações de unidades da API funcionam no portal.

### Etapa 6 — Colaboradores

- [ ] listar colaboradores e resolver nomes relacionados;
- [ ] criar usando usuário disponível e unidade ativa;
- [ ] exibir detalhe;
- [ ] atualizar nome e/ou unidade;
- [ ] remover logicamente com confirmação;
- [ ] tratar vínculo único, unidade inativa, duplicidade e `404`;
- [ ] cobrir fluxos por testes.

Critério: todas as operações de colaboradores da API funcionam no portal.

### Etapa 7 — Dashboard e acabamento

- [ ] criar totais derivados das listagens;
- [ ] revisar filtros, ordenação, estados e textos em português;
- [ ] revisar responsividade, acessibilidade e bundle;
- [ ] cruzar comportamento com Swagger e coleção HTTP.

Critério: a interface não oferece operação ausente na API.

### Etapa 8 — Docker e aceite

- [x] criar Dockerfile e Nginx do frontend;
- [x] adicionar `frontend-tests` e `frontend` ao Compose;
- [x] implementar health checks;
- [x] validar falha proposital em cada gate;
- [x] validar subida limpa e segunda subida com `docker compose up -d --build`;
- [ ] validar login e rota protegida pelo portal;
- [x] atualizar README;
- [x] executar revisão final independente.

Critério: a ordem é comprovada por estados e logs; uma falha impede o dependente.

## Gate de aceite do front-end

- [x] `frontend/` existe ao lado de `backend/`;
- [x] Angular é a tecnologia principal e pacotes estão fixados no lockfile;
- [x] lint, todos os testes e build passam;
- [ ] autenticação e proteção de rotas funcionam;
- [ ] todas as operações disponíveis na API estão cobertas;
- [ ] Problem Details são tratados consistentemente;
- [ ] bundle não contém segredo nem URL absoluta da API;
- [ ] interface é responsiva, acessível e minimalista;
- [ ] não existem gradientes nem cores chamativas;
- [x] Nginx serve a SPA e encaminha `/api`;
- [x] Compose respeita `postgres -> tests -> api -> frontend-tests -> frontend`;
- [x] falha do backend impede API e falha do frontend impede frontend;
- [x] PostgreSQL, API e frontend terminam saudáveis;
- [x] documentação está atualizada e não há achado material pendente.

## Fora do escopo inicial

- refresh token, logout remoto e papéis;
- exclusão de usuários ou unidades;
- paginação, busca e ordenação server-side;
- internacionalização, tema escuro, SSR e modo offline;
- testes que gravem no banco principal.

Esses itens exigem decisão de produto ou mudança no backend e não devem ser simulados apenas no cliente.
