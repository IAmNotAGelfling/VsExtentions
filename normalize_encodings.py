#!/usr/bin/env python3
"""
Normalize text file encodings to UTF-8 with BOM and CRLF line endings.
Processes .cs, .csproj, .slnx, .md, .xml, .json, .txt, and other common text files.
"""

import os
import sys
from pathlib import Path
from typing import Set

# Enable UTF-8 console output on Windows
if sys.platform == 'win32':
    try:
        sys.stdout.reconfigure(encoding='utf-8')
    except AttributeError:
        import codecs
        sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

# File extensions to process
TEXT_EXTENSIONS: Set[str] = {
    '.cs', '.csproj', '.slnx', '.sln', '.props', '.targets',
    '.md', '.txt', '.xml', '.json', '.config', '.resx',
    '.xaml', '.cshtml', '.razor', '.yml', '.yaml',
    '.gitignore', '.editorconfig', '.gitattributes'
}

# Directories to skip
SKIP_DIRS: Set[str] = {
    '.git', '.vs', 'bin', 'obj', 'node_modules',
    'packages', '.vscode', '.idea'
}


def should_process_file(file_path: Path) -> bool:
    """Check if file should be processed based on extension."""
    if file_path.suffix.lower() in TEXT_EXTENSIONS:
        return True
    if file_path.name.lower() in {'.gitignore', '.gitattributes', '.editorconfig'}:
        return True
    return False


def normalize_file(file_path: Path) -> bool:
    """
    Convert file to UTF-8 with BOM and CRLF line endings.
    Returns True if file was modified, False otherwise.
    """
    try:
        # Try reading with various encodings
        content: str | None = None
        original_encoding: str = 'unknown'

        for encoding in ['utf-8-sig', 'utf-8', 'utf-16', 'cp1252', 'latin-1']:
            try:
                with open(file_path, 'r', encoding=encoding, newline='') as f:
                    content = f.read()
                    original_encoding = encoding
                    break
            except (UnicodeDecodeError, LookupError):
                continue

        if content is None:
            print(f"❌ Could not decode: {file_path}")
            return False

        # Normalize line endings to CRLF
        content = content.replace('\r\n', '\n').replace('\r', '\n').replace('\n', '\r\n')

        # Check if file already has correct encoding and line endings
        needs_update: bool = False
        if original_encoding != 'utf-8-sig':
            needs_update = True
        else:
            # Check if line endings are already CRLF
            with open(file_path, 'rb') as f:
                raw_content: bytes = f.read()
                if b'\r\n' not in raw_content and b'\n' in raw_content:
                    needs_update = True

        if needs_update:
            # Write back with UTF-8 BOM and CRLF
            with open(file_path, 'w', encoding='utf-8-sig', newline='') as f:
                f.write(content)
            print(f"✅ Normalized: {file_path}")
            return True
        else:
            print(f"⏭️  Already correct: {file_path}")
            return False

    except Exception as e:
        print(f"❌ Error processing {file_path}: {e}")
        return False


def process_directory(root_dir: Path) -> tuple[int, int]:
    """
    Process all text files in directory recursively.
    Returns tuple of (files_processed, files_modified).
    """
    files_processed: int = 0
    files_modified: int = 0

    for item in root_dir.rglob('*'):
        # Skip directories in SKIP_DIRS
        if any(skip_dir in item.parts for skip_dir in SKIP_DIRS):
            continue

        if item.is_file() and should_process_file(item):
            files_processed += 1
            if normalize_file(item):
                files_modified += 1

    return files_processed, files_modified


def main() -> int:
    """Main entry point."""
    script_dir: Path = Path(__file__).parent
    print(f"Processing files in: {script_dir}")
    print(f"Target encoding: UTF-8 with BOM")
    print(f"Target line endings: CRLF (Windows)")
    print("-" * 60)

    files_processed, files_modified = process_directory(script_dir)

    print("-" * 60)
    print(f"📊 Summary:")
    print(f"   Files processed: {files_processed}")
    print(f"   Files modified: {files_modified}")
    print(f"   Files unchanged: {files_processed - files_modified}")

    return 0


if __name__ == '__main__':
    sys.exit(main())
