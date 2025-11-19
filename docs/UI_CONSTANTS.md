# Sistema de Constantes UI (UIConstants)

## Visão Geral

O ficheiro `UIConstants.cs` centraliza todas as configurações de UI (tamanhos de fonte, cores, espaçamentos e strings) num único local, facilitando manutenção e futura internacionalização.

## Estrutura

### 1. **FontSizes** - Tamanhos de Fonte
```csharp
UIConstants.FontSizes.MenuTitle    // 16
UIConstants.FontSizes.MenuItem     // 16
UIConstants.FontSizes.SplashText   // 16
UIConstants.FontSizes.Credits      // 16
```

### 2. **Colors** - Paleta de Cores
```csharp
UIConstants.Colors.MenuTitleDefault   // Branco (títulos)
UIConstants.Colors.MenuItemDefault    // Branco (items)
UIConstants.Colors.MenuItemSelected   // Amarelo/Dourado (item selecionado)
UIConstants.Colors.MenuItemDisabled   // Cinzento (item desativado)
UIConstants.Colors.SplashText         // Cinzento (PRESS ENTER)
UIConstants.Colors.CreditsTitle       // Branco (títulos dos créditos)
UIConstants.Colors.CreditsText        // Cinzento claro (texto dos créditos)
```

### 3. **Spacing** - Espaçamentos
```csharp
UIConstants.Spacing.MenuTitleLineHeight        // 24px
UIConstants.Spacing.MenuItemVerticalSpacing    // 24px
UIConstants.Spacing.MenuItemHorizontalSpacing  // 80px
UIConstants.Spacing.CreditsLineHeight          // 30px
```

### 4. **Strings** - Textos da UI
```csharp
UIConstants.Strings.SplashPressEnter  // "PRESS ENTER"
UIConstants.Strings.MenuStartGame     // "START GAME"
UIConstants.Strings.MenuOptions       // "OPTIONS"
UIConstants.Strings.QuitTitle         // "ARE YOU SURE YOU\nWANT TO QUIT"
UIConstants.Strings.CreditsLines      // Array com todas as linhas
// ... e muitos outros
```

## Como Usar

### Importar com `using static`
```csharp
using static WipeoutRewrite.Infrastructure.UI.UIConstants;

// Depois podes usar diretamente:
var titleSize = FontSizes.MenuTitle;
var color = Colors.MenuItemSelected;
var text = Strings.SplashPressEnter;
```

### Exemplo Prático
```csharp
// Antes (hardcoded)
DrawTextCentered("PRESS ENTER", pos, 16, new Color4(0.5f, 0.5f, 0.5f, 1.0f));

// Depois (usando constantes)
DrawTextCentered(Strings.SplashPressEnter, pos, FontSizes.SplashText, Colors.SplashText);
```

## Ficheiros Refactorizados

✅ **MenuRenderer.cs** - Usa FontSizes, Colors, Spacing
✅ **TitleScreen.cs** - Usa Strings.SplashPressEnter, Colors.SplashText
✅ **CreditsScreen.cs** - Usa Strings.CreditsLines, Colors, Spacing
✅ **MainMenuPages.cs** - Usa Strings para todos os labels dos menus

## Vantagens

1. **Manutenção Centralizada**: Mudar um tamanho/cor afeta toda a aplicação
2. **Consistência**: Garante que todos os elementos usam os mesmos valores
3. **Preparado para i18n**: Fácil adicionar traduções (PT, ES, FR, etc)
4. **Refactoring Seguro**: Compiler avisa se mudares nomes
5. **Documentação Implícita**: Nomes descritivos autodocumentam o código

## Próximos Passos (Futuro)

### Fase 1: Sistema de Configuração JSON (opcional)
```json
{
  "ui": {
    "fonts": { "menuTitle": 16, "menuItem": 16 },
    "colors": { "accent": "#FFC800", "default": "#FFFFFF" },
    "spacing": { "menuItemVertical": 24 }
  }
}
```

### Fase 2: Internacionalização (i18n)
```json
{
  "languages": {
    "en": { "menu.quit.title": "ARE YOU SURE YOU\nWANT TO QUIT" },
    "pt": { "menu.quit.title": "TEM A CERTEZA QUE\nQUER SAIR" },
    "es": { "menu.quit.title": "¿ESTÁS SEGURO QUE\nQUIERES SALIR?" }
  }
}
```

### Fase 3: Hot Reload durante Desenvolvimento
- Modificar JSON sem recompilar
- Ver mudanças em tempo real

## Notas

- Por agora mantemos constantes C# para **velocidade de desenvolvimento**
- Sistema está preparado para evolução futura sem quebrar código existente
- Todos os valores coincidem com o jogo original (Wipeout 1995)
