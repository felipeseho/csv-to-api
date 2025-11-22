<div align="center">
  <h1>⚙️ Argumentos de Linha de Comando</h1>
  <p><strong>Referência completa de todos os argumentos CLI disponíveis</strong></p>
</div>

---

## 📋 Visão Geral

Os argumentos de linha de comando permitem sobrescrever configurações do arquivo YAML sem precisar editá-lo. Isso é especialmente útil para:

- 🧪 **Testes rápidos** com diferentes configurações
- 🔄 **Scripts automatizados** e pipelines CI/CD
- 🎯 **Processamento ad-hoc** de arquivos específicos
- 🐛 **Debug e troubleshooting**

---

## 🔍 Ver Todas as Opções

```bash
dotnet run -- --help
```

---

## 📊 Tabela de Referência Completa

| Argumento | Atalho | Descrição | Tipo | Exemplo |
|-----------|--------|-----------|------|---------|
| `--config` | `-c` | Arquivo de configuração YAML | String | `--config config.yaml` |
| `--input` | `-i` | Arquivo CSV de entrada | String | `--input data/vendas.csv` |
| `--batch-lines` | `-b` | Linhas processadas por lote | Number | `--batch-lines 500` |
| `--start-line` | `-s` | Linha inicial do processamento | Number | `--start-line 100` |
| `--max-lines` | `-n` | Máximo de linhas a processar | Number | `--max-lines 1000` |
| `--log-dir` | `-l` | Diretório de logs | String | `--log-dir logs/prod` |
| `--delimiter` | `-d` | Delimitador do CSV | String | `--delimiter ";"` |
| `--execution-id` | `--exec-id` | UUID para continuar checkpoint | String | `--exec-id abc-123...` |
| `--endpoint-name` | | Nome do endpoint a ser usado | String | `--endpoint-name producao` |
| `--verbose` | `-v` | Exibir logs detalhados | Flag | `--verbose` |
| `--dry-run` | `--test` | Testar sem fazer requisições | Flag | `--dry-run` |

---

## 🎯 Argumentos Detalhados

### 📁 Arquivo de Configuração (`--config`, `-c`)

Especifica qual arquivo YAML usar para configurações.

```bash
# Usar configuração de produção
dotnet run -- --config config-prod.yaml

# Usar configuração de teste
dotnet run -- -c config-test.yaml
```

**Padrão:** `config.yaml`

---

### 📄 Arquivo CSV de Entrada (`--input`, `-i`)

Sobrescreve o arquivo CSV a ser processado.

```bash
# Processar arquivo específico
dotnet run -- --input data/vendas-janeiro.csv

# Forma curta
dotnet run -- -i data/clientes.csv
```

**Sobrescreve:** `file.inputPath` no YAML

---

### 📦 Linhas por Lote (`--batch-lines`, `-b`)

Define quantas linhas processar por vez.

```bash
# Processar 500 linhas por vez
dotnet run -- --batch-lines 500

# Lotes menores para APIs lentas
dotnet run -- -b 50
```

**Recomendações:**
- 🐢 **APIs lentas:** 10-50 linhas
- ⚡ **APIs rápidas:** 500-1000 linhas
- 🗄️ **Arquivos grandes:** 200-500 linhas

**Sobrescreve:** `file.batchLines` no YAML

---

### 🎯 Linha Inicial (`--start-line`, `-s`)

Define a partir de qual linha começar o processamento.

```bash
# Começar a partir da linha 100
dotnet run -- --start-line 100

# Retomar após falha
dotnet run -- -s 1001 -v
```

**Casos de uso:**
- 🔄 Retomar processamento após falha
- 🧪 Testar intervalo específico de dados
- 📊 Processar arquivo em partes

**Sobrescreve:** `file.startLine` no YAML

---

### 🔢 Máximo de Linhas (`--max-lines`, `-n`)

Limita quantas linhas processar no total.

```bash
# Processar apenas primeiras 1000 linhas
dotnet run -- --max-lines 1000

# Teste rápido com 10 linhas
dotnet run -- -n 10 -v
```

**Combinar com `--start-line`:**

```bash
# Processar linhas 101 a 200
dotnet run -- -s 101 -n 100 -v
```

**Sobrescreve:** `file.maxLines` no YAML

---

### 📝 Diretório de Logs (`--log-dir`, `-l`)

Define onde salvar os arquivos de log.

```bash
# Logs em diretório específico
dotnet run -- --log-dir logs/producao

# Separar logs por ambiente
dotnet run -- -l logs/teste
```

**Sobrescreve:** `file.logDirectory` no YAML

---

### 🔤 Delimitador CSV (`--delimiter`, `-d`)

Define o caractere delimitador do CSV.

```bash
# Usar ponto-e-vírgula
dotnet run -- --delimiter ";"

# Usar pipe
dotnet run -- -d "|"

# Usar tab
dotnet run -- -d "\t"
```

**Padrão:** `,` (vírgula)

**Sobrescreve:** `file.csvDelimiter` no YAML

---

### 🔑 Execution ID (`--execution-id`, `--exec-id`)

Continua uma execução existente usando seu UUID.

```bash
# Nova execução (gera UUID automaticamente)
dotnet run

# Saída: Execution ID: 6869cdf3-5fb0-4178-966d-9a21015ffb4d

# Continuar execução existente
dotnet run -- --execution-id 6869cdf3-5fb0-4178-966d-9a21015ffb4d

# Forma curta
dotnet run -- --exec-id 6869cdf3-5fb0-4178-966d-9a21015ffb4d -v
```

**Benefícios:**
- 📊 Retomar processamento após falha
- 🔍 Manter logs organizados por execução
- 💾 Usar checkpoints salvos

**Arquivos gerados por execução:**
- `logs/process_{uuid}.log`
- `checkpoints/checkpoint_{uuid}.json`

---

### 🌐 Nome do Endpoint (`--endpoint-name`)

Seleciona qual endpoint configurado usar.

```bash
# Usar endpoint de produção
dotnet run -- --endpoint-name producao

# Usar endpoint de teste
dotnet run -- --endpoint-name homologacao -v
```

**Prioridade:**
1. Argumento `--endpoint-name` (maior)
2. Coluna CSV (se `endpointColumnName` configurado)
3. `defaultEndpoint` no YAML
4. Endpoint único (se houver apenas um)

**Sobrescreve:** Seleção dinâmica de endpoint

---

### 🔊 Modo Verbose (`--verbose`, `-v`)

Exibe logs detalhados durante o processamento.

```bash
# Logs detalhados
dotnet run -- --verbose

# Forma curta
dotnet run -- -v

# Combinar com outros argumentos
dotnet run -- -i data/vendas.csv -v
```

**Mostra:**
- ✅ Progresso linha por linha
- 📊 Métricas em tempo real
- 🔍 Detalhes de requisições HTTP
- ⚠️ Avisos e validações

---

### 🧪 Modo Dry-Run (`--dry-run`, `--test`)

Testa a configuração sem fazer requisições HTTP reais.

```bash
# Modo dry-run
dotnet run -- --dry-run

# Alias
dotnet run -- --test

# Combinar com verbose
dotnet run -- --dry-run --verbose

# Testar subset de dados
dotnet run -- --dry-run -n 100 -v
```

**Útil para:**
- ✅ Validar configuração
- ✅ Testar filtros e transformações
- ✅ Verificar mapeamento de dados
- ✅ Estimar tempo de processamento

---

## 💡 Exemplos Práticos

### Teste Rápido de Desenvolvimento

```bash
dotnet run -- \
  -i data/test.csv \
  --endpoint-name desenvolvimento \
  -b 10 \
  -n 50 \
  --dry-run \
  -v
```

### Processamento de Produção

```bash
dotnet run -- \
  --config config-prod.yaml \
  --input data/vendas-diarias.csv \
  --endpoint-name producao \
  --batch-lines 1000 \
  --verbose
```

### Teste com Webhook.site

```bash
dotnet run -- \
  -i data/sample.csv \
  --endpoint-name webhook1 \
  -b 5 \
  -n 20 \
  -v
```

### Retomar Processamento Após Falha

```bash
# Usar o mesmo execution-id
dotnet run -- \
  --execution-id abc-123-def-456 \
  --verbose
```

### Processar Intervalo Específico

```bash
# Linhas 1001 a 2000
dotnet run -- \
  -i data/arquivo-grande.csv \
  -s 1001 \
  -n 1000 \
  --endpoint-name producao \
  -v
```

### Debug de Arquivo CSV Novo

```bash
dotnet run -- \
  --input data/novo-arquivo.csv \
  --delimiter ";" \
  --batch-lines 5 \
  --max-lines 10 \
  --dry-run \
  --verbose
```

### Processar com Configuração Específica

```bash
dotnet run -- \
  --config config-cliente-x.yaml \
  --input data/cliente-x/dados.csv \
  --log-dir logs/cliente-x \
  --verbose
```

---

## ⚡ Combinações Poderosas

### Teste Completo sem Requisições

```bash
dotnet run -- -i data/test.csv -n 100 --dry-run -v
```

### Processamento em Partes

```bash
# Parte 1: Linhas 1-1000
dotnet run -- -i data/grande.csv -s 1 -n 1000 -v

# Parte 2: Linhas 1001-2000
dotnet run -- -i data/grande.csv -s 1001 -n 1000 -v

# Parte 3: Linhas 2001-3000
dotnet run -- -i data/grande.csv -s 2001 -n 1000 -v
```

### Debug com Logs Organizados

```bash
dotnet run -- \
  --input data/problematico.csv \
  --log-dir logs/debug \
  --batch-lines 1 \
  --max-lines 10 \
  --verbose
```

---

## 🔄 Prioridade das Configurações

A ordem de prioridade (maior para menor):

1. 🥇 **Argumentos de linha de comando**
2. 🥈 **Arquivo YAML especificado em `--config`**
3. 🥉 **Arquivo `config.yaml` padrão**

**Exemplo:**

```yaml
# config.yaml
file:
  batchLines: 100
  inputPath: "data/default.csv"
```

```bash
# Este comando usa:
# - batchLines: 500 (argumento CLI)
# - inputPath: "data/custom.csv" (argumento CLI)
dotnet run -- -i data/custom.csv -b 500
```

---

## 📝 Dicas e Boas Práticas

### ✅ Recomendações

- 💡 **Sempre use `-v` para desenvolvimento** e troubleshooting
- 💡 **Teste com `--dry-run`** antes de processar arquivos grandes
- 💡 **Use `--batch-lines` pequeno** (10-50) para primeiros testes
- 💡 **Combine `-s` e `-n`** para processar intervalos específicos
- 💡 **Salve execution-id** para retomar processamentos longos

### ⚠️ Cuidados

- ❌ **Não use `--dry-run` em produção** - não faz requisições reais
- ❌ **Cuidado com `-n` muito alto** em primeiros testes
- ❌ **Verifique delimitador** antes de processar arquivos novos
- ❌ **Confirme endpoint** antes de processar dados sensíveis

---

## 🔧 Scripts e Automação

### Bash Script Exemplo

```bash
#!/bin/bash

# Script para processar múltiplos arquivos
for file in data/*.csv; do
  echo "Processando: $file"
  dotnet run -- \
    --input "$file" \
    --endpoint-name producao \
    --batch-lines 500 \
    --verbose
done
```

### Cron Job Exemplo

```bash
# Executar todos os dias às 2h da manhã
0 2 * * * cd /path/to/n2n && dotnet run -- -c config-daily.yaml -v >> /var/log/n2n-cron.log 2>&1
```

---

## 📚 Documentação Relacionada

- 📖 [README Principal](../README.md)
- 🚀 [Quick Start](QUICKSTART.md)
- 💡 [Exemplos Práticos](EXAMPLES.md)
- 🎨 [Transformações](TRANSFORMATIONS.md)
- 🔍 [Filtros](FILTERS.md)

---

<div align="center">
  <p><strong>💡 Dica: Combine argumentos para máxima flexibilidade!</strong></p>
  <p>
    <a href="#-visão-geral">Voltar ao topo ⬆️</a>
  </p>
</div>
