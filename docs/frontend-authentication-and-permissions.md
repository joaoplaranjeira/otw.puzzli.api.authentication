# Integração frontend: autenticação OTP e permissões

Este documento descreve o contrato atual da `otw.puzzli.api.authentication` para autenticação por OTP, sessão JWT, gestão de utilizadores e configuração de permissões.

## 1. Configuração base

Definir a URL da API através da configuração do frontend, sem a colocar diretamente nos componentes:

```env
VITE_AUTH_API_URL=https://auth.exemplo.pt
```

Nos exemplos seguintes, `AUTH_API_URL` representa esse valor. Todos os pedidos e respostas usam JSON.

## 2. Fluxo de autenticação

```text
Email -> pedir OTP -> introduzir código -> validar OTP -> guardar JWT
      -> GET /api/users/me -> criar sessão do utilizador
```

O OTP:

- tem 6 algarismos;
- expira ao fim de 10 minutos;
- só pode ser utilizado uma vez;
- permite no máximo 3 pedidos por email em 15 minutos;
- bloqueia novas tentativas durante 15 minutos após 5 códigos incorretos.

### 2.1. Pedir OTP

```http
POST /api/Otp/send
Content-Type: application/json
```

```json
{
  "email": "utilizador@exemplo.pt"
}
```

Resposta de sucesso:

```json
{
  "success": true,
  "message": "OTP enviado com sucesso.",
  "expiresAt": "2026-08-16T15:10:00Z",
  "token": null
}
```

Por segurança, um email inexistente ou inativo também pode produzir `200` com uma mensagem genérica. O frontend deve sempre apresentar a mensagem devolvida e avançar para o ecrã do código, sem tentar inferir se o email existe.

### 2.2. Validar OTP

```http
POST /api/Otp/validate
Content-Type: application/json
```

```json
{
  "email": "utilizador@exemplo.pt",
  "code": "123456"
}
```

Resposta de sucesso:

```json
{
  "success": true,
  "message": "Autenticação concluída com sucesso.",
  "expiresAt": null,
  "token": "eyJhbGciOi..."
}
```

O JWT tem atualmente uma validade predefinida de 60 minutos. Não existe refresh token; quando expirar, o utilizador deve repetir o fluxo OTP.

### 2.3. Obter o utilizador autenticado

Após guardar o token, carregar sempre o perfil a partir da API:

```http
GET /api/users/me
Authorization: Bearer <token>
```

Exemplo de resposta:

```json
{
  "id": 1,
  "name": "Administrador",
  "username": "admin",
  "email": "admin@exemplo.pt",
  "role": "Administrator",
  "photoUrl": null,
  "profile": "Administrador",
  "isActive": true,
  "insertedDate": "2026-08-16T14:00:00Z",
  "updatedDate": "2026-08-16T14:00:00Z",
  "isDefaultPassword": false,
  "permissions": [
    "users.view",
    "users.create",
    "users.edit",
    "permissions.view",
    "permissions.edit"
  ]
}
```

Usar a propriedade `permissions` desta resposta como fonte do estado de autorização da interface.

## 3. Permissões disponíveis

| Permissão | Operação permitida |
| --- | --- |
| `users.view` | Listar e consultar utilizadores |
| `users.create` | Criar utilizadores |
| `users.edit` | Alterar utilizadores e ativar/desativar contas |
| `permissions.view` | Consultar permissões atribuídas |
| `permissions.edit` | Substituir as permissões de um utilizador |

O frontend pode esconder ou desativar ações sem permissão para melhorar a experiência, mas isto não substitui a autorização da API. A API responde com `403` quando o JWT não contém a permissão necessária.

## 4. Gestão de utilizadores

Todos os endpoints desta secção exigem `Authorization: Bearer <token>`.

### 4.1. Listar utilizadores

Requer `users.view`:

```http
GET /api/users
```

Resposta: array de objetos com o mesmo formato de `GET /api/users/me`.

### 4.2. Consultar um utilizador

Requer `users.view`:

```http
GET /api/users/{id}
```

### 4.3. Criar um utilizador

Requer `users.create`:

```http
POST /api/users
Content-Type: application/json
```

```json
{
  "name": "Joana Silva",
  "username": "joana.silva",
  "email": "joana@exemplo.pt",
  "role": "User",
  "photoUrl": null,
  "profile": "Utilizador",
  "isActive": true
}
```

Resposta: `201 Created` com o utilizador criado. Email e username são únicos; uma duplicação produz `409 Conflict`.

### 4.4. Atualizar um utilizador

Requer `users.edit`:

```http
PUT /api/users/{id}
Content-Type: application/json
```

```json
{
  "name": "Joana Silva",
  "username": "joana.silva",
  "email": "joana@exemplo.pt",
  "role": "User",
  "photoUrl": null,
  "profile": "Utilizador",
  "isActive": false
}
```

O `PUT` espera o objeto completo. Não omitir `isActive`: o valor predefinido de um booleano ausente é `false`.

## 5. Gestão de permissões

### 5.1. Consultar permissões

Requer `permissions.view`:

```http
GET /api/users/{userId}/permissions
Authorization: Bearer <token>
```

```json
[
  "users.view",
  "users.edit"
]
```

### 5.2. Substituir permissões

Requer `permissions.edit`:

```http
PUT /api/users/{userId}/permissions
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "permissionKeys": [
    "users.view",
    "users.edit"
  ]
}
```

Importante: este pedido **substitui integralmente** as permissões existentes. O frontend deve:

1. obter as permissões atuais;
2. apresentar todas as opções com checkbox;
3. enviar a lista completa de opções selecionadas;
4. usar a resposta como novo estado do utilizador.

Enviar uma lista vazia remove todas as permissões do utilizador.

As permissões estão incorporadas no JWT no momento do login. Depois de alterar as permissões do próprio utilizador autenticado, o token atual continua com as claims antigas até existir uma nova autenticação OTP. Para evitar uma interface desatualizada, terminar a sessão ou pedir novo login quando as permissões da sessão corrente forem alteradas.

## 6. Tipos TypeScript sugeridos

```ts
export type Permission =
  | "users.view"
  | "users.create"
  | "users.edit"
  | "permissions.view"
  | "permissions.edit";

export interface OtpResponse {
  success: boolean;
  message: string;
  expiresAt: string | null;
  token: string | null;
}

export interface User {
  id: number;
  name: string | null;
  username: string | null;
  email: string | null;
  role: string | null;
  photoUrl: string | null;
  profile: string | null;
  isActive: boolean;
  insertedDate: string;
  updatedDate: string;
  isDefaultPassword: boolean;
  permissions: string[];
}

export interface UserInput {
  name: string;
  username: string;
  email: string;
  role?: string | null;
  photoUrl?: string | null;
  profile?: string | null;
  isActive: boolean;
}
```

Usar `string[]` em `User.permissions` se outras APIs puderem introduzir permissões adicionais. O tipo fechado `Permission` é útil apenas para as permissões conhecidas por este frontend.

## 7. Cliente HTTP e sessão

Exemplo simplificado com `fetch`:

```ts
const AUTH_API_URL = import.meta.env.VITE_AUTH_API_URL;

export async function authRequest<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const token = sessionStorage.getItem("access_token");
  const response = await fetch(`${AUTH_API_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers,
    },
  });

  if (response.status === 401) {
    sessionStorage.removeItem("access_token");
    window.location.assign("/login");
    throw new Error("Sessão expirada");
  }

  if (!response.ok) {
    const error = await response.json().catch(() => null);
    throw new Error(error?.message ?? `Erro HTTP ${response.status}`);
  }

  return response.json() as Promise<T>;
}
```

Como a API não emite cookie `HttpOnly` nem refresh token, o exemplo usa `sessionStorage`. Não guardar o JWT em logs, URLs, analytics ou mensagens de erro. Se for necessária uma sessão persistente e mais resistente a XSS, o backend deverá evoluir para cookies seguros ou um padrão BFF.

### Estado de autenticação recomendado

```ts
interface AuthState {
  token: string | null;
  user: User | null;
  status: "loading" | "authenticated" | "anonymous";
}

const can = (user: User | null, permission: string) =>
  user?.permissions.includes(permission) === true;
```

No arranque do frontend:

1. ler o token da sessão;
2. se não existir, mostrar login;
3. se existir, chamar `GET /api/users/me`;
4. em `200`, preencher a sessão;
5. em `401`, apagar o token e voltar ao login.

Não usar apenas o conteúdo descodificado do JWT para dados de perfil. A descodificação pode ser usada para otimizações visuais, mas `GET /api/users/me` deve validar e atualizar a sessão.

## 8. Tratamento de estados HTTP

| Estado | Significado no frontend |
| --- | --- |
| `200` | Pedido concluído |
| `201` | Utilizador criado |
| `400` | DTO inválido, OTP inválido/expirado, bloqueio ou limite de pedidos |
| `401` | Token ausente, inválido ou expirado; terminar sessão |
| `403` | Sessão válida sem a permissão necessária |
| `404` | Utilizador inexistente |
| `409` | Email ou username já utilizado |
| `500` | Erro inesperado; mostrar mensagem genérica e permitir repetir |

Em erros de validação automática do ASP.NET, a resposta pode usar o formato `ValidationProblemDetails`, com um objeto `errors` por campo. Os formulários devem aceitar tanto `message` como `errors`.

## 9. Bootstrap do primeiro administrador

Este passo pertence ao deployment da API, não ao frontend. Antes do primeiro login, configurar:

```text
BootstrapAdmin__Enabled=true
BootstrapAdmin__Email=admin@exemplo.pt
BootstrapAdmin__Name=Administrador
BootstrapAdmin__Username=admin
```

Depois de aplicar as migrações e reiniciar a API, esse utilizador fica ativo e recebe todas as permissões conhecidas. Pode então autenticar-se por OTP e gerir os restantes utilizadores no frontend.

## 10. Checklist de integração

- [ ] URL da API definida por ambiente.
- [ ] Formulário para pedir OTP.
- [ ] Formulário de 6 algarismos com estado de expiração.
- [ ] JWT guardado apenas no mecanismo de sessão escolhido.
- [ ] `Authorization: Bearer` aplicado aos pedidos protegidos.
- [ ] Sessão carregada através de `GET /api/users/me`.
- [ ] Tratamento global de `401` e `403`.
- [ ] Rotas e botões condicionados por permissão.
- [ ] Lista, criação e edição de utilizadores.
- [ ] Editor de permissões envia sempre a lista completa.
- [ ] Nova autenticação após alterar permissões do utilizador atual.
