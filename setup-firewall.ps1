# SMTP防火墙配置脚本
# 以管理员身份运行此脚本

Write-Host "正在配置防火墙规则..." -ForegroundColor Green

# 允许SMTP端口587
Write-Host "添加SMTP端口587规则..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName "SMTP端口587" -Direction Inbound -Protocol TCP -LocalPort 587 -Action Allow -Profile Any

# 允许SMTP端口465
Write-Host "添加SMTP端口465规则..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName "SMTP端口465" -Direction Inbound -Protocol TCP -LocalPort 465 -Action Allow -Profile Any

# 允许DotNet程序
Write-Host "添加DotNet程序规则..." -ForegroundColor Yellow
$dotnetPath = (Get-Command dotnet).Source
if ($dotnetPath) {
    New-NetFirewallRule -DisplayName "DotNet应用程序" -Direction Inbound -Program $dotnetPath -Action Allow -Profile Any
}

Write-Host "防火墙配置完成！" -ForegroundColor Green
Write-Host "请按任意键继续..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")