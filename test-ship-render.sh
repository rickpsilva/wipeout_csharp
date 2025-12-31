#!/bin/bash
# Ship Render Test Tool - Debug ship rendering
cd "$(dirname "$0")"
export MESA_D3D12_DEFAULT_ADAPTER_NAME=NVIDIA
export GALLIUM_DRIVER=d3d12
export LIBGL_ALWAYS_INDIRECT=false
export LIBGL_ALWAYS_SOFTWARE=false

dotnet run --project tools/ShipRenderTest/ShipRenderTest.csproj