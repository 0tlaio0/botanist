$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$cardsPath = Join-Path $root "botanist\localization\zhs\cards.json"
$cardsDirectory = Join-Path $root "Scripts\Models\Cards"

if (-not (Test-Path $cardsPath)) {
    throw "Missing card localization file: $cardsPath"
}

$cards = Get-Content -Raw -Encoding UTF8 -LiteralPath $cardsPath | ConvertFrom-Json
$errors = [System.Collections.Generic.List[string]]::new()
$checkedCards = 0

function ConvertTo-LocalizationId([string] $className) {
    $snake = [regex]::Replace($className, "([a-z0-9])([A-Z])", '$1_$2')
    $snake = [regex]::Replace($snake, "([A-Z]+)([A-Z][a-z])", '$1_$2')
    return "BOTANIST-" + $snake.ToUpperInvariant()
}

function Get-DeclaredVariableName(
    [string] $variableType,
    [string] $genericType,
    [string] $firstArgument,
    [hashtable] $constants) {
    if ($firstArgument.StartsWith('"')) {
        return $firstArgument.Trim('"')
    }

    if ($constants.ContainsKey($firstArgument)) {
        return $constants[$firstArgument]
    }

    if ($variableType -eq "PowerVar" -and $genericType) {
        return $genericType.Split(".")[-1]
    }

    switch ($variableType) {
        "DamageVar" { return "Damage" }
        "OstyDamageVar" { return "OstyDamage" }
        "BlockVar" { return "Block" }
        "CardsVar" { return "Cards" }
        "HealVar" { return "Heal" }
        "RepeatVar" { return "Repeat" }
    }

    return $null
}

foreach ($file in Get-ChildItem -LiteralPath $cardsDirectory -Filter "*.cs" -File) {
    $source = Get-Content -Raw -Encoding UTF8 -LiteralPath $file.FullName
    if ($source -notmatch "\[Pool\(typeof\(BotanistCardPool\)\)\]" -or
        $source -notmatch "public class (?<class>Botanist[A-Za-z0-9_]+)") {
        continue
    }

    $checkedCards++
    $className = $Matches["class"]
    $localizationId = ConvertTo-LocalizationId $className
    $descriptionKey = "$localizationId.description"
    $descriptionProperty = $cards.PSObject.Properties[$descriptionKey]
    if ($null -eq $descriptionProperty) {
        $errors.Add("$($file.Name): missing localization key '$descriptionKey'.")
        continue
    }

    $constants = @{}
    foreach ($match in [regex]::Matches(
        $source,
        'private\s+const\s+string\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*"(?<value>[^"]+)"')) {
        $constants[$match.Groups["name"].Value] = $match.Groups["value"].Value
    }

    $declaredVariables = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    $variablePattern = 'new\s+(?<type>[A-Za-z0-9_]+Var)(?:<(?<generic>[^>]+)>)?\s*\(\s*(?<first>[^,\)]+)'
    foreach ($match in [regex]::Matches($source, $variablePattern)) {
        $variableType = $match.Groups["type"].Value
        $genericType = $match.Groups["generic"].Value
        $firstArgument = $match.Groups["first"].Value.Trim()
        $variableName = Get-DeclaredVariableName `
            -variableType $variableType `
            -genericType $genericType `
            -firstArgument $firstArgument `
            -constants $constants

        if ([string]::IsNullOrWhiteSpace($variableName)) {
            $errors.Add("$($file.Name): cannot resolve dynamic variable '$match'.")
            continue
        }

        $declaredVariables.Add($variableName) | Out-Null
    }

    foreach ($match in [regex]::Matches($source, 'description\.Add\("(?<name>[^"]+)"')) {
        $declaredVariables.Add($match.Groups["name"].Value) | Out-Null
    }

    $description = [string] $descriptionProperty.Value
    $placeholderPattern = '\{(?<name>[A-Za-z_][A-Za-z0-9_]*)(?::[^}]*)?\}'
    foreach ($match in [regex]::Matches($description, $placeholderPattern)) {
        $placeholder = $match.Groups["name"].Value
        if (-not $declaredVariables.Contains($placeholder)) {
            $errors.Add(
                "$($file.Name): description references '{$placeholder}' but no matching dynamic variable is declared.")
        }
    }
}

if ($errors.Count -gt 0) {
    foreach ($errorMessage in $errors) {
        Write-Error $errorMessage
    }

    exit 1
}

Write-Host "Validated card localization variables for $checkedCards cards."
