#!/usr/bin/env python3
"""
Screenshot Checker & Retype Build Script

Scans all Markdown files under docs/retype/ for <Screenshot: ...| path> tokens,
verifies that every referenced screenshot file exists, then optionally runs
the Retype build if all screenshots are present.

Usage:
    python scripts/check_screenshots.py              # Check only
    python scripts/check_screenshots.py --build       # Check + build if all OK
    python scripts/check_screenshots.py --build --serve  # Check + start dev server
    python scripts/check_screenshots.py --list        # List all expected screenshots
"""

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path

# ── Configuration ──────────────────────────────────────────────────────────
DOCS_DIR = Path("docs/retype")
SCREENSHOT_PATTERN = re.compile(r"<Screenshot:\s*(.*?)\|\s*(.+?)>")


# ── Helpers ────────────────────────────────────────────────────────────────

def find_screenshot_tokens(root: Path) -> list[dict]:
    """
    Walk all .md files under *root*, find every <Screenshot: ...| path> token,
    and return a list of dicts with file, line, description, and image_path.
    """
    tokens = []
    for md_file in sorted(root.rglob("*.md")):
        rel = md_file.relative_to(root)
        for lineno, line in enumerate(md_file.read_text(encoding="utf-8").splitlines(), start=1):
            for match in SCREENSHOT_PATTERN.finditer(line):
                tokens.append({
                    "source_file": str(rel),
                    "line": lineno,
                    "description": match.group(1).strip(),
                    "image_path": match.group(2).strip(),
                })
    return tokens


def check_screenshots(tokens: list[dict], project_root: Path) -> tuple[list[dict], list[dict]]:
    """
    Returns (found, missing) where each entry is a dict with token info.
    Screenshot paths in tokens are relative to the project root.
    """
    found = []
    missing = []
    for t in tokens:
        img = project_root / t["image_path"]
        if img.exists():
            found.append(t)
        else:
            missing.append(t)
    return found, missing


def print_report(found: list[dict], missing: list[dict]) -> None:
    """Print a human-readable report to stdout."""
    print("=" * 72)
    print(f"  Screenshot Check Report")
    print(f"  {len(found)} found  |  {len(missing)} missing")
    print("=" * 72)

    if found:
        print(f"\n  {'✓' * len(found)}  Present ({len(found)}):")
        for t in found:
            print(f"    ✓ {t['image_path']}")
            print(f"        from {t['source_file']}:{t['line']}")

    if missing:
        print(f"\n  {'✗' * len(missing)}  Missing ({len(missing)}):")
        for t in missing:
            print(f"    ✗ {t['image_path']}")
            print(f"        from {t['source_file']}:{t['line']}")
            print(f"        desc: {t['description']}")

    print("=" * 72)


def list_expected(tokens: list[dict]) -> None:
    """Print a machine-friendly list of all expected screenshot files."""
    seen = set()
    for t in tokens:
        if t["image_path"] not in seen:
            seen.add(t["image_path"])
            print(t["image_path"])


def run_retype_build(docs_dir: Path) -> int:
    """Run `retype build` and return the exit code."""
    print("\n  Running: retype build ...\n")
    result = subprocess.run(
        ["retype", "build"],
        cwd=docs_dir,
        capture_output=False,
        shell=True,
    )
    return result.returncode


def run_retype_serve(docs_dir: Path) -> int:
    """Run `retype start` (dev server) and return the exit code."""
    print("\n  Running: retype start ...\n")
    result = subprocess.run(
        ["retype", "start"],
        cwd=docs_dir,
        capture_output=False,
        shell=True,
    )
    return result.returncode


# ── Main ───────────────────────────────────────────────────────────────────

def main() -> int:
    parser = argparse.ArgumentParser(
        description="Check screenshot tokens and optionally build Retype docs.",
    )
    parser.add_argument(
        "--build", "-b",
        action="store_true",
        help="Run `retype build` if all screenshots are present.",
    )
    parser.add_argument(
        "--serve", "-s",
        action="store_true",
        help="Run `retype start` (dev server) if all screenshots are present.",
    )
    parser.add_argument(
        "--list", "-l",
        action="store_true",
        help="List all expected screenshot file paths and exit.",
    )
    parser.add_argument(
        "--root",
        type=str,
        default=None,
        help="Root directory for docs (default: parent of this script's location).",
    )
    args = parser.parse_args()

    # Resolve root
    if args.root:
        project_root = Path(args.root).resolve()
    else:
        project_root = Path.cwd()

    docs_dir = project_root / DOCS_DIR

    if not docs_dir.is_dir():
        print(f"Error: docs directory not found at {docs_dir}", file=sys.stderr)
        print("Run this script from the project root, or use --root.", file=sys.stderr)
        return 1

    # Find all screenshot tokens
    tokens = find_screenshot_tokens(docs_dir)

    if not tokens:
        print(f"No <Screenshot: ...> tokens found in {root}.")
        return 0

    if args.list:
        list_expected(tokens)
        return 0

    # Check which files exist
    found, missing = check_screenshots(tokens, project_root)
    print_report(found, missing)

    # Decide what to do next
    if missing:
        print("\n  ❌  Some screenshots are missing. Build blocked.")
        print("     Capture the missing screenshots and re-run.")
        return 1

    print("\n  ✅  All screenshots present.")

    if args.build:
        return run_retype_build(docs_dir)
    elif args.serve:
        return run_retype_serve(docs_dir)
    else:
        print("  (use --build or --serve to build/serve the docs)")
        return 0


if __name__ == "__main__":
    sys.exit(main())
