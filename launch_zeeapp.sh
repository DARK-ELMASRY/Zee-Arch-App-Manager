#!/bin/bash
# Wrapper to launch the published ZeeAppManager binary
# Adjust PUB_DIR if you moved the publish output
#!/bin/bash
# Wrapper to launch the published ZeeAppManager binary from any working directory.
# It resolves the script's directory and uses the publish folder relative to it.

set -e

# Resolve the directory this script lives in (works when symlinked)
SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
	DIR="$(cd -P "$(dirname "$SOURCE")" >/dev/null 2>&1 && pwd)"
	SOURCE="$(readlink "$SOURCE")"
	[[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE"
done
SCRIPT_DIR="$(cd -P "$(dirname "$SOURCE")" >/dev/null 2>&1 && pwd)"

# Publish directory relative to the script directory
PUB_DIR="$SCRIPT_DIR/bin/Release/net8.0/linux-x64/publish"
BINARY="$PUB_DIR/ZeeAppManager"

if [ ! -f "$BINARY" ]; then
	echo "Published binary not found at: $BINARY" >&2
	echo "Make sure you ran: dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true" >&2
	exit 2
fi

if [ ! -x "$BINARY" ]; then
	chmod +x "$BINARY" 2>/dev/null || true
fi

exec "$BINARY" "$@"
