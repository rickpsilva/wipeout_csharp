#!/usr/bin/env bash
set -euo pipefail

echo "Running wipeout_csharp..."
#export SHIP_INDEX=7   #Forçar índice do navio 
#export PRM_LE_FIRST=1 #Ative heurística little-endian para diagnóstico
#export SHIPPREVIEW_PERMISSIVE=1 #Permitir pré-visualização de navio não oficial
#export SHIPPREVIEW_OVERLAY=1 #Ativar sobreposição de pré-visualização do navio
#export SHIPPREVIEW_OVERLAY_WIREFRAME=1 #Ativar sobreposição de pré-visualização do navio em modo wireframe
#export SHIPPREVIEW_OVERLAY_LABELS=1 #Ativar sobreposição de pré-visualização do navio com etiquetas
#export SHIPPREVIEW_OVERLAY_UVSAMPLES=1 #Ativar sobreposição de pré-visualização do navio com amostras UV
export SHIPPREVIEW_FLIP_UV_X=1
dotnet run --project wipeout_csharp.csproj
