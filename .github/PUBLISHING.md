# Publishing to NuGet

SimpleRSS uses [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) from GitHub Actions. No long-lived API key is stored in the repository.

## One-time setup

### 1. NuGet.org — Trusted Publisher

Sign in at [nuget.org](https://www.nuget.org) and open **Account → Trusted Publishers → Add**.

| Field | Value |
|---|---|
| Provider | GitHub |
| Owner | `kearns2000` |
| Repository | `SimpleRSS` |
| Workflow file | `ci.yml` |

Save the policy. After the first successful OIDC login from CI, it becomes permanently active.

### 2. GitHub — Repository secret

Open [Settings → Secrets and variables → Actions](https://github.com/kearns2000/SimpleRSS/settings/secrets/actions) and add:

| Secret | Value |
|---|---|
| `NUGET_USER` | Your **nuget.org username** (profile name from your NuGet profile URL — not your email) |

You do **not** need `NUGET_API_KEY`.

## Releasing a new version

1. Bump `<Version>` in `src/SimpleRSS/SimpleRSS.csproj`
2. Update `CHANGELOG.md` (move `Unreleased` items into the new version section)
3. Commit and push to `main`
4. Create a [GitHub Release](https://github.com/kearns2000/SimpleRSS/releases/new):
   - Tag: `vX.Y.Z` (must match the csproj version exactly, e.g. `v2.1.1`)
   - Publish the release

The `publish` job in `.github/workflows/ci.yml` will build, test, pack, authenticate via OIDC, and push to nuget.org.

## Verify

- [Actions](https://github.com/kearns2000/SimpleRSS/actions) — `publish` job should succeed
- [nuget.org/packages/SimpleRSS](https://www.nuget.org/packages/SimpleRSS) — new version appears
