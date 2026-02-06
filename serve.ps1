# Simple HTTP Server for testing
$port = 8000
$url = "http://localhost:$port/"

Write-Host "Starting local web server..." -ForegroundColor Green
Write-Host ""
Write-Host "🎮 Browser app: http://localhost:$port/web/" -ForegroundColor Cyan
Write-Host "📄 Landing page: http://localhost:$port/index.html" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add($url)
$listener.Start()

Write-Host "Server is running on port $port" -ForegroundColor Green

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response
        
        $path = $request.Url.LocalPath
        if ($path -eq '/') { $path = '/index.html' }
        
        $filePath = Join-Path -Path $PSScriptRoot -ChildPath $path.TrimStart('/')
        
        if (Test-Path $filePath -PathType Leaf) {
            $content = [System.IO.File]::ReadAllBytes($filePath)
            
            $ext = [System.IO.Path]::GetExtension($filePath)
            $contentType = switch ($ext) {
                '.html' { 'text/html; charset=utf-8' }
                '.css'  { 'text/css; charset=utf-8' }
                '.js'   { 'application/javascript; charset=utf-8' }
                '.json' { 'application/json; charset=utf-8' }
                '.png'  { 'image/png' }
                '.jpg'  { 'image/jpeg' }
                '.gif'  { 'image/gif' }
                default { 'application/octet-stream' }
            }
            
            $response.ContentType = $contentType
            $response.ContentLength64 = $content.Length
            $response.OutputStream.Write($content, 0, $content.Length)
        } else {
            $response.StatusCode = 404
            $message = [System.Text.Encoding]::UTF8.GetBytes("404 - File Not Found: $path")
            $response.OutputStream.Write($message, 0, $message.Length)
        }
        
        $response.Close()
        
        $statusColor = if ($response.StatusCode -eq 200) { 'Green' } else { 'Red' }
        Write-Host "$($request.HttpMethod) $($request.Url.LocalPath) - $($response.StatusCode)" -ForegroundColor $statusColor
    }
} finally {
    $listener.Stop()
}
