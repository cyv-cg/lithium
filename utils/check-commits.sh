#!/usr/bin/bash

set -euo pipefail

git fetch origin main develop

# Fetch all the commits that are at HEAD but not in develop.
MISSING_COMMITS=$(git cherry origin/develop HEAD | grep '^+' | cut -d' ' -f2 || true)

if [[ -n "$MISSING_COMMITS" ]]; then
	echo "The following commits are missing from develop:"
	# Write them out all pretty :)
	for c in $MISSING_COMMITS; do
		git log --oneline -1 "$c"
	done
	exit 1
fi
