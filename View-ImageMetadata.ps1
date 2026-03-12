# View Image Metadata
# Usage: .\View-ImageMetadata.ps1 "path\to\image.png"

param(
    [Parameter(Mandatory=$true)]
    [string]$ImagePath
)

if (-not (Test-Path $ImagePath)) {
    Write-Host "Error: File not found: $ImagePath" -ForegroundColor Red
    exit
}

$extension = [System.IO.Path]::GetExtension($ImagePath).ToLower()

if ($extension -eq ".svg") {
    # SVG - Just show the metadata section
    Write-Host "`nSVG Metadata from: $ImagePath" -ForegroundColor Cyan
    Write-Host ("=" * 60) -ForegroundColor Gray
    
    $content = Get-Content $ImagePath -Raw
    
    # Extract metadata from desc tag
    if ($content -match '<g id="visualization-parameters"[^>]*>.*?<desc>(.*?)</desc>') {
        Write-Host $matches[1] -ForegroundColor Green
    } else {
        Write-Host "No visualization metadata found in SVG" -ForegroundColor Yellow
    }
} else {
    # PNG/JPEG/BMP - Use System.Drawing
    Add-Type -AssemblyName System.Drawing
    
    try {
        $img = [System.Drawing.Image]::FromFile($ImagePath)
        
        Write-Host "`nImage Metadata from: $ImagePath" -ForegroundColor Cyan
        Write-Host ("=" * 60) -ForegroundColor Gray
        Write-Host "Size: $($img.Width) x $($img.Height) pixels" -ForegroundColor Yellow
        Write-Host "Format: $($img.RawFormat)" -ForegroundColor Yellow
        Write-Host "`nEmbedded Metadata:" -ForegroundColor Magenta
        
        $foundMetadata = $false
        
        foreach ($prop in $img.PropertyItems) {
            $value = [System.Text.Encoding]::ASCII.GetString($prop.Value).TrimEnd("`0")
            
            switch ($prop.Id) {
                0x010E { 
                    Write-Host "`n[Image Description]" -ForegroundColor Cyan
                    Write-Host $value -ForegroundColor White
                    $foundMetadata = $true
                }
                0x0131 { 
                    Write-Host "`n[Software]" -ForegroundColor Cyan
                    Write-Host $value -ForegroundColor White
                    $foundMetadata = $true
                }
                0x9286 { 
                    Write-Host "`n[Full Parameters]" -ForegroundColor Cyan
                    Write-Host $value -ForegroundColor Green
                    $foundMetadata = $true
                }
            }
        }
        
        if (-not $foundMetadata) {
            Write-Host "`nNo custom metadata found in this image." -ForegroundColor Yellow
        }
        
        $img.Dispose()
    }
    catch {
        Write-Host "Error reading image: $_" -ForegroundColor Red
    }
}

Write-Host "`n"
