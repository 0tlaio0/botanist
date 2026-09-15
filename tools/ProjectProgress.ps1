[CmdletBinding()]
param(
    [ValidateRange(1, 65535)]
    [int]$Port = 8765,

    [string]$BindAddress = "0.0.0.0",

    [switch]$GenerateOnly,

    [switch]$OpenFirewall,

    [string]$FirewallProfile = "Private,Domain"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$templatePath = Join-Path $PSScriptRoot "project-progress\index.template.html"
$outputPath = Join-Path $repoRoot "build\project-progress\index.html"

function Get-JsonValue {
    param(
        [Parameter(Mandatory)]
        $Object,

        [Parameter(Mandatory)]
        [string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-LocalizedValue {
    param(
        [Parameter(Mandatory)]
        $Localization,

        [Parameter(Mandatory)]
        [string]$Key
    )

    $value = Get-JsonValue -Object $Localization -Name $Key
    if ($null -eq $value) {
        return $Key
    }

    return [string]$value
}

function Convert-ToPascalSnake {
    param(
        [Parameter(Mandatory)]
        [string]$ClassName
    )

    $withoutPrefix = $ClassName -replace '^Botanist', ''
    return ($withoutPrefix -creplace '([a-z0-9])([A-Z])', '$1_$2').ToUpperInvariant()
}

function Get-RepoVersion {
    try {
        $commit = (& git -C $repoRoot rev-parse --short HEAD 2>$null).Trim()
        $branch = (& git -C $repoRoot branch --show-current 2>$null).Trim()
        $dirty = [bool](& git -C $repoRoot status --porcelain=v1 2>$null)

        return [ordered]@{
            commit = $commit
            branch = $branch
            dirty = $dirty
        }
    }
    catch {
        return [ordered]@{
            commit = ""
            branch = ""
            dirty = $false
        }
    }
}

function Get-CardCatalog {
    param(
        [Parameter(Mandatory)]
        $Localization
    )

    $cardsDirectory = Join-Path $repoRoot "Scripts\Models\Cards"
    $cards = [System.Collections.Generic.List[object]]::new()
    $seedClassNames = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)

    foreach ($file in Get-ChildItem -LiteralPath $cardsDirectory -Filter "*.cs" -File | Sort-Object Name) {
        $source = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
        $classMatch = [regex]::Match(
            $source,
            'public\s+class\s+(?<class>Botanist\w+)\s*:\s*(?<base>Botanist\w+)')
        $baseMatch = [regex]::Match(
            $source,
            'base\s*\(\s*(?<cost>\d+)\s*,\s*CardType\.(?<type>\w+)\s*,\s*CardRarity\.(?<rarity>\w+)',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

        if (-not $classMatch.Success -or -not $baseMatch.Success) {
            continue
        }

        $className = $classMatch.Groups["class"].Value
        $baseClass = $classMatch.Groups["base"].Value
        $seed = $baseClass -in @("BotanistSeedCardModel", "BotanistTargetedSeedCardModel")
        if ($seed) {
            [void]$seedClassNames.Add($className)
        }

        $localizationKey = "BOTANIST-BOTANIST_$(Convert-ToPascalSnake $className)"
        $elementMatch = [regex]::Match(
            $source,
            'BotanistElement\.(?<element>Earth|Fire|Water|Wind|Aether)')
        $requirements = [System.Collections.Generic.List[object]]::new()
        $requirementsMatch = [regex]::Match(
            $source,
            'Requirements\s*=>\s*\[(?<body>.*?)\]\s*;',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

        if ($requirementsMatch.Success) {
            foreach ($requirementMatch in [regex]::Matches(
                    $requirementsMatch.Groups["body"].Value,
                    'new\s*\(\s*BotanistElement\.(?<element>\w+)\s*,\s*(?<count>\d+)\s*\)')) {
                $requirements.Add([ordered]@{
                    element = $requirementMatch.Groups["element"].Value
                    count = [int]$requirementMatch.Groups["count"].Value
                })
            }
        }

        $cards.Add([ordered]@{
            name = Get-LocalizedValue -Localization $Localization -Key "$localizationKey.title"
            className = $className
            cost = [int]$baseMatch.Groups["cost"].Value
            type = $baseMatch.Groups["type"].Value
            rarity = $baseMatch.Groups["rarity"].Value
            seed = $seed
            element = if ($elementMatch.Success) { $elementMatch.Groups["element"].Value } else { "" }
            requirements = @($requirements)
            description = Get-LocalizedValue -Localization $Localization -Key "$localizationKey.description"
        })
    }

    return [ordered]@{
        items = @($cards)
        seedClassNames = $seedClassNames
    }
}

function Get-RelicCatalog {
    param(
        [Parameter(Mandatory)]
        $Localization
    )

    $relicsDirectory = Join-Path $repoRoot "Scripts\Models\Relics"
    $relics = [System.Collections.Generic.List[object]]::new()

    foreach ($file in Get-ChildItem -LiteralPath $relicsDirectory -Filter "*.cs" -File | Sort-Object Name) {
        $source = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
        $classMatch = [regex]::Match(
            $source,
            'public\s+class\s+(?<class>Botanist\w+)\s*:\s*CustomRelicModel')
        if (-not $classMatch.Success) {
            continue
        }

        $className = $classMatch.Groups["class"].Value
        $localizationKey = "BOTANIST-BOTANIST_$(Convert-ToPascalSnake $className)"
        $rarityMatch = [regex]::Match($source, 'RelicRarity\.(?<rarity>\w+)')
        $relics.Add([ordered]@{
            name = Get-LocalizedValue -Localization $Localization -Key "$localizationKey.title"
            className = $className
            rarity = if ($rarityMatch.Success) { $rarityMatch.Groups["rarity"].Value } else { "" }
            description = Get-LocalizedValue -Localization $Localization -Key "$localizationKey.description"
        })
    }

    return @($relics)
}

function Get-PotionCatalog {
    param(
        [Parameter(Mandatory)]
        $Localization
    )

    $potionsDirectory = Join-Path $repoRoot "Scripts\Models\Potions"
    $potions = [System.Collections.Generic.List[object]]::new()

    foreach ($file in Get-ChildItem -LiteralPath $potionsDirectory -Filter "*.cs" -File | Sort-Object Name) {
        $source = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
        $classMatch = [regex]::Match(
            $source,
            'public\s+class\s+(?<class>Botanist\w+)\s*:\s*CustomPotionModel')
        if (-not $classMatch.Success) {
            continue
        }

        $className = $classMatch.Groups["class"].Value
        $localizationKey = "BOTANIST-BOTANIST_$(Convert-ToPascalSnake $className)"
        $rarityMatch = [regex]::Match($source, 'PotionRarity\.(?<rarity>\w+)')
        $potions.Add([ordered]@{
            name = Get-LocalizedValue -Localization $Localization -Key "$localizationKey.title"
            className = $className
            rarity = if ($rarityMatch.Success) { $rarityMatch.Groups["rarity"].Value } else { "" }
            description = Get-LocalizedValue -Localization $Localization -Key "$localizationKey.description"
        })
    }

    return @($potions)
}

function Get-StartingDeckInfo {
    param(
        [Parameter(Mandatory)]
        [System.Collections.Generic.HashSet[string]]$SeedClassNames
    )

    $characterPath = Join-Path $repoRoot "Scripts\Models\Characters\BotanistCharacter.cs"
    $source = Get-Content -LiteralPath $characterPath -Raw -Encoding UTF8
    $deckMatch = [regex]::Match(
        $source,
        'StartingDeck\s*=>\s*\[(?<body>.*?)\]\s*;',
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $classNames = [System.Collections.Generic.List[string]]::new()

    if ($deckMatch.Success) {
        foreach ($match in [regex]::Matches(
                $deckMatch.Groups["body"].Value,
                'ModelDb\.Card<(?<class>\w+)>\(\)')) {
            $classNames.Add($match.Groups["class"].Value)
        }
    }

    $startingSeedCount = 0
    foreach ($className in $classNames) {
        if ($SeedClassNames.Contains($className)) {
            $startingSeedCount++
        }
    }

    return [ordered]@{
        count = $classNames.Count
        seedCount = $startingSeedCount
    }
}

function New-CardsByField {
    param(
        [Parameter(Mandatory)]
        [object[]]$Cards,

        [Parameter(Mandatory)]
        [string]$Field,

        [Parameter(Mandatory)]
        [string[]]$Keys
    )

    $result = [ordered]@{}
    foreach ($key in $Keys) {
        $result[$key] = @($Cards | Where-Object { $_.$Field -eq $key }).Count
    }

    return $result
}

function New-ProgressData {
    $cardsLocalization = Get-Content -LiteralPath (
        Join-Path $repoRoot "botanist\localization\zhs\cards.json") -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $relicsLocalization = Get-Content -LiteralPath (
        Join-Path $repoRoot "botanist\localization\zhs\relics.json") -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $potionsLocalization = Get-Content -LiteralPath (
        Join-Path $repoRoot "botanist\localization\zhs\potions.json") -Raw -Encoding UTF8 |
        ConvertFrom-Json

    $cardCatalog = Get-CardCatalog -Localization $cardsLocalization
    $cards = @($cardCatalog.items)
    $relics = @(Get-RelicCatalog -Localization $relicsLocalization)
    $potions = @(Get-PotionCatalog -Localization $potionsLocalization)
    $startingDeck = Get-StartingDeckInfo -SeedClassNames $cardCatalog.seedClassNames

    $typeTargets = [ordered]@{
        Attack = 19
        Skill = 38
        Power = 28
    }
    $rarityTargets = [ordered]@{
        Common = 19
        Uncommon = 38
        Rare = 28
    }
    $typeTotals = New-CardsByField -Cards $cards -Field "type" -Keys @("Attack", "Skill", "Power")
    $rarityTotals = New-CardsByField -Cards $cards -Field "rarity" -Keys @("Common", "Uncommon", "Rare")
    $elementTotals = New-CardsByField -Cards $cards -Field "element" -Keys @("Earth", "Fire", "Water", "Wind", "Aether")

    $nonTokenCount = @($cards | Where-Object { $_.rarity -ne "Token" }).Count
    $basicCount = @($cards | Where-Object { $_.rarity -eq "Basic" }).Count
    $tokenCount = @($cards | Where-Object { $_.rarity -eq "Token" }).Count
    $ancientCount = @($cards | Where-Object { $_.rarity -eq "Ancient" }).Count
    $seedCount = @($cards | Where-Object { $_.seed }).Count

    return [ordered]@{
        generatedAt = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss zzz")
        version = Get-RepoVersion
        targets = [ordered]@{
            cards = 85
            relics = 9
            potions = 3
            startingDeck = 10
            types = $typeTargets
            rarities = $rarityTargets
        }
        summary = [ordered]@{
            cards = [ordered]@{
                current = $cards.Count
                target = 85
                nonToken = $nonTokenCount
            }
            relics = [ordered]@{
                current = $relics.Count
                target = 9
            }
            potions = [ordered]@{
                current = $potions.Count
                target = 3
            }
        }
        cards = $cards
        relics = $relics
        potions = $potions
        counters = [ordered]@{
            seeds = $seedCount
            basic = $basicCount
            token = $tokenCount
            ancient = $ancientCount
            startingDeck = $startingDeck.count
            startingSeeds = $startingDeck.seedCount
        }
        types = $typeTotals
        rarities = $rarityTotals
        elements = $elementTotals
    }
}

function Build-ProgressHtml {
    if (-not (Test-Path -LiteralPath $templatePath -PathType Leaf)) {
        throw "找不到进度页面模板：$templatePath"
    }

    $template = Get-Content -LiteralPath $templatePath -Raw -Encoding UTF8
    $json = New-ProgressData | ConvertTo-Json -Depth 12 -Compress

    # JSON 内嵌到 script 标签时避免提前结束标签。
    $json = $json.Replace("</", "<\/")
    return $template.Replace("__PROJECT_DATA__", $json)
}

function Write-ProgressHtml {
    $outputDirectory = Split-Path -Parent $outputPath
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    Build-ProgressHtml | Set-Content -LiteralPath $outputPath -Encoding UTF8 -NoNewline
    return $outputPath
}

function Get-LanAddresses {
    $result = [System.Collections.Generic.List[string]]::new()

    foreach ($networkInterface in [System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces()) {
        if ($networkInterface.OperationalStatus -ne [System.Net.NetworkInformation.OperationalStatus]::Up) {
            continue
        }

        if ($networkInterface.Name -match 'Loopback|WSL|vEthernet|Virtual') {
            continue
        }

        foreach ($address in $networkInterface.GetIPProperties().UnicastAddresses) {
            if ($address.Address.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork) {
                continue
            }

            $ip = $address.Address.ToString()
            if ($ip -like "127.*" -or $ip -like "169.254.*") {
                continue
            }

            if (-not $result.Contains($ip)) {
                $result.Add($ip)
            }
        }
    }

    return @($result)
}

function Write-HttpResponse {
    param(
        [Parameter(Mandatory)]
        [System.Net.Sockets.NetworkStream]$Stream,

        [Parameter(Mandatory)]
        [int]$StatusCode,

        [Parameter(Mandatory)]
        [string]$StatusText,

        [Parameter(Mandatory)]
        [string]$ContentType,

        [Parameter(Mandatory)]
        [byte[]]$Body,

        [switch]$HeadOnly
    )

    $headers = @(
        "HTTP/1.0 $StatusCode $StatusText",
        "Content-Type: $ContentType",
        "Content-Length: $($Body.Length)",
        "Cache-Control: no-store, no-cache, must-revalidate",
        "Pragma: no-cache",
        "Connection: close",
        "",
        ""
    ) -join "`r`n"

    $headerBytes = [System.Text.Encoding]::ASCII.GetBytes($headers)
    $Stream.Write($headerBytes, 0, $headerBytes.Length)
    if (-not $HeadOnly -and $Body.Length -gt 0) {
        $Stream.Write($Body, 0, $Body.Length)
    }
}

function Enable-ProjectProgressFirewall {
    $ruleName = "Botanist Project Progress ($Port)"
    $isAdministrator = (
        [Security.Principal.WindowsPrincipal](
            [Security.Principal.WindowsIdentity]::GetCurrent())
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

    if (-not $isAdministrator) {
        throw "创建防火墙规则需要管理员权限，请以管理员身份重新运行此脚本并保留 -OpenFirewall。"
    }

    $existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
    if ($null -ne $existing) {
        Set-NetFirewallRule -DisplayName $ruleName -Enabled True -Profile $FirewallProfile -Action Allow
        return
    }

    New-NetFirewallRule `
        -DisplayName $ruleName `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort $Port `
        -Action Allow `
        -Profile $FirewallProfile | Out-Null
}

if ($OpenFirewall) {
    Enable-ProjectProgressFirewall
}

if ($GenerateOnly) {
    $writtenPath = Write-ProgressHtml
    Write-Host "项目进度页面已生成：$writtenPath"
    return
}

$bindIp = if ($BindAddress -in @("0.0.0.0", "*")) {
    [System.Net.IPAddress]::Any
}
else {
    [System.Net.IPAddress]::Parse($BindAddress)
}

$listener = [System.Net.Sockets.TcpListener]::new($bindIp, $Port)

try {
    $listener.Start()
}
catch {
    throw "无法监听端口 $Port。请确认端口未被占用，或使用 -Port 指定其他端口。原始错误：$($_.Exception.Message)"
}

$lanAddresses = Get-LanAddresses
Write-Host ""
Write-Host "植物学家项目进度统计已启动。" -ForegroundColor Green
Write-Host "本机访问：http://localhost:$Port/"
foreach ($address in $lanAddresses) {
    Write-Host "局域网/虚拟网访问：http://${address}:$Port/"
}
Write-Host ""
Write-Host "页面每次刷新都会重新扫描当前源码。按 Ctrl+C 停止服务。"
Write-Host "若其他电脑无法访问，请确认 Windows 防火墙已放行 TCP $Port；可运行："
Write-Host "  .\tools\ProjectProgress.ps1 -Port $Port -OpenFirewall -GenerateOnly"
Write-Host ""

try {
    while ($true) {
        $client = $listener.AcceptTcpClient()
        try {
            $client.ReceiveTimeout = 5000
            $client.SendTimeout = 5000
            $stream = $client.GetStream()
            $reader = [System.IO.StreamReader]::new(
                $stream,
                [System.Text.Encoding]::ASCII,
                $false,
                1024,
                $true)
            $requestLine = $reader.ReadLine()

            if ([string]::IsNullOrWhiteSpace($requestLine)) {
                continue
            }

            while ($null -ne ($line = $reader.ReadLine()) -and $line.Length -gt 0) {
                # 读取并丢弃请求头。
            }

            $parts = $requestLine.Split(" ")
            $method = $parts[0].ToUpperInvariant()
            $requestTarget = if ($parts.Length -gt 1) { $parts[1] } else { "/" }
            $path = $requestTarget.Split("?")[0]

            switch ($path) {
                "/" {
                    $html = Build-ProgressHtml
                    $body = [System.Text.Encoding]::UTF8.GetBytes($html)
                    Write-HttpResponse `
                        -Stream $stream `
                        -StatusCode 200 `
                        -StatusText "OK" `
                        -ContentType "text/html; charset=utf-8" `
                        -Body $body `
                        -HeadOnly:($method -eq "HEAD")
                }
                "/index.html" {
                    $html = Build-ProgressHtml
                    $body = [System.Text.Encoding]::UTF8.GetBytes($html)
                    Write-HttpResponse `
                        -Stream $stream `
                        -StatusCode 200 `
                        -StatusText "OK" `
                        -ContentType "text/html; charset=utf-8" `
                        -Body $body `
                        -HeadOnly:($method -eq "HEAD")
                }
                "/health" {
                    $body = [System.Text.Encoding]::UTF8.GetBytes('{"status":"ok"}')
                    Write-HttpResponse `
                        -Stream $stream `
                        -StatusCode 200 `
                        -StatusText "OK" `
                        -ContentType "application/json; charset=utf-8" `
                        -Body $body `
                        -HeadOnly:($method -eq "HEAD")
                }
                "/favicon.ico" {
                    Write-HttpResponse `
                        -Stream $stream `
                        -StatusCode 204 `
                        -StatusText "No Content" `
                        -ContentType "image/x-icon" `
                        -Body ([byte[]]@())
                }
                default {
                    $body = [System.Text.Encoding]::UTF8.GetBytes("Not Found")
                    Write-HttpResponse `
                        -Stream $stream `
                        -StatusCode 404 `
                        -StatusText "Not Found" `
                        -ContentType "text/plain; charset=utf-8" `
                        -Body $body `
                        -HeadOnly:($method -eq "HEAD")
                }
            }
        }
        catch {
            Write-Warning "处理请求失败：$($_.Exception.Message)"
        }
        finally {
            $client.Dispose()
        }
    }
}
finally {
    $listener.Stop()
}
