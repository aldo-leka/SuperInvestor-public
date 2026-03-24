#!/usr/bin/env bash
set -euo pipefail

REPO_DIR="/root/SuperInvestor"
IMAGE_TAG="${1:-latest}"

if [[ "$IMAGE_TAG" != "latest" && "$IMAGE_TAG" != sha-* ]]; then
  IMAGE_TAG="sha-$IMAGE_TAG"
fi

cd "$REPO_DIR"
git pull --ff-only
IMAGE_TAG="$IMAGE_TAG" docker compose pull app
IMAGE_TAG="$IMAGE_TAG" docker compose up -d app
