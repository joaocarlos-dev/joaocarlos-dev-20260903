# Sistema de Gestão de Colaboradores e Unidades

## Sobre o projeto

Este projeto é uma aplicação para gestão de colaboradores, unidades e usuários.

As principais tecnologias utilizadas são:

- .NET 8 e ASP.NET Core;
- Entity Framework Core;
- PostgreSQL;
- Angular 22;
- Docker e Docker Compose.

O Docker Compose foi construído simulando o fluxo de deploy de uma pipeline. Dessa forma, os serviços da aplicação serão inicializados somente após a execução e aprovação automática dos testes do backend e das verificações do frontend.

## Pré-requisitos

- Docker Desktop com Docker Compose.

## Inicialização

Na raiz do projeto, copie o arquivo `.env.example` para `.env` e substitua os valores indicados, especialmente `POSTGRES_PASSWORD`, `JWT_SIGNING_KEY` e `INITIAL_USER_PASSWORD`.

O arquivo `.env` contém configurações locais e não deve ser versionado.

Inicialize os serviços com:

```powershell
docker compose up -d --build
```

O Docker Compose configura o banco de dados, inicializa a API e o portal e executa automaticamente as verificações do projeto.

O RabbitMQ fica disponível no painel de gerenciamento em `http://localhost:15672`, usando `RABBITMQ_USER` e `RABBITMQ_PASSWORD` do arquivo `.env`.

Depois de subir os containers, para executar opcionalmente o teste de estresse que registra 500 usuários em paralelo e valida os 500 eventos publicados:

```powershell
docker compose --profile stress run --rm --build rabbitmq-stress-tests
```

O teste de estresse usa um banco separado e não é executado pelo fluxo padrão de subida da aplicação.

## Coleção Postman

A coleção [EmployeeManagementSystem.postman_collection.json](postman/EmployeeManagementSystem.postman_collection.json) pode ser importada no Postman para testar as rotas da API.

Após a importação, ajuste a variável baseUrl se necessário:

- Docker: http://localhost:8080;
- Execução local: http://localhost:5121.

Configure também as variáveis adminLogin e adminPassword com os valores de INITIAL_USER_LOGIN e INITIAL_USER_PASSWORD do arquivo .env. O Postman não lê esse arquivo automaticamente.

Execute primeiro Health Check e Login do administrador. A coleção salva automaticamente o token de acesso e os IDs criados para reutilizá-los nas requisições seguintes.

Para encerrar os serviços:

```powershell
docker compose down
```

Para encerrar os serviços e remover os dados locais do banco:

```powershell
docker compose down -v
```
