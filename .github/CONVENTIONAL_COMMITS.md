# Conventional Commits Guide

This project uses [Conventional Commits](https://www.conventionalcommits.org/) to automate versioning and changelog generation.

## Commit Message Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

## Examples

### Adding a new feature (minor bump: 1.0.0 → 1.1.0)
```bash
git commit -m "feat: add new slide transition effect"
git commit -m "feat(transitions): add zoom transition"
```

### Fixing a bug (patch bump: 1.0.0 → 1.0.1)
```bash
git commit -m "fix: correct image loading race condition"
git commit -m "fix(api): handle missing configuration gracefully"
```

### Breaking changes (major bump: 1.0.0 → 2.0.0)
```bash
git commit -m "feat!: redesign transition API"

# Or with footer
git commit -m "feat: redesign transition API

BREAKING CHANGE: transition functions now require onComplete callback"
```

### Non-versioning commits
```bash
git commit -m "docs: update README with new features"
git commit -m "style: fix code formatting"
git commit -m "test: add unit tests for ImageService"
```

## Commit Types

| Type | Description | Version Bump | Appears in Changelog |
|------|-------------|--------------|---------------------|
| `feat` | New feature | Minor (1.x.0) | Yes |
| `fix` | Bug fix | Patch (1.0.x) | Yes |
| `perf` | Performance improvement | Patch (1.0.x) | Yes |
| `chore` | Maintenance, dependencies | Patch (1.0.x) | No |
| `docs` | Documentation only | None | No |
| `style` | Code style/formatting | None | No |
| `refactor` | Code refactoring | None | No |
| `test` | Adding/updating tests | None | No |
| `build` | Build system changes | None | No |
| `ci` | CI/CD changes | None | No |

## Scopes (Optional)

Scopes provide context about what part of the codebase is affected:

```bash
feat(transitions): add pixelate effect
fix(api): handle null configuration
docs(readme): update installation instructions
chore(deps): update Serilog to 8.0.2
```

## Breaking Changes

Mark breaking changes with `!` or `BREAKING CHANGE:` footer:

```bash
# Method 1: Use ! after type/scope
feat!: remove legacy transition API
feat(api)!: change configuration format

# Method 2: Use BREAKING CHANGE footer
feat: redesign transition system

BREAKING CHANGE: Transitions now use async/await pattern instead of callbacks.
Migration guide: update all transition functions to return Promises.
```

## Multi-line Commits

For complex changes, use body and footer:

```bash
git commit -m "feat: add configurable transition duration

This adds a new Duration configuration option for all transitions.
Each transition can now specify its own duration in seconds.

Closes #123
Refs #456"
```

## Tips

1. **Use imperative mood**: "add feature" not "added feature" or "adds feature"
2. **Don't capitalize first letter**: "feat: add" not "feat: Add"
3. **No period at the end**: "feat: add transition" not "feat: add transition."
4. **Keep description under 72 characters**
5. **Use body for detailed explanation** if needed

## Running Versionize

Once you've committed following conventional commits:

```powershell
# Analyze what version bump would happen
versionize --dry-run

# Actually bump version and update changelog
versionize

# Or use the publish script (recommended)
.\publish.ps1
```

## Changelog Generation

Versionize automatically generates/updates `CHANGELOG.md` based on your commits:

```markdown
## [1.2.0] - 2025-12-05

### Features
- add new pixelate transition
- add port configuration support

### Bug Fixes
- correct image loading race condition
- handle missing configuration gracefully
```

## Useful Links

- [Conventional Commits Specification](https://www.conventionalcommits.org/)
- [Versionize Documentation](https://github.com/versionize/versionize)
- [Semantic Versioning](https://semver.org/)
