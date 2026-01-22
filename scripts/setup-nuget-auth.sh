#!/bin/bash

# Ensure gh cli is installed
if ! command -v gh &> /dev/null
then
    echo "❌ GitHub CLI (gh) could not be found. Please install it first."
    exit 1
fi

# Ensure user is logged in and has read:packages scope
if ! gh auth status --scope read:packages &> /dev/null
then
    echo "⚠️  Missing read:packages scope. Refreshing..."
    gh auth refresh -s read:packages
fi

# Get values from gh cli
GITHUB_USERNAME=$(gh api user -q .login)
GITHUB_PACKAGES_TOKEN=$(gh auth token)

# Export variables
export GITHUB_USERNAME
export GITHUB_PACKAGES_TOKEN

echo "✅ Environment variables set for this session:"
echo "   GITHUB_USERNAME: $GITHUB_USERNAME"
echo "   GITHUB_PACKAGES_TOKEN: (retrieved from gh auth token)"
echo ""
echo "💡 To make these persistent, add the following to your ~/.zshrc or ~/.bashrc:"
echo ""
echo "export GITHUB_USERNAME=\"$GITHUB_USERNAME\""
echo "export GITHUB_PACKAGES_TOKEN=\$(gh auth token)"
