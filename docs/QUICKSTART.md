<div align="center">
  <h1>🚀 Guia de Início Rápido</h1>
  <p><strong>Configure e execute o n2n em 5 minutos</strong></p>
</div>

---

## ⚡ Configuração em 5 Minutos

### 1️⃣ Verificar Pré-requisitos

```bash
dotnet --version
# Deve retornar 10.0.x ou superior
```

**Requisitos:**
- ✅ [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) ou superior
- ✅ Acesso ao Terminal/Prompt de Comando

---

### 2️⃣ Navegar para o Projeto

```bash
cd /caminho/para/n2n
```

---

### 3️⃣ Restaurar Dependências

```bash
dotnet restore
```

---

### 4️⃣ Executar a Aplicação

```bash
dotnet run
```

✅ **Pronto!** A aplicação processará o arquivo `config.yaml` padrão.

---

## 🧪 Testando com APIs Reais

### Opção 1: Webhook.site (Recomendado para Testes)

**Perfeito para testar sem configurar um servidor de API real:**

1. **Acesse** [https://webhook.site](https://webhook.site)
2. **Copie** sua URL única
3. **Execute** com a URL como argumento:

```bash
dotnet run -- --endpoint "https://webhook.site/SUA-URL-UNICA" --verbose
```

4. **Acompanhe** as requisições chegando em tempo real no webhook.site

**Alternativa:** Edite o `config.yaml`:

```yaml
endpoints:
  - name: "teste"
    endpointUrl: "https://webhook.site/SUA-URL-UNICA"
    method: "POST"
    mapping:
      - attribute: "name"
        csvColumn: "Name"
      - attribute: "email"
        csvColumn: "Email"
```

Depois execute:

```bash
dotnet run --verbose
```

---

### Opção 2: Argumentos de Linha de Comando

**Sobrescreva configurações sem editar arquivos:**

```bash
# Teste rápido
dotnet run -- --input data/sample.csv --batch-lines 10 --verbose

# Modo dry-run (teste sem requisições reais)
dotnet run -- --dry-run --verbose

# Usar endpoint específico
dotnet run -- --endpoint-name producao --verbose

# Combinar múltiplas opções
dotnet run -- \
  --input data/vendas.csv \
  --endpoint-name producao \
  --batch-lines 500 \
  --verbose
```

**Ver todas as opções disponíveis:**

```bash
dotnet run -- --help
```

---

### Opção 3: Seu Próprio Endpoint de API

**Edite o `config.yaml` com suas configurações:**

```yaml
file:
  inputPath: "data/seu-arquivo.csv"
  batchLines: 100
  logDirectory: "logs"
  csvDelimiter: ","

endpoints:
  - name: "minha-api"
    endpointUrl: "https://sua-api.com/endpoint"
    headers:
      Authorization: "Bearer seu-token-aqui"
      X-Custom-Header: "seu-valor"
    method: "POST"
    mapping:
      - attribute: "nome"
        csvColumn: "Nome"
        transform: "title-case"
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"
```

---

## 📁 Estrutura Mínima Necessária

```
n2n/
├── src/
│   ├── Program.cs          ✅ Código principal
│   └── n2n.csproj          ✅ Arquivo do projeto
├── config.yaml             ✅ Configuração
└── data/
    └── input.csv           ✅ Seu arquivo CSV
```

---

## 📄 Exemplo de Arquivo CSV

Crie `data/sample.csv`:

```csv
Nome,Email,Telefone
João Silva,joao@exemplo.com,+5511987654321
Maria Santos,maria@exemplo.com,+5511876543210
Pedro Costa,pedro@exemplo.com,+5511765432109
```

---

## ⚙️ Exemplo de Configuração Mínima

Crie `config.yaml`:

```yaml
file:
  inputPath: "data/sample.csv"
  batchLines: 100
  logDirectory: "logs"
  csvDelimiter: ","
  checkpointDirectory: "checkpoints"
  mapping: []

endpoints:
  - name: "default"
    endpointUrl: "https://webhook.site/SUA-URL"
    method: "POST"
    headers:
      Authorization: "Bearer seu-token-aqui"
    requestTimeout: 30
    mapping:
      - attribute: "nome"
        csvColumn: "Nome"
        transform: "title-case"
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"
      - attribute: "telefone"
        csvColumn: "Telefone"
```

---

## 🎯 Executar e Ver Resultados

```bash
dotnet run --verbose
```

**Saída Esperada:**

```
╔══════════════════════════════════════════════════════════╗
║                    n2n - CSV to API                      ║
║              Processa CSV → Envia para API REST          ║
╚══════════════════════════════════════════════════════════╝

✓ Configuração carregada
✓ Arquivo CSV encontrado: data/sample.csv
✓ Processamento iniciado...

📊 Progresso: 100% [████████████████████] 3/3 linhas

✓ Processamento concluído!
  • Total de linhas: 3
  • Sucessos: 3
  • Erros: 0
```

---

## 📋 Ver Logs de Erro (se houver)

```bash
cat logs/process_[execution-id].log
```

---

## 🏗️ Compilar para Produção

### macOS (ARM64 - M1/M2/M3)

```bash
dotnet publish -c Release -r osx-arm64 --self-contained
./bin/Release/net10.0/osx-arm64/publish/n2n --help
```

### macOS (Intel x64)

```bash
dotnet publish -c Release -r osx-x64 --self-contained
./bin/Release/net10.0/osx-x64/publish/n2n --help
```

### Linux (x64)

```bash
dotnet publish -c Release -r linux-x64 --self-contained
./bin/Release/net10.0/linux-x64/publish/n2n --help
```

### Windows (x64)

```bash
dotnet publish -c Release -r win-x64 --self-contained
.\bin\Release\net10.0\win-x64\publish\n2n.exe --help
```

---

## 🛠️ Comandos Úteis

```bash
# Ver progresso detalhado
dotnet run -- --verbose

# Testar sem fazer requisições reais (dry-run)
dotnet run -- --dry-run --verbose

# Usar arquivo de configuração específico
dotnet run -- --config minha-config.yaml

# Processar apenas primeiras 100 linhas
dotnet run -- --max-lines 100 --verbose

# Usar endpoint específico
dotnet run -- --endpoint-name producao --verbose

# Continuar a partir de checkpoint
dotnet run -- --execution-id abc-123-def-456 --verbose

# Build de release
dotnet build -c Release

# Limpar build
dotnet clean
```

---

## 🐛 Solução Rápida de Problemas

### ❌ "Arquivo CSV não encontrado"

```bash
# Verificar se o arquivo existe
ls -la data/input.csv

# Usar caminho absoluto no config.yaml
inputPath: "/caminho/completo/para/arquivo.csv"
```

### ❌ "URL do endpoint não configurada"

```bash
# Verificar config.yaml
cat config.yaml | grep endpointUrl
```

### ❌ Falha no build

```bash
# Limpar e reconstruir
dotnet clean
dotnet restore
dotnet build
```

### ❌ Erro de autenticação (401)

- Verifique o header `Authorization` no `config.yaml`
- Confirme que seu token não expirou
- Valide se a URL do endpoint está correta

### ❌ Timeout na conexão

- Aumente o `requestTimeout` na configuração do endpoint
- Verifique sua conexão com a internet
- Confirme se o endpoint da API está acessível

---

## 🎓 Próximos Passos

Depois de ter o teste básico funcionando, explore mais recursos:

1. ✅ **Entender o básico** → Leia [README.md](../README.md)
2. 🎨 **Usar transformações** → Leia [TRANSFORMATIONS.md](TRANSFORMATIONS.md)
3. 🔍 **Configurar filtros** → Leia [FILTERS.md](FILTERS.md)
4. ⚙️ **Argumentos CLI** → Leia [CLI-ARGUMENTS.md](CLI-ARGUMENTS.md)
5. 💡 **Exemplos do mundo real** → Leia [EXAMPLES.md](EXAMPLES.md)
6. 🔧 **Configuração avançada** → Customize o `config.yaml`

---

## 📚 Documentação

- 📖 [Documentação Principal](../README.md)
- 💡 [Exemplos](EXAMPLES.md)
- 🎨 [Transformações](TRANSFORMATIONS.md)
- 🔍 [Filtros](FILTERS.md)
- ⚙️ [Argumentos CLI](CLI-ARGUMENTS.md)
- 📝 [Changelog](CHANGELOG.md)

---

## 💡 Dicas Profissionais

- 💡 **Sempre teste com `--dry-run` primeiro** antes de processar arquivos grandes
- 💡 **Use `--verbose` para debug** e ver progresso detalhado
- 💡 **Comece com `--batch-lines` pequeno** (ex: 10) ao testar
- 💡 **Use checkpoints** para retomar processamento após falhas
- 💡 **Monitore os logs** no diretório `logs/` para erros

---

<div align="center">
  <p><strong>⏱️ Tempo estimado para primeiro teste: 5 minutos</strong></p>
  <p>
    <a href="#-configuração-em-5-minutos">Voltar ao topo ⬆️</a>
  </p>
</div>
