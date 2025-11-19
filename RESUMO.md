# 🚀 CsvToApi - Solução Completa Implementada

## ✅ Funcionalidades Implementadas

### 1. **Processamento de Arquivos CSV em Lotes**
- ✅ Leitura eficiente de arquivos grandes
- ✅ Processamento em lotes configuráveis (`batchLines`)
- ✅ Não carrega todo arquivo na memória
- ✅ Suporte a diferentes delimitadores CSV

### 2. **Validação de Dados**
- ✅ Validação por tipo (string, date)
- ✅ Validação com regex customizável
- ✅ Validação de formato de data (ex: YYYY-MM-DD, DD/MM/YYYY)
- ✅ Detecção de colunas faltantes

### 3. **Chamadas de API**
- ✅ Suporte HTTP POST e PUT
- ✅ Autenticação Bearer Token
- ✅ Timeout configurável
- ✅ Payload JSON dinâmico
- ✅ Suporte a atributos aninhados (ex: `address.street`)

### 4. **Logging de Erros**
- ✅ Registro do número da linha com erro
- ✅ Todas as colunas originais do CSV
- ✅ HTTP Code do erro
- ✅ Mensagem de erro detalhada
- ✅ Thread-safe (múltiplas threads podem escrever simultaneamente)

### 5. **Performance**
- ✅ Processamento paralelo de requisições HTTP
- ✅ Async/await para operações I/O
- ✅ SemaphoreSlim para sincronização eficiente
- ✅ Lotes configuráveis para otimizar throughput

### 6. **Configuração via YAML**
- ✅ Todos os parâmetros externalizados
- ✅ Fácil manutenção sem recompilação
- ✅ Múltiplos arquivos de configuração suportados
- ✅ Validação de configuração no início

### 7. **Recursos do .NET 10**
- ✅ Top-level statements (arquivo único)
- ✅ ImplicitUsings habilitado
- ✅ Nullable reference types
- ✅ Target framework net10.0

## 📁 Estrutura de Arquivos

```
CsvToApi/
├── build.sh                      # Script de build
├── README.md                     # Documentação principal
├── EXEMPLOS.md                   # Exemplos de uso
├── CsvToApi/
│   ├── Program.cs                # ⭐ Código principal (um arquivo único!)
│   ├── CsvToApi.csproj           # Configuração do projeto
│   ├── config.yaml               # Configuração padrão
│   ├── config-validation-test.yaml  # Config para testes
│   ├── data/
│   │   ├── input.csv             # CSV de exemplo
│   │   └── test-validation.csv   # CSV para teste de validação
│   └── logs/
│       ├── process.log           # Log de erros padrão
│       └── validation-test.log   # Log de teste
```

## 🎯 Como Usar

### Instalação
```bash
cd CsvToApi
dotnet restore
dotnet build
```

### Execução Básica
```bash
cd CsvToApi
dotnet run
```

### Execução com Config Customizado
```bash
cd CsvToApi
dotnet run -- /caminho/para/config.yaml
```

### Build para Produção
```bash
./build.sh
```

## 🔧 Configuração (config.yaml)

```yaml
file:
    inputPath: "data/input.csv"
    batchLines: 100
    logPath: "logs/process.log"
    csvDelimiter: ","
    mapping:
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
    authToken: "your_auth_token_here"
    method: "POST"
    requestTimeout: 30
    mapping:
      - attribute: "name"
        csvColumn: "Name"
      - attribute: "address.street"  # Atributos aninhados!
        csvColumn: "Street"
```

## 📊 Exemplo de Output

```
Processadas 100 linhas. Erros: 5
Processadas 200 linhas. Erros: 12
Processadas 300 linhas. Erros: 15

Total de linhas processadas: 300
Total de erros: 15
Processamento concluído!
```

## 📝 Formato do Log de Erros

```csv
LineNumber,Name,Email,Street,Birthdate,HttpCode,ErrorMessage
3,Invalid Email,not-an-email,456 Oak Ave,1985-08-22,400,Valor 'not-an-email' inválido para coluna 'Email'
4,Invalid Date,test@example.com,789 Pine Rd,1992-13-99,400,Data '1992-13-99' inválida para formato 'YYYY-MM-DD' na coluna 'Birthdate'
```

## 🚀 Recursos Avançados

### 1. Atributos Aninhados
```yaml
mapping:
  - attribute: "user.profile.name"
    csvColumn: "Name"
```

Gera:
```json
{
  "user": {
    "profile": {
      "name": "John Doe"
    }
  }
}
```

### 2. Validação com Regex
```yaml
- column: "CPF"
  type: "string"
  regex: "^\\d{3}\\.\\d{3}\\.\\d{3}-\\d{2}$"
```

### 3. Validação de Datas
```yaml
- column: "DataNascimento"
  type: "date"
  format: "DD/MM/YYYY"
```

### 4. Processamento Paralelo
O processamento de cada lote é feito em paralelo automaticamente para máxima performance!

## 📦 Dependências

- **YamlDotNet** (16.2.0): Parser YAML
- **CsvHelper** (33.0.1): Processamento CSV eficiente

## 🎓 Conceitos Aplicados

1. **Async/Await**: Todas operações I/O são assíncronas
2. **Paralelismo**: Task.WhenAll para processar múltiplas requisições
3. **Thread-Safety**: SemaphoreSlim para logging thread-safe
4. **SOLID**: Separação de responsabilidades
5. **Performance**: Streaming de arquivo, não carrega tudo na memória
6. **Error Handling**: Try/catch com logging detalhado

## 🧪 Testes Realizados

✅ Processamento de arquivo CSV válido
✅ Validação de email com regex
✅ Validação de data com formato customizado
✅ Log de erros com todas as colunas
✅ Processamento em lotes
✅ Configuração via YAML
✅ Build em Release mode

## 📈 Performance Esperada

- **Arquivos pequenos** (< 1000 linhas): < 10 segundos
- **Arquivos médios** (1000-10000 linhas): < 1 minuto
- **Arquivos grandes** (10000-100000 linhas): 5-10 minutos
- **Arquivos muito grandes** (> 100000 linhas): Ajustar batchLines

*Performance depende da velocidade da API e da largura de banda*

## 🔐 Segurança

- ✅ Autenticação Bearer Token
- ✅ Timeout para evitar travamentos
- ✅ Validação de entrada
- ✅ Escape de caracteres especiais no log

## 📚 Documentação Adicional

- **README.md**: Documentação completa
- **EXEMPLOS.md**: Exemplos práticos de uso
- Este arquivo: Resumo da implementação

## 🎉 Pronto para Uso em Produção!

A solução está completa e pronta para processar arquivos CSV em produção com:
- Alta performance
- Tratamento robusto de erros
- Logging detalhado
- Configuração flexível
- Código limpo e manutenível

