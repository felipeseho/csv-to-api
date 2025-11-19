# Guia Rápido de Argumentos de Linha de Comando

## Ver Todas as Opções
```bash
dotnet run -- --help
```

## Opções Principais

### Arquivo de Configuração
```bash
# Especificar arquivo de configuração
dotnet run -- --config config-prod.yaml
dotnet run -- -c config-test.yaml
```

### Sobrescrever Arquivo CSV
```bash
# Processar arquivo diferente
dotnet run -- --input data/vendas.csv
dotnet run -- -i data/clientes.csv
```

### Ajustar Processamento em Lote
```bash
# Processar 500 linhas por vez
dotnet run -- --batch-lines 500
dotnet run -- -b 1000
```

### Linha Inicial
```bash
# Começar processamento a partir da linha 100
dotnet run -- --start-line 100
dotnet run -- -s 500

# Útil para retomar processamento após falha
dotnet run -- -i data/vendas.csv -s 1001 -v
```

### Limitar Quantidade de Linhas
```bash
# Processar apenas as primeiras 1000 linhas
dotnet run -- --max-lines 1000
dotnet run -- -n 500

# Útil para testes ou processamento parcial
dotnet run -- -i data/vendas.csv -n 100 -v

# Combinar com linha inicial para processar um intervalo específico
# Exemplo: processar linhas 101 a 200
dotnet run -- -s 101 -n 100 -v
```

### Configurar Endpoint da API
```bash
# Apontar para API diferente
dotnet run -- --endpoint https://api.producao.com/upload
dotnet run -- -e http://localhost:3000/api/data
```

### Autenticação
```bash
# Fornecer token de autenticação
dotnet run -- --auth-token "Bearer abc123xyz"
dotnet run -- -a "Bearer token-producao"
```

### Método HTTP
```bash
# Usar PUT em vez de POST
dotnet run -- --method PUT
dotnet run -- -m POST
```

### Timeout
```bash
# Aumentar timeout para 120 segundos
dotnet run -- --timeout 120
dotnet run -- -t 60
```

### Logs Detalhados
```bash
# Ativar modo verboso
dotnet run -- --verbose
dotnet run -- -v
```

### Delimitador CSV
```bash
# Usar ponto-e-vírgula como delimitador
dotnet run -- --delimiter ";"
dotnet run -- -d "|"
```

### Arquivo de Log
```bash
# Especificar arquivo de log diferente
dotnet run -- --log-path logs/erros-producao.log
dotnet run -- -l logs/debug.log
```

## Exemplos Combinados

### Teste em Desenvolvimento
```bash
dotnet run -- \
  -i data/test.csv \
  -e http://localhost:3000/api/users \
  -b 10 \
  -n 50 \
  -v
```

### Produção com Todas as Configurações
```bash
dotnet run -- \
  --config config-prod.yaml \
  --input data/vendas-diarias.csv \
  --endpoint https://api.empresa.com/vendas \
  --auth-token "Bearer prod-token-xyz123" \
  --batch-lines 1000 \
  --timeout 90 \
  --method POST \
  --verbose
```

### Teste Rápido com Webhook
```bash
dotnet run -- \
  -i data/sample.csv \
  -e "https://webhook.site/sua-url-unica" \
  -b 5 \
  -v
```

### Processar Arquivo com Configuração Específica
```bash
dotnet run -- \
  --config config.yaml \
  --input data/novos-usuarios.csv \
  --batch-lines 100 \
  --log-path logs/usuarios-$(date +%Y%m%d).log \
  --verbose
```

### Retomar Processamento Após Falha
```bash
# Se o processamento falhou na linha 500, retome a partir dela
dotnet run -- \
  --input data/vendas-grandes.csv \
  --start-line 501 \
  --batch-lines 100 \
  --verbose
```

## Prioridade das Configurações

1. **Argumentos de linha de comando** (maior prioridade)
2. Arquivo YAML especificado em `--config`
3. `config.yaml` padrão (se nenhum argumento for fornecido)

## Dicas

- Use `-v` ou `--verbose` para debug e acompanhamento do processo
- Combine argumentos para testes rápidos sem modificar arquivos YAML
- Argumentos são especialmente úteis em scripts e CI/CD
- Sempre teste com `--batch-lines` pequeno primeiro (ex: 10) antes de processar arquivos grandes
