#!/bin/bash
echo "Starting wipeout_csharp visual test..."
echo "Window should display sprites with arrow key movement"
echo "Press ESC to close"
echo ""

timeout 3 dotnet run 2>&1 &
PID=$!

sleep 1
echo "✓ Application should now be running for 3 seconds..."
wait $PID

echo "✓ Test completed"
