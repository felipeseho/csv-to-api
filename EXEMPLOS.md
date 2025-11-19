# Exemplo de Uso com API Real

## Exemplo 1: Webhook.site (para testes)

Para testar a aplicação com uma API real sem precisar criar um servidor, você pode usar o [webhook.site](https://webhook.site):

1. Acesse https://webhook.site
2. Copie a URL única gerada (ex: `https://webhook.site/12345678-abcd-...`)
3. Atualize o `config.yaml`:

```yaml
api:
    endpointUrl: "https://webhook.site/sua-url-unica-aqui"
    authToken: ""
    method: "POST"
    requestTimeout: 30
```

4. Execute o programa:
```bash
dotnet run
```

5. Verifique as requisições recebidas no webhook.site

## Exemplo 2: API REST Real

### Configuração para API de Cadastro de Usuários

```yaml
file:
    inputPath: "data/usuarios.csv"
    batchLines: 50
    logPath: "logs/usuarios.log"
    csvDelimiter: ","
    mapping:
        - column: "Nome Completo"
          type: "string"
        - column: "E-mail"
          type: "string"
          regex: "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"
        - column: "CPF"
          type: "string"
          regex: "^\\d{3}\\.\\d{3}\\.\\d{3}-\\d{2}$"
        - column: "Data Nascimento"
          type: "date"
          format: "DD/MM/YYYY"
        - column: "CEP"
          type: "string"
          regex: "^\\d{5}-\\d{3}$"

api:
    endpointUrl: "https://api.exemplo.com.br/api/v1/usuarios"
    authToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    method: "POST"
    requestTimeout: 45
    mapping:
      - attribute: "nome"
        csvColumn: "Nome Completo"
      - attribute: "email"
        csvColumn: "E-mail"
      - attribute: "cpf"
        csvColumn: "CPF"
      - attribute: "dataNascimento"
        csvColumn: "Data Nascimento"
      - attribute: "endereco.cep"
        csvColumn: "CEP"
      - attribute: "endereco.rua"
        csvColumn: "Rua"
      - attribute: "endereco.numero"
        csvColumn: "Numero"
      - attribute: "endereco.cidade"
        csvColumn: "Cidade"
      - attribute: "endereco.estado"
        csvColumn: "Estado"
```

### Arquivo CSV de Exemplo (usuarios.csv)

```csv
Nome Completo,E-mail,CPF,Data Nascimento,CEP,Rua,Numero,Cidade,Estado
João da Silva,joao.silva@email.com,123.456.789-00,15/05/1990,12345-678,Rua das Flores,100,São Paulo,SP
Maria Santos,maria.santos@email.com,987.654.321-00,22/08/1985,98765-432,Av. Principal,250,Rio de Janeiro,RJ
```

### Payload Gerado

```json
{
  "nome": "João da Silva",
  "email": "joao.silva@email.com",
  "cpf": "123.456.789-00",
  "dataNascimento": "15/05/1990",
  "endereco": {
    "cep": "12345-678",
    "rua": "Rua das Flores",
    "numero": "100",
    "cidade": "São Paulo",
    "estado": "SP"
  }
}
```

## Exemplo 3: Atualização em Massa (PUT)

```yaml
api:
    endpointUrl: "https://api.exemplo.com/usuarios/{id}/atualizar"
    authToken: "seu-token-aqui"
    method: "PUT"
    requestTimeout: 30
    mapping:
      - attribute: "id"
        csvColumn: "ID"
      - attribute: "status"
        csvColumn: "Status"
      - attribute: "ultimaAtualizacao"
        csvColumn: "Data Atualizacao"
```

## Dicas de Performance

### Para arquivos grandes (1M+ linhas)

```yaml
file:
    batchLines: 500        # Lotes maiores
    
api:
    requestTimeout: 60     # Timeout maior
```

### Para APIs lentas

```yaml
file:
    batchLines: 10         # Lotes menores para evitar timeout
    
api:
    requestTimeout: 120    # Timeout maior
```

### Para máxima velocidade

```yaml
file:
    batchLines: 1000       # Lotes grandes
    
api:
    requestTimeout: 30     # API rápida
```

## Monitoramento de Progresso

A aplicação mostra o progresso em tempo real:

```
Processadas 100 linhas. Erros: 5
Processadas 200 linhas. Erros: 12
Processadas 300 linhas. Erros: 15
...
Total de linhas processadas: 10000
Total de erros: 234
Processamento concluído!
```

## Análise de Logs

### Verificar total de erros
```bash
wc -l logs/process.log
```

### Ver apenas erros de validação (HTTP 400)
```bash
grep ",400," logs/process.log
```

### Ver apenas erros de API (HTTP 500+)
```bash
grep -E ",(500|502|503|504)," logs/process.log
```

### Extrair emails com erro
```bash
awk -F',' '{print $3}' logs/process.log | tail -n +2
```

## Integração com Scripts

### Processar múltiplos arquivos

```bash
#!/bin/bash
for config in configs/*.yaml; do
    echo "Processando: $config"
    dotnet run -- "$config"
done
```

### Agendar com cron

```bash
# Executar todos os dias às 2h da manhã
0 2 * * * cd /path/to/CsvToApi && dotnet run -- config-diario.yaml
```

## Troubleshooting

### Erro: "Arquivo CSV não encontrado"
- Verifique o caminho em `file.inputPath`
- Use caminhos relativos ou absolutos

### Erro: "Connection timeout"
- Aumente `api.requestTimeout`
- Reduza `file.batchLines`
- Verifique conectividade com a API

### Erro: "401 Unauthorized"
- Verifique o `api.authToken`
- Certifique-se que o token não expirou

### Muitos erros de validação
- Revise as expressões regex em `file.mapping`
- Verifique o formato dos dados no CSV
- Ajuste o formato de data se necessário

## Exemplos com Argumentos de Linha de Comando

### Processar arquivo diferente sem alterar config.yaml
```bash
dotnet run -- --input data/vendas-janeiro.csv
```

### Usar endpoint de produção temporariamente
```bash
dotnet run -- --endpoint https://api.producao.com/vendas --verbose
```

### Processar com lotes maiores e timeout customizado
```bash
dotnet run -- --batch-lines 1000 --timeout 120 --verbose
```

### Processar arquivo com delimitador ponto-e-vírgula
```bash
dotnet run -- --input data/export.csv --delimiter ";" --verbose
```

### Usar configuração de teste com autenticação específica
```bash
dotnet run -- \
  --config config-test.yaml \
  --input data/test-data.csv \
  --endpoint https://test-api.com/upload \
  --auth-token "Bearer test-token-123" \
  --batch-lines 10 \
  --verbose
```

### Processar arquivo CSV para ambiente de desenvolvimento
```bash
dotnet run -- \
  -i data/clientes.csv \
  -e http://localhost:3000/api/clientes \
  -b 50 \
  -v
```

### Teste rápido com webhook.site
```bash
dotnet run -- \
  --input data/sample.csv \
  --endpoint "https://webhook.site/sua-url-aqui" \
  --batch-lines 5 \
  --verbose
```

### Retomar processamento após falha
```bash
# Se o processamento falhou ou foi interrompido na linha 2500
dotnet run -- \
  --input data/vendas-grandes.csv \
  --start-line 2501 \
  --endpoint https://api.producao.com/vendas \
  --auth-token "Bearer token-prod" \
  --verbose
```

### Processar apenas um subconjunto de linhas para teste
```bash
# Processar apenas linhas 100 a 200 (aproximadamente)
dotnet run -- \
  --input data/clientes.csv \
  --start-line 100 \
  --batch-lines 10 \
  --endpoint https://webhook.site/test \
  --verbose
# Interrompa após processar linhas suficientes (Ctrl+C)
```

