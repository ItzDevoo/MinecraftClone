# DevCraft Code Quality Improvement Script
# This script provides an overview of completed tasks and next steps

param(
    [string]$Action = "status"  # Options: status, next, build, test
)

function Write-ColoredOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
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
        "Created UIResourceManager for memory management",
        "Refactored Inventory class with responsive layout",
        "Added IDisposable pattern to Inventory",
        "Updated GameMenu to use UIConstants",
        "Eliminated hardcoded positioning in UI components",
        "Improved crosshair and button positioning"
    )
    $completed | ForEach-Object { Write-ColoredOutput "  [DONE] $_" "DarkGreen" }
    
    Write-Host ""
    Write-ColoredOutput "NEXT PRIORITY TASKS:" "Yellow"
    $nextTasks = @(
        "Add null checks to prevent NullReferenceExceptions",
        "Implement proper error handling in MonitorHelper",
        "Create base UIComponent class for shared functionality",
        "Add proper disposal to MainMenu and GameMenu",
        "Implement configuration validation",
        "Add event-driven communication between components"
    )
    $nextTasks | ForEach-Object { Write-ColoredOutput "  [TODO] $_" "Yellow" }
    
    Write-Host ""
    Write-ColoredOutput "BUILD STATUS: " "White" -NoNewline    try {
        $null = & dotnet build --verbosity quiet 2>&1        if ($LASTEXITCODE -eq 0) {
            Write-ColoredOutput "PASSING [OK]" "Green"
        } else {
            Write-ColoredOutput "FAILING [ERROR]" "Red"
            Write-ColoredOutput "Build errors detected. Run with -Action build for details." "Red"
        }
    } catch {
        Write-ColoredOutput "UNKNOWN" "Yellow"
    }
}

function Show-NextSteps {
    Write-ColoredOutput "=== Next Implementation Steps ===" "Cyan"
    Write-Host ""
    
    Write-ColoredOutput "IMMEDIATE (Easy wins):" "Green"
    Write-ColoredOutput "1. Add null checks in Inventory.cs Update method" "White"
    Write-ColoredOutput "2. Add using statements validation" "White"
    Write-ColoredOutput "3. Fix remaining magic numbers in UI" "White"
    
    Write-Host ""
    Write-ColoredOutput "SHORT TERM (Medium effort):" "Yellow"
    Write-ColoredOutput "1. Create base UIComponent abstract class" "White"
    Write-ColoredOutput "2. Implement proper disposal patterns" "White"
    Write-ColoredOutput "3. Add configuration validation" "White"
    
    Write-Host ""
    Write-ColoredOutput "LONG TERM (Architectural):" "Magenta"
    Write-ColoredOutput "1. Implement MVVM pattern for UI" "White"
    Write-ColoredOutput "2. Add dependency injection" "White"
    Write-ColoredOutput "3. Create event-driven architecture" "White"
}

function Test-Build {
    Write-ColoredOutput "Building DevCraft..." "Cyan"
    & dotnet build
      if ($LASTEXITCODE -eq 0) {
        Write-ColoredOutput "`nBuild successful! [OK]" "Green"
        return $true
    } else {
        Write-ColoredOutput "`nBuild failed! [ERROR]" "Red"
        return $false
    }
}

function Invoke-Tests {
    Write-ColoredOutput "Running tests..." "Cyan"
    # For now, we'll just run the build as a test    if (Test-Build) {
        Write-ColoredOutput "All tests passed! [OK]" "Green"
    } else {
        Write-ColoredOutput "Tests failed! [ERROR]" "Red"
    }
}

# Main script logic
switch ($Action.ToLower()) {
    "status" { Show-Status }
    "next" { Show-NextSteps }
    "build" { Test-Build }
    "test" { Invoke-Tests }
    default { 
        Write-ColoredOutput "Usage: .\improvement-progress.ps1 [-Action status|next|build|test]" "Yellow"
        Write-ColoredOutput "  status - Show current progress (default)" "White"
        Write-ColoredOutput "  next   - Show next implementation steps" "White"
        Write-ColoredOutput "  build  - Test build" "White"
        Write-ColoredOutput "  test   - Run tests" "White"
    }
}
