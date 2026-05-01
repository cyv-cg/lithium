#!/usr/bin/bash

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Clear out old results.
if [ -d "$ROOT/coverage/" ]; then
	rm -r "$ROOT/coverage/"
fi
for dir in $ROOT/tests/*; do
	if [ -d "$dir/TestResults" ]; then
		rm -r "$dir/TestResults"
	fi
done

# Run tests and generate the coverage report.
dotnet test "$ROOT/Lithium.sln" --settings "$ROOT/tests.runsettings"

# Compile the report into a readable HTML page.
reportgenerator \
    -reports:"$ROOT/tests/**/TestResults/**/coverage.cobertura.xml" \
    -targetdir:"$ROOT/coverage" \
    -reporttypes:Html
