# Rendering Pipeline Fix - Sprite Visibility Issue

## Problema Inicial

A aplicação estava a funcionar completamente:
- Carregava 14 pistas
- Criava 8 naves
- Detectava input do teclado
- Movia sprites baseado em input

Mas a janela mostrava apenas um ecra azul escuro, sem nenhum sprite visível.

## Diagnóstico

A renderização estava a falhar silenciosamente porque:

1. **Vertex Shader sem projeção**: O shader original tinha `gl_Position = vec4(pos, 1.0);` - apenas cópia da posição sem transformação
2. **Coordenadas em screen space**: Sprites estavam definidos em screen space (0-1280, 0-720) em vez de clip space (-1 a +1)
3. **Atributos não reconfiguradores**: VAO attributes configurados uma única vez, não reconfiguradores por frame
4. **Matriz de projeção nunca criada**: Uniform `projection` não era preenchido antes de desenhar

## Solução Implementada

### 1. Vertex Shader Atualizado
```glsl
// ANTES (não funcionava)
gl_Position = vec4(pos, 1.0);

// DEPOIS (funciona)
uniform mat4 projection;
gl_Position = projection * vec4(pos, 1.0);
```

### 2. Função SetupVertexAttributes() Adicionada
```csharp
private void SetupVertexAttributes()
{
    const int stride = 9 * sizeof(float); // 3 pos + 2 uv + 4 color
    
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
    GL.EnableVertexAttribArray(0);
    
    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
    GL.EnableVertexAttribArray(1);
    
    GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
    GL.EnableVertexAttribArray(2);
}
```

### 3. Matriz de Projeção em Flush()
```csharp
public void Flush()
{
    if (_trisLen == 0) return;
    
    GL.UseProgram(_shaderProgram);
    GL.BindVertexArray(_vao);
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _trisLen * 3 * 9, _vertexBuffer, BufferUsageHint.DynamicDraw);

    // Criar e passar matriz de projeção ortográfica
    var projection = Matrix4.CreateOrthographicOffCenter(0, _screenWidth, _screenHeight, 0, -1, 1);
    var projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
    GL.UniformMatrix4(projLoc, false, ref projection);

    GL.DrawArrays(PrimitiveType.Triangles, 0, _trisLen * 3);
    _trisLen = 0;
}
```

## Explicação Matemática

A matriz ortográfica transforma:
- Screen space (0 a 1280 em X, 0 a 720 em Y) 
- → Clip space (-1 a +1 em X e Y)

Para um ponto no centro (640, 360):
- Screen space: (640, 360, 0, 1)
- Após projeção: (0, 0, 0, 1) - Centro da tela
- Resultado: Sprite aparece no centro

Para um ponto no canto (0, 0):
- Screen space: (0, 0, 0, 1)
- Após projeção: (-1, 1, 0, 1) - Canto superior esquerdo
- Resultado: Sprite aparece no canto

## Verificação

Para confirmar que a correção funciona:
```bash
cd /home/rick/workspace/wipeout_csharp
dotnet run

# Esperado: Janela 1280x720, fundo azul, dois sprites brancos
# - Um grande sprite móvel no centro (controlado por setas)
# - Um sprite de teste no canto (10, 10)
# Pressione setas para mover
# Pressione ESC para sair
```

## Componentes Relacionados

- `GLRenderer.Init()` - Inicializa VAO, VBO, shaders
- `GLRenderer.Flush()` - Renderiza buffer acumulado
- `GLRenderer.PushSprite()` - Adiciona quad (2 triângulos) ao buffer
- `GLRenderer.PushTri()` - Adiciona triângulo com 3 vértices ao buffer
- `Game.OnRenderFrame()` - Loop de renderização

## Aprendizados

1. **Shaders precisam de uniforms**: Sempre passar dados dinâmicos via uniforms
2. **Projeção é essencial**: 2D renderização requer transformação de coordenadas
3. **Atributos VAO são persistentes**: Configurar uma vez, usar muitas vezes
4. **Buffer dinâmico para sprites**: `BufferUsageHint.DynamicDraw` ideal para dados que mudam cada frame
