#!/bin/bash

# Scripts de exemplo para execução da aplicação CSV to API

# =============================================================================
# DESENVOLVIMENTO
# =============================================================================

# Teste rápido com logs detalhados
alias csv-test="dotnet run -- --batch-lines 10 --verbose"

# Processar arquivo de teste
alias csv-dev="dotnet run -- \
  --input data/test.csv \
  --endpoint http://localhost:3000/api/data \
  --batch-lines 50 \
  --verbose"

# =============================================================================
# TESTES COM WEBHOOK.SITE
# =============================================================================

# Teste com webhook.site (substitua pela sua URL)
csv-webhook() {
    local WEBHOOK_URL="${1:-https://webhook.site/sua-url-aqui}"
    dotnet run -- \
        --endpoint "$WEBHOOK_URL" \
        --batch-lines 5 \
        --verbose
}

# =============================================================================
# PRODUÇÃO
# =============================================================================

# Processar vendas em produção
csv-vendas-prod() {
    dotnet run -- \
        --config config-prod.yaml \
        --input data/vendas-$(date +%Y%m%d).csv \
        --endpoint https://api.producao.com/vendas \
        --auth-token "${API_TOKEN}" \
        --batch-lines 1000 \
        --timeout 120 \
        --log-path logs/vendas-$(date +%Y%m%d-%H%M%S).log \
        --verbose
}

# Processar clientes em produção
csv-clientes-prod() {
    dotnet run -- \
        --config config-prod.yaml \
        --input data/clientes.csv \
        --endpoint https://api.producao.com/clientes \
        --auth-token "${API_TOKEN}" \
        --batch-lines 500 \
        --timeout 90 \
        --verbose
}

# =============================================================================
# STAGING
# =============================================================================

# Processar em ambiente de staging
csv-staging() {
    local INPUT_FILE="${1:-data/input.csv}"
    dotnet run -- \
        --config config-staging.yaml \
        --input "$INPUT_FILE" \
        --endpoint https://staging-api.producao.com/upload \
        --auth-token "${STAGING_TOKEN}" \
        --batch-lines 200 \
        --verbose
}

# =============================================================================
# UTILITÁRIOS
# =============================================================================

# Compilar e executar
csv-build-run() {
    dotnet build --configuration Release && \
    ./bin/Release/net10.0/CsvToApi "$@"
}

# Executar executável compilado
csv-exe() {
    ./bin/Release/net10.0/CsvToApi "$@"
}

# Processar com configuração customizada
csv-custom() {
    local CONFIG="${1:-config.yaml}"
    local INPUT="${2:-data/input.csv}"
    local ENDPOINT="${3:-}"
    
    if [ -z "$ENDPOINT" ]; then
        dotnet run -- --config "$CONFIG" --input "$INPUT" --verbose
    else
        dotnet run -- \
            --config "$CONFIG" \
            --input "$INPUT" \
            --endpoint "$ENDPOINT" \
            --verbose
    fi
}

# Processar com delimitador customizado
csv-delim() {
    local INPUT="${1:-data/input.csv}"
    local DELIMITER="${2:-;}"
    
    dotnet run -- \
        --input "$INPUT" \
        --delimiter "$DELIMITER" \
        --verbose
}

# Retomar processamento a partir de uma linha específica
csv-resume() {
    local INPUT="${1:-data/input.csv}"
    local START_LINE="${2:-1}"
    
    dotnet run -- \
        --input "$INPUT" \
        --start-line "$START_LINE" \
        --verbose
}

# Processar intervalo de linhas
csv-range() {
    local INPUT="${1:-data/input.csv}"
    local START_LINE="${2:-1}"
    local BATCH_SIZE="${3:-100}"
    
    dotnet run -- \
        --input "$INPUT" \
        --start-line "$START_LINE" \
        --batch-lines "$BATCH_SIZE" \
        --verbose
}

# =============================================================================
# MONITORAMENTO
# =============================================================================

# Processar e monitorar logs em tempo real
csv-monitor() {
    local LOG_FILE="logs/process-$(date +%Y%m%d-%H%M%S).log"
    
    dotnet run -- \
        --log-path "$LOG_FILE" \
        --verbose &
    
    local PID=$!
    
    # Aguardar arquivo de log ser criado
    sleep 2
    
    # Monitorar log
    tail -f "$LOG_FILE" &
    local TAIL_PID=$!
    
    # Aguardar processo principal
    wait $PID
    
    # Parar monitoramento
    kill $TAIL_PID 2>/dev/null
}

# =============================================================================
# EXEMPLOS DE USO
# =============================================================================

# Para usar estes scripts:
# 1. Torne o arquivo executável:
#    chmod +x scripts.sh
#
# 2. Source o arquivo no seu terminal:
#    source scripts.sh
#
# 3. Use os comandos/funções:
#    csv-test
#    csv-webhook "https://webhook.site/sua-url"
#    csv-custom config-prod.yaml data/vendas.csv
#    csv-delim data/export.csv ";"
#    csv-resume data/vendas.csv 1001
#    csv-range data/vendas.csv 500 100
#
# 4. OU execute diretamente:
#    ./scripts.sh

# =============================================================================
# VARIÁVEIS DE AMBIENTE
# =============================================================================

# Defina estas variáveis no seu .bashrc ou .zshrc:
# export API_TOKEN="Bearer seu-token-producao"
# export STAGING_TOKEN="Bearer seu-token-staging"
# export CSV_CONFIG_PROD="/caminho/para/config-prod.yaml"
# export CSV_CONFIG_STAGING="/caminho/para/config-staging.yaml"

echo "Scripts CSV to API carregados!"
echo "Use 'csv-' + TAB para ver comandos disponíveis"
