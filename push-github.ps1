$ErrorActionPreference = "Stop"

$GitHubUsername = "FrederickCampbell"
$GitEmail = "FC@Frederickcampbell.com"
$RepoName = "magikarp-forms-checklist"
$BranchName = "main"

$Root = "C:\Users\fredd\Personal Projects\Scripts and Tools\Polished Crystal\Magikarp Finder"

Set-Location -LiteralPath $Root

Write-Host ""
Write-Host "Configuring Git identity..." -ForegroundColor Cyan

git config --global user.name $GitHubUsername
git config --global user.email $GitEmail

Write-Host ""
Write-Host "Checking Git repository..." -ForegroundColor Cyan

if (-not (Test-Path ".git")) {
    git init
}

git branch -M $BranchName

Write-Host ""
Write-Host "Making sure .gitignore is correct..." -ForegroundColor Cyan

$GitIgnore = @'
node_modules/
release/
dist/
*.log
npm-debug.log*
yarn-debug.log*
yarn-error.log*
.DS_Store
Thumbs.db
'@

Set-Content -Path ".gitignore" -Value $GitIgnore -Encoding UTF8

Write-Host ""
Write-Host "Adding files..." -ForegroundColor Cyan

git add .

Write-Host ""
Write-Host "Committing files..." -ForegroundColor Cyan

git diff --cached --quiet
$HasNoStagedChanges = $LASTEXITCODE -eq 0

git rev-parse --verify HEAD *> $null
$HasExistingCommit = $LASTEXITCODE -eq 0

if ($HasNoStagedChanges -and $HasExistingCommit) {
    Write-Host "Nothing new to commit." -ForegroundColor Yellow
}
else {
    git commit -m "Initial Magikarp forms checklist app"
}

Write-Host ""
Write-Host "Setting GitHub remote..." -ForegroundColor Cyan

$RemoteUrl = "https://github.com/$GitHubUsername/$RepoName.git"

git remote get-url origin *> $null

if ($LASTEXITCODE -eq 0) {
    git remote set-url origin $RemoteUrl
}
else {
    git remote add origin $RemoteUrl
}

Write-Host ""
Write-Host "Remote set to:" -ForegroundColor Green
git remote -v

Write-Host ""
Write-Host "Pushing to GitHub..." -ForegroundColor Cyan

git push -u origin $BranchName

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "Repo URL:" -ForegroundColor Green
Write-Host "https://github.com/$GitHubUsername/$RepoName"