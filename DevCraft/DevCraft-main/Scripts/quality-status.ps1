# DevCraft Code Quality Status Script
param([string]$Action = "status")

function Write-ColoredOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Show-Status {
    Write-ColoredOutput "=== DevCraft Code Quality Status ===" "Green"
    Write-Host ""
      Write-ColoredOutput "COMPLETED IMPROVEMENTS:" "Green"
    $completed = @(
        "Fixed 'spurce' -> 'spruce' typo in assets",
        "Created UIConstants class for configuration values",
        "Created UILayoutHelper for responsive positioning", 
        "Created UIResourceManager for proper memory management",
        "Refactored Inventory class with responsive layout",
        "Added IDisposable pattern to Inventory",
        "Updated GameMenu to use UIConstants",
        "Eliminated hardcoded positioning in UI components",
        "Fixed MainMenu text wrapping and layout issues",
        "Created base UIComponent class for shared functionality",
        "Implemented proper error handling in MonitorHelper", 
        "Added proper disposal to MainMenu and GameMenu",
        "Enhanced color definitions in UIConstants"
    )
    $completed | ForEach-Object { Write-ColoredOutput "  [DONE] $_" "DarkGreen" }
    
    Write-Host ""
    Write-ColoredOutput "NEXT PRIORITY TASKS:" "Yellow"    $nextTasks = @(
        "Standardize texture file formats (.png vs .jpg)",
        "Optimize Content.mgcb processor parameters",
        "Add event-driven communication system",
        "Implement asset caching for better performance"
    )
    $nextTasks | ForEach-Object { Write-ColoredOutput "  [TODO] $_" "Yellow" }
    
    Write-Host ""
    Write-ColoredOutput "BUILD STATUS: " "White" -NoNewline
    try {
        $null = & dotnet build --verbosity quiet 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-ColoredOutput "PASSING" "Green"
        } else {
            Write-ColoredOutput "FAILING" "Red"
        }
    } catch {
        Write-ColoredOutput "UNKNOWN" "Yellow"
    }
}

function Show-NextSteps {
    Write-ColoredOutput "=== Next Implementation Steps ===" "Cyan"
    Write-Host ""
    Write-ColoredOutput "1. Add null checks in UI components" "White"
    Write-ColoredOutput "2. Create base UIComponent abstract class" "White"
    Write-ColoredOutput "3. Implement proper disposal patterns" "White"
    Write-ColoredOutput "4. Add configuration validation" "White"
}

switch ($Action.ToLower()) {
    "status" { Show-Status }
    "next" { Show-NextSteps }
    default { 
        Write-ColoredOutput "Usage: .\code-quality-tracker.ps1 [-Action status|next]" "Yellow"
    }
}
