# Sample Conventional Commit Messages

To trigger version bumps and releases with Release Please, you need to use conventional commit messages. Here are a few examples to get started:

## Initial Release

```
feat: initial release

This is the first release of ServiceBusEmulatorConfig with GitVersion to Release Please migration.
```

## Features (Minor Version Bump)

```
feat(core): add support for filtering queues

Added capability to filter queues by name pattern for more targeted configuration.
```

## Bug Fixes (Patch Version Bump)

```
fix(web): resolve connection timeout issue

Fixed a bug where the web interface would timeout when connecting to a service bus with many entities.
```

## Breaking Changes (Major Version Bump)

```
feat!: redesign CLI interface

BREAKING CHANGE: All CLI commands have been reorganized. The `transform` command is now `convert`.
```

## Multiple Scopes

```
feat(core,web): add export to CSV functionality

Implemented CSV export in both the core library and web interface.
```

## Component-Specific Commits

For targeting specific packages within a monorepo:

```
feat(ServiceBusEmulatorConfig.Cli): add new parameter for connection timeout
```

Remember, Release Please uses these commit messages to:
1. Determine which version number to bump
2. Generate release notes
3. Create GitHub releases
4. Update package versions
