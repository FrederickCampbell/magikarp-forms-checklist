$ErrorActionPreference = "Stop"

npm install
if ($LASTEXITCODE -ne 0) {
    throw "npm install failed"
}

npm run build
if ($LASTEXITCODE -ne 0) {
    throw "npm run build failed"
}
