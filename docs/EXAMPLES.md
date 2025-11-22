<div align="center">
  <h1>💡 Exemplos Práticos</h1>
  <p><strong>Guia completo com exemplos reais de uso do n2n</strong></p>
</div>

---

## 📋 Índice

- [Exemplo 1: Webhook.site (Testes)](#-exemplo-1-webhooksite-para-testes)
- [Exemplo 2: API REST Real](#-exemplo-2-api-rest-real)
- [Exemplo 3: Atualização em Massa (PUT)](#-exemplo-3-atualização-em-massa-put)
- [Exemplo 4: Múltiplos Endpoints](#-exemplo-4-múltiplos-endpoints-com-roteamento)
- [Exemplo 5: Valores Fixos](#-exemplo-5-usando-valores-fixos)
- [Exemplo 6: Transformações Avançadas](#-exemplo-6-transformações-avançadas)
- [Exemplo 7: Filtros de Dados](#-exemplo-7-filtros-de-dados)

---

## 🧪 Exemplo 1: Webhook.site (para Testes)

**Objetivo:** Testar a aplicação sem configurar um servidor real.

### Passo a Passo

1. **Acesse** [https://webhook.site](https://webhook.site)
2. **Copie** a URL única gerada (ex: `https://webhook.site/12345678-abcd...`)
3. **Configure** o `config.yaml`:

```yaml
endpoints:
  - name: "teste"
    endpointUrl: "https://webhook.site/sua-url-unica-aqui"
    headers:
      Authorization: "Bearer token-opcional"
    method: "POST"
    requestTimeout: 30
    mapping:
      - attribute: "name"
        csvColumn: "Name"
      - attribute: "email"
        csvColumn: "Email"
```

4. **Execute**:

```bash
dotnet run --verbose
```

5. **Verifique** as requisições chegando em tempo real no webhook.site

### CSV de Exemplo

```csv
Name,Email
João Silva,joao@example.com
Maria Santos,maria@example.com
```

### Payload Enviado

```json
{
  "name": "João Silva",
  "email": "joao@example.com"
}
```

---

## 🏢 Exemplo 2: API REST Real

**Objetivo:** Cadastrar usuários em uma API de produção.

### Configuração Completa

```yaml
file:
  inputPath: "data/usuarios.csv"
  batchLines: 50
  logDirectory: "logs"
  csvDelimiter: ","
  checkpointDirectory: "checkpoints"
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

endpoints:
  - name: "usuarios"
    endpointUrl: "https://api.exemplo.com.br/api/v1/usuarios"
    headers:
      Authorization: "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
      X-API-Version: "1.0"
    method: "POST"
    requestTimeout: 45
    retryAttempts: 3
    retryDelaySeconds: 5
    mapping:
      - attribute: "nome"
        csvColumn: "Nome Completo"
        transform: "title-case"
      - attribute: "email"
        csvColumn: "E-mail"
        transform: "lowercase"
      - attribute: "cpf"
        csvColumn: "CPF"
        transform: "remove-non-numeric"
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

### Arquivo CSV (usuarios.csv)

```csv
Nome Completo,E-mail,CPF,Data Nascimento,CEP,Rua,Numero,Cidade,Estado
joão da silva,JOAO.SILVA@EMAIL.COM,123.456.789-00,15/05/1990,12345-678,Rua das Flores,100,São Paulo,SP
MARIA SANTOS,Maria.Santos@Email.Com,987.654.321-00,22/08/1985,98765-432,Av. Principal,250,Rio de Janeiro,RJ
```

### Payload Gerado

```json
{
  "nome": "João Da Silva",
  "email": "joao.silva@email.com",
  "cpf": "12345678900",
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

---

## 🔄 Exemplo 3: Atualização em Massa (PUT)

**Objetivo:** Atualizar status de múltiplos registros existentes.

### Configuração

```yaml
file:
  inputPath: "data/atualizacoes.csv"
  batchLines: 100
  mapping:
    - column: "ID"
      type: "string"
    - column: "Status"
      type: "string"
    - column: "Data"
      type: "date"
      format: "YYYY-MM-DD"

endpoints:
  - name: "atualizacao"
    endpointUrl: "https://api.exemplo.com/usuarios/{id}/status"
    headers:
      Authorization: "Bearer seu-token-aqui"
      X-Update-Source: "csv-batch"
    method: "PUT"
    requestTimeout: 30
    mapping:
      - attribute: "id"
        csvColumn: "ID"
      - attribute: "status"
        csvColumn: "Status"
        transform: "lowercase"
      - attribute: "updatedAt"
        csvColumn: "Data"
      - attribute: "updatedBy"
        fixedValue: "sistema-batch"
```

### CSV de Exemplo

```csv
ID,Status,Data
user-123,ATIVO,2024-01-15
user-456,SUSPENSO,2024-01-16
user-789,ATIVO,2024-01-17
```

### Requisição Gerada

```http
PUT /usuarios/user-123/status HTTP/1.1
Host: api.exemplo.com
Authorization: Bearer seu-token-aqui
Content-Type: application/json

{
  "id": "user-123",
  "status": "ativo",
  "updatedAt": "2024-01-15",
  "updatedBy": "sistema-batch"
}
```

---

## 🌐 Exemplo 4: Múltiplos Endpoints com Roteamento

**Objetivo:** Enviar dados para diferentes APIs baseado em uma coluna do CSV.

### Arquivo CSV com Roteamento

```csv
Nome,Email,Sistema,Plano
João Silva,joao@empresa.com,CRM,premium
Maria Santos,maria@empresa.com,ERP,basic
Pedro Costa,pedro@empresa.com,CRM,premium
Ana Lima,ana@empresa.com,BILLING,enterprise
```

### Configuração

```yaml
file:
  inputPath: "data/multi-sistema.csv"
  batchLines: 50

# Define qual coluna do CSV indica o endpoint
endpointColumnName: "Sistema"

# Endpoint padrão se nenhum for especificado
defaultEndpoint: "CRM"

endpoints:
  # Endpoint para CRM
  - name: "CRM"
    endpointUrl: "https://api.crm.com/contacts"
    headers:
      Authorization: "Bearer token-crm"
    method: "POST"
    mapping:
      - attribute: "fullName"
        csvColumn: "Nome"
        transform: "title-case"
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"
      - attribute: "tier"
        csvColumn: "Plano"
  
  # Endpoint para ERP
  - name: "ERP"
    endpointUrl: "https://api.erp.com/customers"
    headers:
      Authorization: "Bearer token-erp"
      X-Source: "n2n-integration"
    method: "POST"
    mapping:
      - attribute: "customerName"
        csvColumn: "Nome"
        transform: "uppercase"
      - attribute: "contactEmail"
        csvColumn: "Email"
      - attribute: "subscription"
        csvColumn: "Plano"
  
  # Endpoint para Billing
  - name: "BILLING"
    endpointUrl: "https://api.billing.com/accounts"
    headers:
      Authorization: "Bearer token-billing"
    method: "POST"
    mapping:
      - attribute: "accountName"
        csvColumn: "Nome"
      - attribute: "billingEmail"
        csvColumn: "Email"
      - attribute: "planType"
        csvColumn: "Plano"
```

### Resultado

- **João Silva** → Enviado para API do CRM
- **Maria Santos** → Enviado para API do ERP
- **Pedro Costa** → Enviado para API do CRM
- **Ana Lima** → Enviado para API do BILLING

---

## 📌 Exemplo 5: Usando Valores Fixos

**Objetivo:** Combinar dados do CSV com metadados fixos.

### Configuração

```yaml
endpoints:
  - name: "produtos"
    endpointUrl: "https://api.loja.com/produtos"
    headers:
      Authorization: "Bearer token-123"
    method: "POST"
    mapping:
      # Dados do CSV
      - attribute: "nome"
        csvColumn: "Nome Produto"
        transform: "title-case"
      
      - attribute: "preco"
        csvColumn: "Preco"
      
      - attribute: "estoque"
        csvColumn: "Quantidade"
      
      # Valores fixos (mesmos para todas as linhas)
      - attribute: "origem"
        fixedValue: "importacao-csv"
      
      - attribute: "versao"
        fixedValue: "1.0"
      
      - attribute: "importadoPor"
        fixedValue: "sistema-batch"
      
      - attribute: "categoria"
        fixedValue: "geral"
      
      - attribute: "ativo"
        fixedValue: true
```

### CSV de Exemplo

```csv
Nome Produto,Preco,Quantidade
camiseta branca,29.90,100
calça jeans,89.90,50
```

### Payload Gerado

```json
{
  "nome": "Camiseta Branca",
  "preco": "29.90",
  "estoque": "100",
  "origem": "importacao-csv",
  "versao": "1.0",
  "importadoPor": "sistema-batch",
  "categoria": "geral",
  "ativo": true
}
```

---

## 🎨 Exemplo 6: Transformações Avançadas

**Objetivo:** Aplicar múltiplas transformações nos dados.

### Configuração

```yaml
endpoints:
  - name: "clientes"
    endpointUrl: "https://api.exemplo.com/clientes"
    method: "POST"
    mapping:
      # Nome em Title Case
      - attribute: "nome"
        csvColumn: "Nome"
        transform: "title-case"
      
      # Email em minúsculas
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"
      
      # CPF formatado
      - attribute: "cpf"
        csvColumn: "CPF"
        transform: "format-cpf"
      
      # Telefone formatado
      - attribute: "telefone"
        csvColumn: "Telefone"
        transform: "format-phone-br"
      
      # CEP formatado
      - attribute: "endereco.cep"
        csvColumn: "CEP"
        transform: "format-cep"
      
      # Slug para URL
      - attribute: "slug"
        csvColumn: "Nome"
        transform: "slugify"
```

### CSV de Exemplo

```csv
Nome,Email,CPF,Telefone,CEP
JOÃO JOSÉ DA SILVA,JOAO@EMAIL.COM,12345678900,11987654321,01310100
```

### Payload com Transformações

```json
{
  "nome": "João José Da Silva",
  "email": "joao@email.com",
  "cpf": "123.456.789-00",
  "telefone": "(11) 98765-4321",
  "endereco": {
    "cep": "01310-100"
  },
  "slug": "joao-jose-da-silva"
}
```

---

## 🔍 Exemplo 7: Filtros de Dados

**Objetivo:** Processar apenas linhas que atendem critérios específicos.

### Configuração

```yaml
file:
  inputPath: "data/campanhas.csv"
  mapping:
    # Filtro 1: Apenas campanha "promo2024"
    - column: "Campanha"
      type: "string"
      filter:
        operator: "Equals"
        value: "promo2024"
        caseInsensitive: true
    
    # Filtro 2: Excluir status "cancelado"
    - column: "Status"
      type: "string"
      filter:
        operator: "NotEquals"
        value: "cancelado"
        caseInsensitive: true
    
    # Filtro 3: Apenas planos premium
    - column: "Plano"
      type: "string"
      filter:
        operator: "Contains"
        value: "premium"
        caseInsensitive: true

endpoints:
  - name: "marketing"
    endpointUrl: "https://api.marketing.com/contacts"
    mapping:
      - attribute: "name"
        csvColumn: "Nome"
      - attribute: "email"
        csvColumn: "Email"
      - attribute: "campaign"
        csvColumn: "Campanha"
```

### CSV de Exemplo

```csv
Nome,Email,Campanha,Status,Plano
João Silva,joao@email.com,promo2024,ativo,premium
Maria Santos,maria@email.com,promo2024,ativo,basic
Pedro Costa,pedro@email.com,natal2024,ativo,premium
Ana Lima,ana@email.com,promo2024,cancelado,premium
Carlos Souza,carlos@email.com,promo2024,ativo,premium
```

### Linhas Processadas

Apenas 2 linhas serão processadas (João Silva e Carlos Souza):

- ✅ **João Silva**: campanha=promo2024, status=ativo, plano=premium
- ❌ **Maria Santos**: plano=basic (falha no filtro)
- ❌ **Pedro Costa**: campanha=natal2024 (falha no filtro)
- ❌ **Ana Lima**: status=cancelado (falha no filtro)
- ✅ **Carlos Souza**: campanha=promo2024, status=ativo, plano=premium

---

## 📊 Dicas de Performance

### Para Arquivos Grandes (1M+ linhas)

```yaml
file:
  batchLines: 500        # Lotes maiores
  logDirectory: "logs"   # Separar logs
```

```bash
dotnet run -- --batch-lines 1000 --verbose
```

### Para APIs Lentas

```yaml
file:
  batchLines: 10         # Lotes menores
  
endpoints:
  - requestTimeout: 60   # Timeout maior
    retryAttempts: 5     # Mais tentativas
```

### Para Máxima Velocidade

```yaml
file:
  batchLines: 1000       # Lotes grandes
  
endpoints:
  - maxRequestsPerSecond: 50  # Mais requisições simultâneas
```

---

## 📝 Análise de Logs

### Ver Total de Erros

```bash
wc -l logs/process_*.log
```

### Filtrar Erros de Validação (HTTP 400)

```bash
grep ",400," logs/process_*.log
```

### Filtrar Erros de Servidor (HTTP 500+)

```bash
grep -E ",(500|502|503|504)," logs/process_*.log
```

### Extrair Emails com Erro

```bash
awk -F',' '{print $3}' logs/process_*.log | tail -n +2
```

---

## 🔧 Integração com Scripts

### Processar Múltiplos Arquivos

```bash
#!/bin/bash

# Processar todos os CSVs em um diretório
for config in configs/*.yaml; do
  echo "Processando: $config"
  dotnet run -- --config "$config" --verbose
  
  # Aguardar entre processamentos
  sleep 5
done
```

### Agendar com Cron

```bash
# Executar todos os dias às 2h da manhã
0 2 * * * cd /path/to/n2n && dotnet run -- --config config-daily.yaml --verbose >> /var/log/n2n.log 2>&1
```

### Processar com Notificação

```bash
#!/bin/bash

echo "Iniciando processamento..."
dotnet run -- --config config.yaml --verbose

if [ $? -eq 0 ]; then
  echo "✅ Processamento concluído com sucesso!"
  # Enviar email de sucesso
else
  echo "❌ Erro no processamento!"
  # Enviar email de erro
fi
```

---

## 🐛 Troubleshooting Comum

### Erro: "Arquivo CSV não encontrado"

**Solução:**

```bash
# Verificar caminho
ls -la data/input.csv

# Usar caminho absoluto
inputPath: "/caminho/completo/para/arquivo.csv"
```

### Erro: "Connection timeout"

**Solução:**

```yaml
endpoints:
  - requestTimeout: 60        # Aumentar timeout
    retryAttempts: 5          # Mais tentativas
    retryDelaySeconds: 10     # Maior delay entre retries
```

### Erro: "401 Unauthorized"

**Solução:**

- Verificar token de autenticação
- Confirmar que token não expirou
- Validar header `Authorization`

### Muitos Erros de Validação

**Solução:**

```bash
# Testar com dry-run primeiro
dotnet run -- --dry-run --max-lines 10 --verbose

# Verificar regex das validações
# Ajustar formato de data se necessário
```

---

## 📚 Documentação Relacionada

- 📖 [README Principal](../README.md)
- 🚀 [Quick Start](QUICKSTART.md)
- ⚙️ [Argumentos CLI](CLI-ARGUMENTS.md)
- 🎨 [Transformações](TRANSFORMATIONS.md)
- 🔍 [Filtros](FILTERS.md)

---

<div align="center">
  <p><strong>💡 Precisa de mais exemplos? Abra uma issue no GitHub!</strong></p>
  <p>
    <a href="#-índice">Voltar ao topo ⬆️</a>
  </p>
</div>
