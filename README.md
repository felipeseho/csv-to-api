# CSV to API - Processador de Arquivos CSV

Aplicação .NET 10 que processa arquivos CSV em lotes e envia os dados para uma API REST.

## ✨ Interface Visual Moderna com Spectre.Console

Este projeto utiliza a biblioteca [Spectre.Console](https://spectreconsole.net/) para oferecer uma experiência de console rica e visualmente atraente:

- 🎨 **Banner ASCII Art** estilizado
- 📊 **Dashboard de métricas** em tempo real
- 📈 **Barras de progresso** animadas
- 🎯 **Tabelas formatadas** para configurações e resultados
- 🌈 **Cores temáticas** para diferentes tipos de mensagens
- ⚡ **Spinners animados** durante operações

Veja detalhes completos em [SPECTRE_CONSOLE.md](SPECTRE_CONSOLE.md)

## Funcionalidades

- ✅ Leitura de arquivos CSV grandes em lotes configuráveis
- ✅ Validação de dados com regex e formatos de data
- ✅ Processamento paralelo para alta performance
- ✅ Chamadas HTTP (POST/PUT) para API REST
- ✅ Log de erros com informações detalhadas (linha, HTTP code, mensagem)
- ✅ Suporte a atributos aninhados no payload da API (ex: `address.street`)
- ✅ Configuração via arquivo YAML
- ✅ Autenticação Bearer Token
- ✅ **Argumentos de linha de comando para sobrescrever configurações**
- ✅ **Interface visual moderna e interativa**

## Requisitos

- .NET 10 SDK
- Arquivo de configuração YAML

## Instalação

```bash
dotnet restore
dotnet build
```

## Uso

### Ajuda e Opções Disponíveis
```bash
dotnet run -- --help
```

### Execução básica (usando config.yaml padrão)
```bash
dotnet run
```

### Execução com arquivo de configuração customizado
```bash
dotnet run -- --config /caminho/para/config.yaml
# ou forma curta
dotnet run -- -c /caminho/para/config.yaml
```

### Sobrescrever configurações via argumentos
```bash
# Sobrescrever arquivo CSV de entrada
dotnet run -- --input data/outro-arquivo.csv

# Sobrescrever endpoint da API
dotnet run -- --endpoint https://api.producao.com/upload

# Sobrescrever múltiplas configurações
dotnet run -- \
  --config config.yaml \
  --input data/vendas.csv \
  --batch-lines 500 \
  --endpoint https://api.example.com/vendas \
  --auth-token "Bearer xyz123" \
  --timeout 60 \
  --verbose

# Processar com logs detalhados
dotnet run -- --verbose
```

### Execução do executável compilado
```bash
./bin/Debug/net10.0/CsvToApi --help
./bin/Debug/net10.0/CsvToApi --config /caminho/para/config.yaml
./bin/Debug/net10.0/CsvToApi -i data/input.csv -e https://api.com/upload -v
```

## Argumentos de Linha de Comando

Todos os argumentos são opcionais e sobrescrevem as configurações do arquivo YAML:

| Argumento | Forma Curta | Descrição | Exemplo |
|-----------|-------------|-----------|---------||
| `--config` | `-c` | Caminho do arquivo de configuração YAML | `--config config.yaml` |
| `--input` | `-i` | Caminho do arquivo CSV de entrada | `--input data/vendas.csv` |
| `--batch-lines` | `-b` | Número de linhas por lote | `--batch-lines 500` |
| `--start-line` | `-s` | Linha inicial para começar o processamento | `--start-line 100` |
| `--max-lines` | `-n` | Número máximo de linhas a processar | `--max-lines 1000` |
| `--log-path` | `-l` | Caminho do arquivo de log | `--log-path logs/erros.log` |
| `--delimiter` | `-d` | Delimitador do CSV | `--delimiter ";"` |
| `--endpoint` | `-e` | URL do endpoint da API | `--endpoint https://api.com/upload` |
| `--auth-token` | `-a` | Token de autenticação Bearer | `--auth-token "Bearer xyz123"` |
| `--method` | `-m` | Método HTTP (POST ou PUT) | `--method PUT` |
| `--timeout` | `-t` | Timeout das requisições (segundos) | `--timeout 60` |
| `--verbose` | `-v` | Exibir logs detalhados | `--verbose` |

### Exemplos Práticos

**Processar arquivo diferente mantendo outras configurações:**
```bash
dotnet run -- -i data/clientes-2024.csv -v
```

**Usar configuração de produção com endpoint específico:**
```bash
dotnet run -- -c config.prod.yaml -e https://prod.api.com/data
```

**Teste rápido com lotes pequenos e timeout maior:**
```bash
dotnet run -- -b 10 -t 120 -v
```

**Processar arquivo com delimitador ponto-e-vírgula:**
```bash
dotnet run -- -i data/export.csv -d ";" -v
```

**Continuar processamento a partir de uma linha específica:**
```bash
# Útil para retomar processamento após falha
dotnet run -- -i data/vendas.csv -s 1001 -v
```

**Processar apenas as primeiras N linhas (útil para testes):**
```bash
# Processar apenas as primeiras 100 linhas
dotnet run -- -i data/vendas.csv -n 100 -v

# Processar um intervalo específico (ex: linhas 101-200)
dotnet run -- -i data/vendas.csv -s 101 -n 100 -v
```

## Configuração (config.yaml)

```yaml
file:
    inputPath: "data/input.csv"           # Caminho do arquivo CSV
    batchLines: 100                       # Número de linhas por lote
    startLine: 1                          # Linha inicial (padrão: 1)
    maxLines: 1000                        # Número máximo de linhas a processar (opcional)
    logPath: "logs/process.log"           # Arquivo de log de erros
    csvDelimiter: ","                     # Delimitador do CSV
    mapping:                              # Validações de colunas
        - column: "Name"
          type: "string"
        - column: "Email"
          type: "string"
          regex: "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"
        - column: "Birthdate"
          type: "date"
          format: "YYYY-MM-DD"

api:
    endpointUrl: "https://api.example.com/upload"
    authToken: "your_auth_token_here"     # Token de autenticação (opcional)
    method: "POST"                        # POST ou PUT
    requestTimeout: 30                    # Timeout em segundos
    mapping:                              # Mapeamento CSV -> API
      - attribute: "name"
        csvColumn: "Name"                 # Valor vem da coluna CSV
      - attribute: "email"
        csvColumn: "Email"
      - attribute: "address.street"       # Suporta atributos aninhados
        csvColumn: "Street"
      - attribute: "birthdate"
        csvColumn: "Birthdate"
      # Parâmetros com valores fixos (não vêm do CSV)
      - attribute: "source"
        fixedValue: "csv-import"          # Valor fixo para todos os registros
      - attribute: "version"
        fixedValue: "1.0"
```

## Formato do Arquivo de Log

Quando ocorrem erros, o arquivo de log contém:
- **LineNumber**: Número da linha no arquivo CSV original
- **Todas as colunas do CSV original**: Valores exatos da linha com erro
- **HttpCode**: Código HTTP do erro (400 para validação, 500 para exceções)
- **ErrorMessage**: Descrição do erro

Exemplo:
```csv
LineNumber,Name,Email,Street,Birthdate,HttpCode,ErrorMessage
5,John Doe,invalid-email,123 Main St,1990-05-15,400,"Valor 'invalid-email' inválido para coluna 'Email'"
8,Jane Smith,jane@example.com,456 Oak Ave,2025-13-45,400,"Data '2025-13-45' inválida para formato 'YYYY-MM-DD' na coluna 'Birthdate'"
```

## Estrutura do Projeto

```
CsvToApi/
├── Program.cs           # Código principal (top-level statements)
├── config.yaml          # Arquivo de configuração
├── CsvToApi.csproj      # Arquivo do projeto
├── data/
│   └── input.csv        # Arquivo CSV de entrada
└── logs/
    └── process.log      # Log de erros
```

## Performance

A aplicação foi otimizada para processar grandes volumes de dados:

1. **Processamento em lotes**: Evita carregar todo o arquivo na memória
2. **Paralelismo**: Múltiplas chamadas HTTP simultâneas
3. **Thread-safe**: Logging seguro com SemaphoreSlim
4. **Async/await**: Operações I/O não-bloqueantes

## Validações Suportadas

- **type: "string"**: Qualquer texto
- **type: "date"**: Valida formato de data
  - format: "YYYY-MM-DD", "DD/MM/YYYY", etc.
- **regex**: Validação com expressão regular customizada

## Exemplos de Payload da API

### Payload com dados do CSV e valores fixos

Com a configuração acima, cada linha do CSV gera um payload como:

```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "address": {
    "street": "123 Main St"
  },
  "birthdate": "1990-05-15",
  "source": "csv-import",
  "version": "1.0"
}
```

### Diferença entre csvColumn e fixedValue

No mapeamento da API, você pode usar:

- **csvColumn**: O valor vem da coluna correspondente no CSV (diferente para cada linha)
  ```yaml
  - attribute: "name"
    csvColumn: "Name"  # Valor varia por linha
  ```

- **fixedValue**: O valor é fixo para todos os registros (mesmo valor em todas as linhas)
  ```yaml
  - attribute: "source"
    fixedValue: "csv-import"  # Sempre "csv-import"
  ```

**Importante**: Cada mapping deve ter **OU** `csvColumn` **OU** `fixedValue`, mas não ambos.

## Tratamento de Erros

A aplicação registra erros em três situações:

1. **Validação de dados**: Regex ou formato inválido (HTTP 400)
2. **Erro na API**: Response não-sucesso (HTTP code real da API)
3. **Exceções**: Timeout, conexão, etc. (HTTP 500)

## Dependências

- **YamlDotNet**: Leitura de arquivos YAML
- **CsvHelper**: Processamento eficiente de CSV
- **System.CommandLine**: Parsing robusto de argumentos CLI

## Licença

MIT

