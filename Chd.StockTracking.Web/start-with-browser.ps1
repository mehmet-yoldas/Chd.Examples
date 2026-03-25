# Start Vite and auto-open browser
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CHD.STOCKTRACKING.WEB - Port 3001         " -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "🚀 Starting Vite dev server..." -ForegroundColor Yellow
Write-Host ""

# Start browser opener in background (wait 8 seconds then open)
Start-Job -ScriptBlock {
    Start-Sleep -Seconds 8
    Start-Process "http://localhost:3001"
} | Out-Null

# Start Vite (this will block and show output)
npm run dev


