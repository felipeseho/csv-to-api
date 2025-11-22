<div align="center">
  <h1>🎨 Transformações de Dados</h1>
  <p><strong>Guia completo de todas as transformações disponíveis</strong></p>
</div>

---

## 📋 Visão Geral

As **transformações de dados** permitem modificar valores das colunas CSV antes de enviá-los para a API. São mais de **20 transformações** disponíveis, desde conversão de texto até formatações específicas brasileiras.

### 💡 Por que usar transformações?

- ✅ **Normalizar dados** de diferentes fontes
- ✅ **Formatar documentos** (CPF, CNPJ, telefone)
- ✅ **Limpar dados** removendo caracteres especiais
- ✅ **Padronizar textos** (maiúsculas, minúsculas, title case)
- ✅ **Criar slugs** para URLs amigáveis

---

## 🎯 Como Usar

Adicione a propriedade `transform` no mapeamento do endpoint:

```yaml
endpoints:
  - name: "api-principal"
    endpointUrl: "https://api.exemplo.com/upload"
    method: "POST"
    mapping:
      - attribute: "name"
        csvColumn: "Name"
        transform: "uppercase"      # ← Transformação aplicada
      
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"      # ← Transformação aplicada
```

---

## 📚 Transformações Disponíveis

### 🔤 Transformações de Texto

#### `uppercase` - Converter para MAIÚSCULAS

```yaml
transform: "uppercase"
```

**Exemplos:**
- `"João Silva"` → `"JOÃO SILVA"`
- `"maria santos"` → `"MARIA SANTOS"`
- `"Pedro123"` → `"PEDRO123"`

---

#### `lowercase` - Converter para minúsculas

```yaml
transform: "lowercase"
```

**Exemplos:**
- `"JOÃO SILVA"` → `"joão silva"`
- `"Maria Santos"` → `"maria santos"`
- `"PEDRO123"` → `"pedro123"`

---

#### `capitalize` - Primeira letra maiúscula

```yaml
transform: "capitalize"
```

**Exemplos:**
- `"joão SILVA"` → `"João silva"`
- `"MARIA"` → `"Maria"`
- `"pedro costa"` → `"Pedro costa"`

---

#### `title-case` - Primeira Letra De Cada Palavra

```yaml
transform: "title-case"
```

**Exemplos:**
- `"joão silva"` → `"João Silva"`
- `"MARIA SANTOS"` → `"Maria Santos"`
- `"pedro josé da costa"` → `"Pedro José Da Costa"`

---

### 🧹 Limpeza de Dados

#### `trim` - Remover espaços nas extremidades

```yaml
transform: "trim"
```

**Exemplos:**
- `"  João  "` → `"João"`
- `"\tMaria\n"` → `"Maria"`
- `"  Pedro  Silva  "` → `"Pedro  Silva"`

---

#### `remove-spaces` - Remover todos os espaços

```yaml
transform: "remove-spaces"
```

**Exemplos:**
- `"João Silva"` → `"JoãoSilva"`
- `"Maria  Santos"` → `"MariaSantos"`
- `"123 456 789"` → `"123456789"`

---

#### `remove-accents` - Remover acentos

```yaml
transform: "remove-accents"
```

**Exemplos:**
- `"João José"` → `"Joao Jose"`
- `"María González"` → `"Maria Gonzalez"`
- `"François"` → `"Francois"`

---

#### `remove-non-numeric` - Manter apenas números

```yaml
transform: "remove-non-numeric"
```

**Exemplos:**
- `"123.456.789-00"` → `"12345678900"`
- `"(11) 98765-4321"` → `"11987654321"`
- `"R$ 1.234,56"` → `"123456"`

---

#### `remove-non-alphanumeric` - Remover caracteres especiais

```yaml
transform: "remove-non-alphanumeric"
```

**Exemplos:**
- `"João-Silva_123!"` → `"JoãoSilva123"`
- `"email@exemplo.com"` → `"emailexemplocom"`
- `"ABC-123/XYZ"` → `"ABC123XYZ"`

---

### 🇧🇷 Formatações Brasileiras

#### `format-cpf` - Formatar CPF (000.000.000-00)

```yaml
transform: "format-cpf"
```

**Exemplos:**
- `"12345678900"` → `"123.456.789-00"`
- `"98765432100"` → `"987.654.321-00"`
- `"123.456.789-00"` → `"123.456.789-00"` (já formatado)

---

#### `format-cnpj` - Formatar CNPJ (00.000.000/0000-00)

```yaml
transform: "format-cnpj"
```

**Exemplos:**
- `"12345678000190"` → `"12.345.678/0001-90"`
- `"98765432000100"` → `"98.765.432/0001-00"`

---

#### `format-phone-br` - Formatar telefone brasileiro

```yaml
transform: "format-phone-br"
```

**Exemplos:**
- `"11987654321"` → `"(11) 98765-4321"` (celular)
- `"1134567890"` → `"(11) 3456-7890"` (fixo)
- `"85912345678"` → `"(85) 91234-5678"`

---

#### `format-cep` - Formatar CEP (00000-000)

```yaml
transform: "format-cep"
```

**Exemplos:**
- `"01310100"` → `"01310-100"`
- `"12345678"` → `"12345-678"`
- `"01310-100"` → `"01310-100"` (já formatado)

---

### 🔧 Outras Transformações

#### `slugify` - Converter para slug (URL-friendly)

```yaml
transform: "slugify"
```

**Exemplos:**
- `"João José da Silva!"` → `"joao-jose-da-silva"`
- `"Produto Novo 2024"` → `"produto-novo-2024"`
- `"Meu Título Especial!"` → `"meu-titulo-especial"`

---

#### `reverse` - Inverter string

```yaml
transform: "reverse"
```

**Exemplos:**
- `"ABC123"` → `"321CBA"`
- `"João"` → `"oãoJ"`
- `"Hello World"` → `"dlroW olleH"`

---

#### `base64-encode` - Codificar em Base64

```yaml
transform: "base64-encode"
```

**Exemplos:**
- `"Hello"` → `"SGVsbG8="`
- `"João"` → `"Sm/Do28="`
- `"123456"` → `"MTIzNDU2"`

---

#### `url-encode` - Codificar para URL

```yaml
transform: "url-encode"
```

**Exemplos:**
- `"João Silva"` → `"Jo%C3%A3o%20Silva"`
- `"email@exemplo.com"` → `"email%40exemplo.com"`
- `"a b c"` → `"a%20b%20c"`

---

## 💡 Exemplos Práticos

### Exemplo 1: E-commerce - Normalização de Produtos

```yaml
endpoints:
  - name: "produtos"
    endpointUrl: "https://api.loja.com/produtos"
    method: "POST"
    mapping:
      - attribute: "titulo"
        csvColumn: "Nome Produto"
        transform: "title-case"
      
      - attribute: "sku"
        csvColumn: "Codigo"
        transform: "uppercase"
      
      - attribute: "slug"
        csvColumn: "Nome Produto"
        transform: "slugify"
      
      - attribute: "descricao"
        csvColumn: "Descricao"
        transform: "trim"
```

**CSV:**

```csv
Nome Produto,Codigo,Descricao
camiseta básica branca,abc123,  Camiseta 100% algodão  
CALÇA JEANS MASCULINA,xyz789,  Calça jeans azul  
```

**Payload enviado:**

```json
{
  "titulo": "Camiseta Básica Branca",
  "sku": "ABC123",
  "slug": "camiseta-basica-branca",
  "descricao": "Camiseta 100% algodão"
}
```

---

### Exemplo 2: CRM - Normalização de Clientes

```yaml
endpoints:
  - name: "clientes"
    endpointUrl: "https://api.crm.com/contacts"
    method: "POST"
    mapping:
      - attribute: "nome"
        csvColumn: "Nome"
        transform: "title-case"
      
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"
      
      - attribute: "cpf"
        csvColumn: "CPF"
        transform: "format-cpf"
      
      - attribute: "telefone"
        csvColumn: "Telefone"
        transform: "format-phone-br"
      
      - attribute: "cep"
        csvColumn: "CEP"
        transform: "format-cep"
```

**CSV:**

```csv
Nome,Email,CPF,Telefone,CEP
JOÃO SILVA,JOAO@EMAIL.COM,12345678900,11987654321,01310100
maria santos,Maria@Email.Com,98765432100,1134567890,12345678
```

**Payloads enviados:**

```json
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "cpf": "123.456.789-00",
  "telefone": "(11) 98765-4321",
  "cep": "01310-100"
}
```

```json
{
  "nome": "Maria Santos",
  "email": "maria@email.com",
  "cpf": "987.654.321-00",
  "telefone": "(11) 3456-7890",
  "cep": "12345-678"
}
```

---

### Exemplo 3: Limpeza de Dados Importados

```yaml
endpoints:
  - name: "api"
    endpointUrl: "https://api.exemplo.com/data"
    method: "POST"
    mapping:
      # Limpar CPF (remover formatação)
      - attribute: "cpf"
        csvColumn: "CPF"
        transform: "remove-non-numeric"
      
      # Limpar telefone (apenas números)
      - attribute: "telefone"
        csvColumn: "Telefone"
        transform: "remove-non-numeric"
      
      # Remover acentos do nome
      - attribute: "nomeNormalizado"
        csvColumn: "Nome"
        transform: "remove-accents"
      
      # Slug do nome
      - attribute: "slug"
        csvColumn: "Nome"
        transform: "slugify"
```

**CSV:**

```csv
Nome,CPF,Telefone
João José,123.456.789-00,(11) 98765-4321
```

**Payload:**

```json
{
  "cpf": "12345678900",
  "telefone": "11987654321",
  "nomeNormalizado": "Joao Jose",
  "slug": "joao-jose"
}
```

---

### Exemplo 4: Múltiplas Transformações no Mesmo Campo

```yaml
# Você pode usar a mesma coluna CSV para diferentes atributos
# com transformações diferentes
endpoints:
  - name: "api"
    mapping:
      # Nome original com title case
      - attribute: "nome"
        csvColumn: "Nome"
        transform: "title-case"
      
      # Nome em maiúsculas para busca
      - attribute: "nomeBusca"
        csvColumn: "Nome"
        transform: "uppercase"
      
      # Slug do nome para URL
      - attribute: "slug"
        csvColumn: "Nome"
        transform: "slugify"
      
      # Nome sem acentos para índice
      - attribute: "nomeIndice"
        csvColumn: "Nome"
        transform: "remove-accents"
```

**CSV:**

```csv
Nome
João José da Silva
```

**Payload:**

```json
{
  "nome": "João José Da Silva",
  "nomeBusca": "JOÃO JOSÉ DA SILVA",
  "slug": "joao-jose-da-silva",
  "nomeIndice": "Joao Jose da Silva"
}
```

---

## 🔗 Encadeamento de Transformações

**Nota:** Atualmente, apenas uma transformação pode ser aplicada por campo. Se você precisa de múltiplas transformações, use o mesmo campo CSV para diferentes atributos, como mostrado no Exemplo 4 acima.

---

## ⚠️ Observações Importantes

### Validação Antes da Transformação

As transformações são aplicadas **após** a validação dos dados. Certifique-se de que suas regexes de validação levem isso em conta.

```yaml
file:
  mapping:
    - column: "Email"
      type: "string"
      regex: "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"

endpoints:
  - mapping:
      - attribute: "email"
        csvColumn: "Email"
        transform: "lowercase"  # Transformação aplicada APÓS validação
```

### Valores Nulos ou Vazios

- Se o valor do CSV for vazio ou nulo, a transformação **não será aplicada**
- O valor será enviado como está (vazio/nulo)

### Performance

- Transformações são aplicadas em **memória** e são muito rápidas
- Não há impacto significativo na performance, mesmo com grandes volumes

---

## 📊 Tabela Resumo

| Transformação | Categoria | Exemplo |
|---------------|-----------|---------|
| `uppercase` | Texto | `"joão"` → `"JOÃO"` |
| `lowercase` | Texto | `"JOÃO"` → `"joão"` |
| `capitalize` | Texto | `"joão silva"` → `"João silva"` |
| `title-case` | Texto | `"joão silva"` → `"João Silva"` |
| `trim` | Limpeza | `"  texto  "` → `"texto"` |
| `remove-spaces` | Limpeza | `"a b c"` → `"abc"` |
| `remove-accents` | Limpeza | `"João"` → `"Joao"` |
| `remove-non-numeric` | Limpeza | `"123-456"` → `"123456"` |
| `remove-non-alphanumeric` | Limpeza | `"abc-123!"` → `"abc123"` |
| `format-cpf` | Brasil | `"12345678900"` → `"123.456.789-00"` |
| `format-cnpj` | Brasil | `"12345678000190"` → `"12.345.678/0001-90"` |
| `format-phone-br` | Brasil | `"11987654321"` → `"(11) 98765-4321"` |
| `format-cep` | Brasil | `"01310100"` → `"01310-100"` |
| `slugify` | Outras | `"João Silva"` → `"joao-silva"` |
| `reverse` | Outras | `"ABC"` → `"CBA"` |
| `base64-encode` | Outras | `"Hello"` → `"SGVsbG8="` |
| `url-encode` | Outras | `"a b"` → `"a%20b"` |

---

## 🎓 Próximos Passos

Agora que você conhece as transformações, explore:

- 🔍 **[Filtros](FILTERS.md)** - Processar apenas linhas específicas
- 💡 **[Exemplos](EXAMPLES.md)** - Ver casos de uso completos
- ⚙️ **[CLI Arguments](CLI-ARGUMENTS.md)** - Argumentos de linha de comando

---

## 📚 Documentação Relacionada

- 📖 [README Principal](../README.md)
- 🚀 [Quick Start](QUICKSTART.md)
- 💡 [Exemplos](EXAMPLES.md)
- 🔍 [Filtros](FILTERS.md)
- ⚙️ [Argumentos CLI](CLI-ARGUMENTS.md)

---

<div align="center">
  <p><strong>💡 Precisa de uma nova transformação? Abra uma issue no GitHub!</strong></p>
  <p>
    <a href="#-visão-geral">Voltar ao topo ⬆️</a>
  </p>
</div>
