#!/usr/bin/env python3
"""
Find all screenshot tokens in documentation
"""

import re
from pathlib import Path

DOCS_DIR = Path("docs/retype")
SCREENSHOT_PATTERN = re.compile(r"<Screenshot:\s*(.*?)\|\s*(.+?)>")

tokens = []
for md_file in sorted(DOCS_DIR.rglob("*.md")):
    rel = md_file.relative_to(DOCS_DIR)
    for lineno, line in enumerate(md_file.read_text(encoding="utf-8").splitlines(), start=1):
        for match in SCREENSHOT_PATTERN.finditer(line):
            tokens.append({
                "source_file": str(rel),
                "line": lineno,
                "description": match.group(1).strip(),
                "image_path": match.group(2).strip(),
            })

print(f"Found {len(tokens)} screenshot tokens:")
print("=" * 80)
for token in tokens:
    print(f"{token['source_file']}:{token['line']}")
    print(f"  Description: {token['description']}")
    print(f"  Image path: {token['image_path']}")
    print()