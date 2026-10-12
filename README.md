# partners-api

API REST para gerir clientes e fornecedores de uma empresa, feita com ASP.NET Core, Entity Framework Core e SQL Server.

Uma entidade pode ser cliente (`CUSTOMER`), fornecedor (`SUPPLIER`) ou ambos (`BOTH`), e só existe um registo por NIF.

## Versões

| Componente | Versão |
| --- | --- |
| .NET SDK | 10.0 (testado com 10.0.401) |
| ASP.NET Core / EF Core | 10.0.12 |
| SQL Server | 2022 (imagem `mcr.microsoft.com/mssql/server:2022-latest`) |
| xUnit | 2.9.3 |
| Testcontainers | 4.16.0 |

## Estrutura

```
src/
  Partners.Api              Controllers, contratos HTTP e tratamento de erros
  Partners.Core             Entidade, regras de validação e serviço (sem dependências de EF ou ASP.NET)
  Partners.Infrastructure   DbContext, configuração EF, migrações, dados de exemplo e repositório
tests/
  Partners.UnitTests        Regras de normalização e validação
  Partners.IntegrationTests Endpoints testados sobre SQL Server real
```

## Pré-requisitos

- .NET SDK 10
- Docker (para o SQL Server e para os testes de integração)

Também é possível usar uma instância local de SQL Server Developer ou Express (ver mais abaixo).

## Preparação

```bash
# 1. Iniciar o SQL Server
docker compose up -d

# 2. Restaurar a ferramenta dotnet-ef (definida em dotnet-tools.json)
dotnet tool restore

# 3. Criar a base de dados e aplicar as migrações (inclui dados de exemplo)
dotnet ef database update --project src/Partners.Infrastructure --startup-project src/Partners.Api
```

A connection string está em `src/Partners.Api/appsettings.json` (`ConnectionStrings:Partners`) e corresponde ao `docker-compose.yml`. Para usar outra instância, basta definir a variável de ambiente `ConnectionStrings__Partners`, por exemplo:

```powershell
# PowerShell, SQL Server local com autenticação Windows
$env:ConnectionStrings__Partners = "Server=localhost;Database=PartnersDb;Trusted_Connection=True;TrustServerCertificate=True"
```

```bash
# bash
export ConnectionStrings__Partners="Server=localhost,1433;Database=PartnersDb;User Id=sa;Password=...;TrustServerCertificate=True"
```

## Execução

```bash
dotnet run --project src/Partners.Api --launch-profile http
```

A API fica disponível em `http://localhost:5089`. Em ambiente de desenvolvimento, o documento OpenAPI está em `http://localhost:5089/openapi/v1.json`.

O ficheiro `src/Partners.Api/Partners.Api.http` tem exemplos de todos os endpoints, incluindo os casos de erro. Pode ser executado no Visual Studio, no Rider ou no VS Code (extensão REST Client).

## Testes

```bash
dotnet test
```

Os testes de integração arrancam um SQL Server num contentor Docker com Testcontainers e aplicam as migrações numa base de dados nova. Sem Docker, é possível apontar para um SQL Server existente; os testes criam e apagam uma base de dados temporária:

```powershell
$env:PARTNERS_TEST_SQLSERVER = "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test
```

## Endpoints

| Método | Rota | Resultado |
| --- | --- | --- |
| `POST` | `/api/partners` | `201` com `Location` e a entidade criada |
| `GET` | `/api/partners/{id}` | `200` (também para inativas) ou `404` |
| `GET` | `/api/partners` | `200` com `items`, `totalCount`, `page`, `pageSize` |
| `PUT` | `/api/partners/{id}` | `200` com a nova `version`, `404` ou `409` |

Parâmetros da listagem: `Search`, `Type`, `IncludeInactive` (por defeito `false`), `Page` (por defeito `1`, mínimo `1`) e `PageSize` (por defeito `20`, entre `1` e `100`).

Exemplo de entidade:

```json
{
  "id": "2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1001",
  "name": "Mercado Kianda, Lda",
  "taxNumber": "500100200",
  "type": "CUSTOMER",
  "email": "compras@kianda.example.com",
  "creditLimit": 50000.00,
  "isActive": true,
  "createdAt": "2026-01-15T09:00:00Z",
  "updatedAt": "2026-01-15T09:00:00Z",
  "version": "AAAAAAAAB9E="
}
```

### Erros

Todos os erros usam o formato Problem Details (`application/problem+json`), sem stack traces. Os erros de validação indicam o campo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "taxNumber": ["TaxNumber must have exactly 9 digits. Spaces, dots and hyphens are ignored."],
    "creditLimit": ["CreditLimit must have at most 2 decimal places."]
  },
  "traceId": "..."
}
```

| Código | Situação |
| --- | --- |
| `400` | Corpo, campos, filtros, Id ou `version` inválidos |
| `404` | Id inexistente |
| `409` | NIF já registado ou `version` desatualizada |

## Decisões de implementação

**NIF único.** O NIF é normalizado (sem espaços, pontos e hífenes) antes de ser guardado em `varchar(9)`, e existe um índice único em `TaxNumber`. A unicidade é garantida pela base de dados: quando dois pedidos tentam gravar o mesmo NIF ao mesmo tempo, o SQL Server rejeita o segundo (erros 2601/2627) e o repositório converte isso em `409`. Não há verificação prévia nem lock em memória. Como as entidades nunca são apagadas, o NIF continua reservado depois de desativar.

**Concorrência otimista.** A coluna `Version` é um `rowversion` do SQL Server, exposto como Base64. No `PUT`, a versão enviada pelo cliente é usada como valor original do token de concorrência do EF, e por isso o `UPDATE` é feito com `WHERE Id = @id AND Version = @version`. Se outra atualização já tiver alterado a linha, nenhuma linha é afetada e a API devolve `409` sem sobrescrever dados. Uma versão que não seja Base64 de 8 bytes devolve `400`.

**Atualização completa.** O `PUT` exige `name`, `taxNumber`, `type`, `creditLimit`, `isActive` e `version`. `email` pode ser `null` ou omitido, e nesse caso fica a `null`. `id`, `createdAt` e `updatedAt` enviados no corpo são ignorados. Todas as validações são feitas antes de qualquer escrita, e a gravação é um único `UPDATE`, por isso um pedido inválido nunca altera o registo parcialmente.

**Pesquisa.** `Search` procura por ocorrência parcial no nome, sem distinguir maiúsculas de minúsculas (a coluna usa a collation `Latin1_General_100_CI_AS`, independentemente da collation da base de dados). A pesquisa por NIF só é usada quando o termo, sem separadores, contém apenas dígitos. Assim, `Search=Ana` nunca se transforma num padrão vazio que corresponda a todos os NIF. Os caracteres `%` e `_` são tratados como texto.

**Listagem.** Filtros, contagem e paginação são executados no SQL Server. O `totalCount` é calculado antes de paginar e a ordenação é por `Name` e depois por `Id`. Uma página além dos resultados devolve `items` vazio com o `totalCount` correto.

**CreditLimit.** Guardado como `decimal(18,2)`. Valores fora de `0`–`1 000 000` ou com mais de duas casas decimais (por exemplo `10.999`) são rejeitados, em vez de arredondados.

**Datas.** `CreatedAt` e `UpdatedAt` são definidas pelo servidor em UTC. `CreatedAt` não muda nas atualizações.

## Cenários de teste

| Cenário | Testes |
| --- | --- |
| 1. Normalização, NIF duplicado e NIF reservado após desativação | `CreatePartnerTests` |
| 2. Entidade `BOTH` nos filtros `CUSTOMER`, `SUPPLIER` e `BOTH` | `ListPartnersTests.Partner_of_type_both_matches_every_type_filter` |
| 3. Desativar e reativar | `ListPartnersTests.Inactive_partner_is_only_listed_when_requested` |
| 4. Manter o próprio NIF ou usar o de outra entidade | `UpdatePartnerTests` |
| 5. Criações e atualizações simultâneas | `ConcurrencyTests` (via HTTP e com contextos EF independentes) |
| 6. Pedidos inválidos sem alterações na base de dados | `ValidationTests`, `ListPartnersTests.Invalid_filters_return_bad_request` |
| 7. Filtros combinados, `totalCount`, desempate por Id e `Search=Ana` | `ListPartnersTests` |
