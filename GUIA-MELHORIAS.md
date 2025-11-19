# 🎯 Guia de Uso: Melhorias de Alta Prioridade

Este documento descreve como usar as novas funcionalidades de alta prioridade implementadas no CsvToApi.

## 📋 Índice

1. [Retry Policy](#retry-policy)
2. [Rate Limiting](#rate-limiting)
3. [Modo Dry Run](#modo-dry-run)
4. [Checkpoint/Resume](#checkpointresume)

---

## 🔄 Retry Policy

### O que é?
Tentativas automáticas de reenvio quando ocorrem erros temporários (timeouts, erros 5xx do servidor).

### Como Configurar

No arquivo `config.yaml`:

```yaml
api:
    retryAttempts: 3           # Número de tentativas (padrão: 3)
    retryDelaySeconds: 5       # Delay entre tentativas em segundos (padrão: 5)
```

### Quando é Ativado

O retry acontece automaticamente para:
- **Erros HTTP 5xx** (500, 502, 503, 504)
- **Timeout de requisição** (408 Request Timeout)
- **Exceções de rede** (HttpRequestException, TaskCanceledException)

### Comportamento

```
Tentativa 1 → Falha (HTTP 502)
⏳ Aguarda 5 segundos...
Tentativa 2 → Falha (HTTP 503)
⏳ Aguarda 5 segundos...
Tentativa 3 → Sucesso ✅
```

### Exemplo de Saída

```
Tentativa 1/3 falhou (HTTP 502). Aguardando 5s...
Tentativa 2/3 falhou (HTTP 503). Aguardando 5s...
Processadas 100 linhas. Sucessos: 98, Erros: 2
```

### Dicas

- Para APIs instáveis, use `retryAttempts: 5`
- Para produção, use `retryDelaySeconds: 10` ou mais
- Erros 4xx (400, 401, 404) **não** disparam retry

---

## ⚡ Rate Limiting

### O que é?
Controla a quantidade máxima de requisições enviadas por segundo, evitando sobrecarga da API.

### Como Configurar

No arquivo `config.yaml`:

```yaml
api:
    maxRequestsPerSecond: 10   # Máximo de 10 requisições/segundo
```

**Omitir ou definir como `null`** = sem limite de taxa.

### Como Funciona

O sistema usa um **Token Bucket**:
- A cada segundo, libera N tokens (N = `maxRequestsPerSecond`)
- Cada requisição consome 1 token
- Se não houver tokens, aguarda até o próximo segundo

### Exemplos de Uso

#### API com Limite de 100 req/min
```yaml
api:
    maxRequestsPerSecond: 1.67  # ~100/min = 1.67/seg
```

#### API com Limite de 1000 req/hora
```yaml
api:
    maxRequestsPerSecond: 0.28  # ~1000/h = 0.28/seg
```

#### API sem Limite
```yaml
api:
    # Não definir maxRequestsPerSecond ou deixar como null
```

### Observações

- O rate limiting é aplicado **em paralelo** com o processamento em lote
- Se `batchLines: 100` e `maxRequestsPerSecond: 10`, cada lote leva ~10 segundos
- Ajuste conforme os limites da sua API de destino

---

## 🔍 Modo Dry Run

### O que é?
Executa todo o processamento mas **não envia** requisições reais para a API. Útil para testar e validar antes de executar.

### Como Usar

#### Via Linha de Comando
```bash
dotnet run -- --config config.yaml --dry-run
```

ou

```bash
dotnet run -- -c config.yaml --test
```

### O que é Exibido

```
🔍 MODO DRY RUN: Nenhuma requisição será enviada à API
🚀 Iniciando processamento do arquivo CSV...
[DRY RUN] Linha 2: {"name":"JOÃO SILVA","email":"joao@email.com"}
[DRY RUN] Linha 3: {"name":"MARIA SANTOS","email":"maria@email.com"}
Processadas 100 linhas. Sucessos: 100, Erros: 0
✅ Processamento concluído com sucesso!
```

### Casos de Uso

#### 1. Validar Transformações
```bash
# Ver como os dados serão transformados
dotnet run -- --dry-run
```

#### 2. Testar Mapeamento
```bash
# Verificar se o payload está correto
dotnet run -- --dry-run | grep "DRY RUN" | head -n 5
```

#### 3. Validar CSV Grande
```bash
# Testar arquivo com milhões de linhas sem enviar dados
dotnet run -- --input big_file.csv --dry-run
```

### Observações

- Validações de dados **são executadas** normalmente
- Logs de erro de validação **são gerados**
- Nenhuma requisição HTTP é feita
- Checkpoint **é salvo** normalmente (opcional)

---

## 💾 Checkpoint/Resume

### O que é?
Salva o progresso do processamento em um arquivo JSON. Se o processo for interrompido, pode retomar de onde parou.

### Como Configurar

No arquivo `config.yaml`:

```yaml
file:
    checkpointPath: "checkpoints/progress.json"
```

**Omitir `checkpointPath`** = checkpoint desabilitado.

### Como Funciona

1. **Durante o processamento**: salva checkpoint a cada 30 segundos
2. **Ao finalizar**: salva checkpoint final
3. **Na próxima execução**: detecta checkpoint e retoma

### Estrutura do Checkpoint

```json
{
  "LastProcessedLine": 5234,
  "LastUpdate": "2025-11-18T20:45:32",
  "TotalProcessed": 5000,
  "SuccessCount": 4850,
  "ErrorCount": 150
}
```

### Exemplo de Uso

#### Primeira Execução
```bash
dotnet run -- --config config.yaml
```

**Saída:**
```
🚀 Iniciando processamento do arquivo CSV...
Processadas 5000 linhas. Sucessos: 4850, Erros: 150
^C (Ctrl+C - interrompido)
💾 Checkpoint salvo em: checkpoints/progress.json
```

#### Retomar Processamento
```bash
dotnet run -- --config config.yaml
```

**Saída:**
```
📍 Checkpoint encontrado! Retomando da linha 5001
   Progresso anterior: 4850 sucessos, 150 erros
⏭️  Puladas 5000 linhas (iniciando na linha 5001)
🚀 Iniciando processamento do arquivo CSV...
Processadas 10000 linhas. Sucessos: 9700, Erros: 300
✅ Processamento concluído com sucesso!
💾 Checkpoint salvo em: checkpoints/progress.json
```

### Casos de Uso

#### 1. Arquivos Muito Grandes
```yaml
file:
    inputPath: "10_million_lines.csv"
    checkpointPath: "checkpoints/big_import.json"
```

Se o processo demorar horas, pode ser interrompido e retomado.

#### 2. Processamento Noturno
```bash
# Executar durante a noite
nohup dotnet run -- --config config.yaml &

# Se falhar, retomar no dia seguinte
dotnet run -- --config config.yaml
```

#### 3. Múltiplas Tentativas
```bash
# Processar até onde conseguir
dotnet run -- --config config.yaml

# Analisar erros no log
cat logs/process.log

# Retomar após corrigir problemas na API
dotnet run -- --config config.yaml
```

### Limpando Checkpoint

Para começar do zero, delete o arquivo:

```bash
rm checkpoints/progress.json
```

Ou crie um comando customizado:

```bash
# Processar do início ignorando checkpoint
rm -f checkpoints/progress.json && dotnet run -- --config config.yaml
```

---

## 🎯 Combinando Funcionalidades

### Cenário 1: Teste Completo
```bash
# Testar com dry run antes de executar
dotnet run -- --config config.yaml --dry-run

# Se OK, executar de verdade
dotnet run -- --config config.yaml
```

### Cenário 2: Importação Segura
```yaml
api:
    retryAttempts: 5
    retryDelaySeconds: 10
    maxRequestsPerSecond: 5

file:
    checkpointPath: "checkpoints/import.json"
```

**Benefícios:**
- Retry em caso de falha temporária
- Rate limiting para não sobrecarregar API
- Checkpoint para retomar se interrompido

### Cenário 3: Validação de Dados
```bash
# Dry run para ver transformações
dotnet run -- --dry-run --verbose

# Ver primeiras 10 linhas processadas
dotnet run -- --dry-run 2>&1 | grep "DRY RUN" | head -n 10
```

### Cenário 4: Processamento Incremental
```yaml
file:
    inputPath: "daily_export.csv"
    checkpointPath: "checkpoints/daily.json"
    
api:
    retryAttempts: 3
    maxRequestsPerSecond: 20
```

**Workflow diário:**
```bash
# Dia 1: processar arquivo
dotnet run -- --config config.yaml

# Dia 2: novo arquivo, começar do zero
rm checkpoints/daily.json
dotnet run -- --config config.yaml
```

---

## 📊 Exemplos Práticos

### E-commerce: Importação de 100k Produtos

```yaml
file:
    inputPath: "products.csv"
    batchLines: 500
    checkpointPath: "checkpoints/products.json"

api:
    endpointUrl: "https://api.loja.com/products"
    retryAttempts: 5
    retryDelaySeconds: 10
    maxRequestsPerSecond: 20
    mapping:
        - attribute: "sku"
          csvColumn: "Código"
          transform: "uppercase"
```

**Comandos:**
```bash
# Teste primeiro
dotnet run -- --dry-run | head -n 20

# Executar importação
dotnet run --

# Se falhar/interromper
dotnet run --  # Retoma automaticamente
```

### CRM: Atualização de Clientes

```yaml
file:
    inputPath: "customers_update.csv"
    batchLines: 100
    checkpointPath: "checkpoints/crm_update.json"

api:
    endpointUrl: "https://api.crm.com/customers"
    method: "PUT"
    retryAttempts: 3
    retryDelaySeconds: 5
    maxRequestsPerSecond: 10
```

### Integrações: Sync Diário

```bash
#!/bin/bash
# sync_daily.sh

# Limpar checkpoint anterior
rm -f checkpoints/sync.json

# Executar sync
dotnet run -- --config sync.yaml

# Se sucesso, notificar
if [ $? -eq 0 ]; then
    echo "Sync concluído com sucesso!"
fi
```

---

## ⚠️ Avisos Importantes

1. **Checkpoint não substitui backup**: O arquivo pode corromper
2. **Dry Run não testa autenticação**: Não valida se o token está correto
3. **Rate Limiting é aproximado**: Pode variar levemente
4. **Retry consome tempo**: Configure limites realistas
5. **Checkpoint salvo a cada 30s**: Últimas linhas podem ser reprocessadas

---

## 🐛 Troubleshooting

### Checkpoint não está funcionando
- Verifique permissões da pasta `checkpoints/`
- Certifique-se que `checkpointPath` está no `config.yaml`
- Delete arquivo corrompido e tente novamente

### Rate Limiting muito lento
- Aumente `maxRequestsPerSecond`
- Ou remova completamente se a API não tiver limite

### Retry não está funcionando
- Verifique se o erro é 5xx (retry não funciona para 4xx)
- Aumente `retryAttempts` se necessário
- Logs mostrarão as tentativas

### Dry Run não mostra output
- Use `--verbose` para ver mais detalhes
- Verifique se há erros de validação no log

---

**Última atualização**: 18 de Novembro de 2025
