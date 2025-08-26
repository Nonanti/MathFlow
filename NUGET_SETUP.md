# NuGet Setup Instructions

To enable automatic NuGet package publishing, you need to set up a GitHub secret with your NuGet API key.

## Steps:

1. **Get your NuGet API Key:**
   - Go to https://www.nuget.org/account/apikeys
   - Sign in with your account
   - Click "Create" to create a new API key
   - Name it (e.g., "MathFlow GitHub Actions")
   - Select "Push" scope
   - Select "MathFlow" package or use glob pattern "MathFlow*"
   - Copy the generated key

2. **Add the API Key to GitHub Secrets:**
   - Go to your repository: https://github.com/Nonanti/MathFlow
   - Click on "Settings" tab
   - Go to "Secrets and variables" → "Actions"
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: [Paste your NuGet API key]
   - Click "Add secret"

3. **How to publish a new version:**
   - Update the version in `MathFlow.Core/MathFlow.Core.csproj`
   - Update CHANGELOG.md with the new version details
   - Commit and push your changes
   - Create a new tag: `git tag v1.0.1` (replace with your version)
   - Push the tag: `git push origin v1.0.1`
   - The GitHub Action will automatically build, test, and publish to NuGet

## Manual Publishing (if needed):

```bash
# Pack the project
dotnet pack MathFlow.Core/MathFlow.Core.csproj -c Release

# Push to NuGet
dotnet nuget push MathFlow.Core/bin/Release/MathFlow.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Testing the workflow:

You can test the build and test parts of the workflow by pushing to master.
The NuGet publishing only happens when you push a version tag (v*).