# Ship Render Test Tool

Ferramenta dedicada para debugar renderização de ships 3D.

## Propósito

Esta ferramenta foi criada para isolar e debugar problemas de renderização de ships, permitindo:
- Testar diferentes escalas
- Testar diferentes posições
- Visualizar vértices e geometria
- Debug de transformações
- Comparar configurações lado a lado

## Como Usar

### Executar

```bash
# Opção 1: Script direto
./test-ship-render.sh

# Opção 2: dotnet run
dotnet run --project tools/ShipRenderTest/ShipRenderTest.csproj
```

### Controles

| Tecla | Ação |
|-------|------|
| `SPACE` | Trocar para próxima configuração de teste |
| `R` | Toggle rotação da ship |
| `+` / `-` | Aumentar/diminuir escala |
| `Setas` | Mover ship (←→↑↓) |
| `ESC` | Sair |

### Configurações de Teste

A ferramenta testa automaticamente 6 configurações:

1. **Full scale - Center**: Escala 1.0x no centro (640, 360)
2. **Half scale - Center**: Escala 0.5x no centro
3. **0.3x scale - Upper left**: Escala 0.3x em (400, 300) - configuração atual do menu
4. **0.2x scale - Center**: Escala 0.2x no centro
5. **0.1x scale - Center**: Escala 0.1x no centro
6. **0.05x scale - Center**: Escala 0.05x no centro

## Debug Output

A ferramenta loga informações detalhadas:
- Vértices do modelo (primeiro e último)
- Contagem de primitivas
- Posição e ângulo da ship
- Escala atual

## Visualização

- **Cruz vermelha**: Centro da tela
- **Marcador verde**: Posição da ship
- **Fundo azul escuro**: Para contraste

## Identificação de Problemas

### Ship não aparece
- Verifique os logs de vértices - se estão em escala apropriada
- Use `+`/`-` para ajustar escala em tempo real
- Use setas para mover e encontrar a ship
- Pressione `R` para rodar e ver se há geometria

### Ship muito pequena
- Pressione `SPACE` para testar escalas maiores
- Use `+` para aumentar gradualmente

### Ship muito grande
- Pressione `SPACE` para testar escalas menores
- Use `-` para diminuir gradualmente

## Integração

Depois de encontrar a configuração ideal:
1. Note a escala e posição no log
2. Atualize `ShipPreview.Initialize()` com esses valores
3. Atualize `ModelLoader.CreateMockShipModelScaled()` se necessário

## Arquitetura

```
tools/ShipRenderTest/
├── Program.cs              # Entry point
├── ShipRenderTest.csproj   # Project file
└── README.md               # This file

src/Tools/
└── ShipRenderTest.cs       # Main test window
```

## Próximos Passos

Esta ferramenta é o primeiro passo para o **WipeoutStudio** proposto em `docs/ARCHITECTURE_IMPROVEMENTS.md`.

Features futuras:
- Múltiplas ships lado a lado
- Comparação de modelos (mock vs PRM real)
- Editor de vértices em tempo real
- Export de configurações
- Visualização de normals e UVs
