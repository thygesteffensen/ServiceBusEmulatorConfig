# How to Make Your First Release with Release Please

Now that you've set up Release Please, you need to make a conventional commit to trigger the release process. Here's a step-by-step guide:

## Making a Conventional Commit

1. Make changes to your code as needed
2. Stage your changes: `git add .`
3. Commit with a conventional commit message

## Example Commit Messages

### For Core Package

```
feat(core): add connection timeout setting

Added configurable connection timeout option to improve reliability when connecting to slow service bus instances.
```

### For Web Package

```
fix(web): resolve rendering issue in topic view

Fixed a bug where topic subscriptions were not displaying correctly when there were more than 10 items.
```

### For CLI Package

```
feat(cli): add export to file command

Added a new command to export configuration directly to file without launching the web UI.
```

### Breaking Change Example

```
feat(core)!: redesign connection interface

BREAKING CHANGE: The IServiceBusConnection interface has been completely redesigned to support the new Azure SDK. Applications using the old interface will need to be updated.
```

## What Happens After Your Commit

1. When you push to `dev` branch:
   - Release Please will create a pull request with version updates
   - Versions will have the `-rc` suffix (e.g., `1.0.1-rc.1`)
   - Each subsequent commit will increment the RC number

2. When you push to `main` branch:
   - Release Please will create a pull request with version updates
   - Versions will be standard releases (e.g., `1.0.1`)
   - Once the PR is merged, GitHub releases and NuGet packages will be created

## Triggering the First Release

To trigger your initial release, simply make a commit with a conventional commit message and push it:

```bash
git add .
git commit -m "feat: initial release with Release Please"
git push origin dev  # For release candidate
# or
git push origin main  # For full release
```

Remember to follow the [Conventional Commits](./CONVENTIONAL_COMMITS.md) format for all future commits to ensure proper versioning!
