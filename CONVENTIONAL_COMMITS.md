# Conventional Commits Guide

This project uses [Release Please](https://github.com/googleapis/release-please) for version management and automated releases, which requires commit messages to follow the [Conventional Commits](https://www.conventionalcommits.org/) format.

## Commit Message Format

Each commit message consists of a **header**, a **body**, and a **footer**. The header has a special format that includes a **type**, an optional **scope**, and a **subject**:

```
<type>(<scope>): <subject>
<BLANK LINE>
<body>
<BLANK LINE>
<footer>
```

### Type

The type must be one of the following:

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that do not affect the meaning of the code (formatting, etc.)
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **perf**: A code change that improves performance
- **test**: Adding missing or correcting existing tests
- **chore**: Changes to the build process or auxiliary tools and libraries
- **ci**: Changes to CI configuration files and scripts

### Scope

The scope is optional and should specify the package or part of the codebase the commit affects (e.g., `core`, `web`, `cli`).

### Subject

The subject contains a succinct description of the change:

- Use the imperative, present tense: "change" not "changed" nor "changes"
- Don't capitalize the first letter
- No period (.) at the end

### Examples of Good Commit Messages

```
feat(web): add export to Excel functionality
fix(core): resolve connection timeout issue
docs: update README with installation instructions
chore: update dependency versions
refactor(cli): improve command handling logic
```

## Version Bump Rules

- **patch**: `fix`, `refactor` (bug fixes and implementation improvements)
- **minor**: `feat` (new features that don't break existing ones)
- **major**: commits with `BREAKING CHANGE` in the footer or a type with `!` (e.g., `feat!: replace api`)

## Branch Configuration

- Commits to `dev` branch produce release candidates (with an `rc` suffix)
- Commits to `main` branch produce full releases

By following these conventions, you help automate the versioning and release process!
