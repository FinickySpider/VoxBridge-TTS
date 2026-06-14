#!/usr/bin/env python3
"""
Fix image paths in documentation
"""

import re
from pathlib import Path

DOCS_DIR = Path("docs/retype")

# Pattern to match markdown images with placeholder descriptions
IMAGE_PATTERN = re.compile(r'!\[\.\.\.\]\((screenshots/.+?)\)')

def fix_file(file_path: Path):
    """Fix image paths in a single file"""
    content = file_path.read_text(encoding="utf-8")
    
    # Determine the relative path to screenshots directory
    rel_to_docs = file_path.relative_to(DOCS_DIR)
    if rel_to_docs.parent == Path("."):
        # File is at root (index.md)
        prefix = "screenshots/"
    else:
        # File is in subdirectory
        prefix = "../screenshots/"
    
    # Replace image paths
    new_content = IMAGE_PATTERN.sub(lambda m: f'![...]({prefix}{m.group(1).split("/")[-1]})', content)
    
    if content != new_content:
        file_path.write_text(new_content, encoding="utf-8")
        return True
    return False

def main():
    """Fix all markdown files"""
    fixed_count = 0
    
    for md_file in DOCS_DIR.rglob("*.md"):
        # Skip .retype directory (generated output)
        if ".retype" in str(md_file):
            continue
            
        if fix_file(md_file):
            print(f"Fixed: {md_file.relative_to(DOCS_DIR)}")
            fixed_count += 1
    
    print(f"\nFixed {fixed_count} files")

if __name__ == "__main__":
    main()