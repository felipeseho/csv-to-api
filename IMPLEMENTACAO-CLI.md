# Implementação de Argumentos de Linha de Comando

## Resumo das Mudanças

Esta implementação adiciona suporte completo a argumentos de linha de comando para parametrizar o processamento de arquivos CSV, permitindo sobrescrever qualquer configuração do arquivo YAML.

## Arquivos Criados

1. **Models/CommandLineOptions.cs**
   - Classe modelo para armazenar opções de linha de comando
   - Suporta todas as configurações principais do YAML

2. **ARGUMENTOS.md**
   - Guia completo de todos os argumentos disponíveis
   - Exemplos práticos de uso
   - Dicas e melhores práticas

## Arquivos Modificados

1. **CsvToApi.csproj**
   - Adicionado pacote `System.CommandLine` (v2.0.0-beta4) para parsing robusto de argumentos

2. **Program.cs**
   - Implementado parsing completo de argumentos CLI
   - Integração com System.CommandLine
   - Mensagens de erro amigáveis com emojis
   - Modo verboso para debug

3. **Services/ConfigurationService.cs**
   - Novo método `MergeWithCommandLineOptions()` para mesclar configurações
   - Permite sobrescrita seletiva de qualquer parâmetro do YAML

4. **README.md**
   - Adicionada seção sobre argumentos de linha de comando
   - Tabela completa de opções disponíveis
   - Exemplos práticos de uso

5. **EXEMPLOS.md**
   - Adicionados exemplos práticos com argumentos CLI
   - Cenários reais de uso

6. **QUICKSTART.md**
   - Atualizado com exemplos usando argumentos
   - Facilitado o teste rápido sem editar arquivos

## Argumentos Implementados

| Argumento | Curto | Descrição | Tipo |
|-----------|-------|-----------|------|
| `--config` | `-c` | Arquivo de configuração YAML | string |
| `--input` | `-i` | Arquivo CSV de entrada | string |
| `--batch-lines` | `-b` | Linhas por lote | int |
| `--log-path` | `-l` | Arquivo de log | string |
| `--delimiter` | `-d` | Delimitador CSV | string |
| `--endpoint` | `-e` | URL da API | string |
| `--auth-token` | `-a` | Token de autenticação | string |
| `--method` | `-m` | Método HTTP (POST/PUT) | string |
| `--timeout` | `-t` | Timeout em segundos | int |
| `--verbose` | `-v` | Logs detalhados | bool |

## Exemplos de Uso

### Comando Básico
```bash
dotnet run -- --help
```

### Teste Rápido
```bash
dotnet run -- --input data/test.csv --batch-lines 10 --verbose
```

### Produção Completa
```bash
dotnet run -- \
  --config config-prod.yaml \
  --input data/vendas.csv \
  --endpoint https://api.producao.com/vendas \
  --auth-token "Bearer xyz123" \
  --batch-lines 1000 \
  --timeout 90 \
  --verbose
```

### Teste com Webhook
```bash
dotnet run -- \
  --endpoint "https://webhook.site/sua-url" \
  --batch-lines 5 \
  --verbose
```

## Prioridade de Configuração

1. **Argumentos de linha de comando** (maior prioridade)
2. Arquivo YAML especificado em `--config`
3. `config.yaml` padrão

## Vantagens

✅ **Flexibilidade**: Sobrescreve configurações sem editar arquivos
✅ **CI/CD**: Ideal para pipelines automatizados
✅ **Testes**: Facilita testes rápidos com diferentes configurações
✅ **Debug**: Modo verboso para acompanhar o processamento
✅ **Usabilidade**: Help integrado e mensagens claras

## Testes Realizados

### Teste 1: Ajuda
```bash
$ dotnet run -- --help
# ✅ Exibe todas as opções disponíveis
```

### Teste 2: Modo Verboso
```bash
$ dotnet run -- -v
📋 Configuração carregada:
  Config: config.yaml
🚀 Iniciando processamento do arquivo CSV...
Processadas 5 linhas. Erros: 0
✅ Processamento concluído com sucesso!
```

### Teste 3: Sobrescrever Batch Lines
```bash
$ dotnet run -- --batch-lines 2 --verbose
📋 Configuração carregada:
  Config: config.yaml
  Batch Lines: 2
🚀 Iniciando processamento do arquivo CSV...
Processadas 2 linhas. Erros: 0
Processadas 4 linhas. Erros: 0
Processadas 5 linhas. Erros: 0
✅ Processamento concluído com sucesso!
```

## Compatibilidade

- ✅ .NET 10
- ✅ System.CommandLine 2.0 beta
- ✅ Mantém retrocompatibilidade com uso anterior (sem argumentos)
- ✅ Funciona com arquivos YAML existentes

## Próximos Passos Sugeridos

1. Adicionar validação de regex para formato de argumentos
2. Implementar profiles de configuração (dev, staging, prod)
3. Adicionar suporte a variáveis de ambiente
4. Criar script de build para diferentes plataformas
5. Adicionar telemetria e métricas de processamento

## Documentação Adicional

- **README.md**: Documentação principal com exemplos
- **ARGUMENTOS.md**: Guia completo de argumentos
- **EXEMPLOS.md**: Casos de uso práticos
- **QUICKSTART.md**: Início rápido atualizado
