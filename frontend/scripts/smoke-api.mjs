const apiBaseUrl = process.env['API_BASE_URL'] ?? 'http://localhost:8080';
const login = process.env['SMOKE_LOGIN'];
const password = process.env['SMOKE_PASSWORD'];

if (!login || !password) {
  throw new Error('SMOKE_LOGIN e SMOKE_PASSWORD são obrigatórios.');
}

const healthResponse = await fetch(`${apiBaseUrl}/health`);

if (!healthResponse.ok) {
  throw new Error(`Health check da API falhou com status ${healthResponse.status}.`);
}

const loginResponse = await fetch(`${apiBaseUrl}/api/v1/auth/login`, {
  method: 'POST',
  headers: { 'content-type': 'application/json' },
  body: JSON.stringify({ login, password }),
});

if (!loginResponse.ok) {
  throw new Error(`Login do smoke test falhou com status ${loginResponse.status}.`);
}

const authentication = await loginResponse.json();

if (typeof authentication.accessToken !== 'string' || authentication.accessToken.length === 0) {
  throw new Error('Login do smoke test não retornou um accessToken válido.');
}

const protectedResponse = await fetch(`${apiBaseUrl}/api/v1/users`, {
  headers: { authorization: `Bearer ${authentication.accessToken}` },
});

if (!protectedResponse.ok) {
  throw new Error(`Rota protegida do smoke test falhou com status ${protectedResponse.status}.`);
}

console.log('Smoke test da API concluído com sucesso.');
