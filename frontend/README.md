# Front-end

Portal Angular 22 do Sistema de Gestão de Colaboradores e Unidades.

Requer Node.js 24.15 ou superior na linha 24.x e npm.

## Desenvolvimento local

Instale as dependências e inicie o servidor:

```powershell
npm ci
npm start
```

O portal fica disponível em `http://localhost:4200`. Requisições para `/api` são encaminhadas à API local em `http://localhost:8080` pelo proxy de desenvolvimento.

## Verificação

Execute todos os gates do portal:

```powershell
$env:SMOKE_LOGIN = "admin"
$env:SMOKE_PASSWORD = "sua-senha-local"
npm run verify:ci
```

Também é possível executar cada gate separadamente:

```powershell
npm run lint
npm run test:ci
npm run smoke:api:ci
npm run build
```

Os testes usam Vitest pelo builder oficial do Angular. O smoke test somente lê a rota protegida de usuários e exige que a API esteja disponível. O build de produção é gravado em `dist/frontend`.
