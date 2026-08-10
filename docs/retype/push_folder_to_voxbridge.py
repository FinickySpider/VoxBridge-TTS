#!/usr/bin/env python3
"""Push a folder's contents to the root of FinickySpider/VoxBridge-TTS."""

from __future__ import annotations

import argparse
import os
import shutil
import subprocess
import sys
from pathlib import Path


DEFAULT_REPO_URL = "https://github.com/FinickySpider/VoxBridge-TTS.git"
DEFAULT_BRANCH = "main"
DEFAULT_COMMIT_MESSAGE = "Push folder contents to repository root"
DEFAULT_CACHE_DIR = Path.home() / ".voxbridge-push-cache"
EXPECTED_REPO_SLUG = "FinickySpider/VoxBridge-TTS"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Push folder contents to VoxBridge-TTS root."
    )
    parser.add_argument("source_folder", help="Folder whose contents should be pushed.")
    parser.add_argument(
        "-m",
        "--message",
        default=os.environ.get("COMMIT_MESSAGE", DEFAULT_COMMIT_MESSAGE),
        help="Commit message.",
    )
    parser.add_argument(
        "--repo",
        default=os.environ.get("REPO_URL", DEFAULT_REPO_URL),
        help=f"Repository URL. Default: {DEFAULT_REPO_URL}",
    )
    parser.add_argument(
        "--branch",
        default=os.environ.get("BRANCH", DEFAULT_BRANCH),
        help=f"Target branch. Default: {DEFAULT_BRANCH}",
    )
    parser.add_argument(
        "--prune",
        action="store_true",
        help="Delete root files not in source folder.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show changes without pushing.",
    )
    parser.add_argument(
        "--cache-dir",
        default=str(DEFAULT_CACHE_DIR),
        help=f"Local repo cache. Default: {DEFAULT_CACHE_DIR}",
    )
    return parser.parse_args()


def fail(message: str) -> None:
    print(f"error: {message}", file=sys.stderr)
    raise SystemExit(1)


def require_command(command: str) -> None:
    if shutil.which(command) is None:
        fail(f"missing required command: {command}")


def run(command: list[str], cwd: Path | None = None) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        command,
        cwd=cwd,
        check=True,
        text=True,
        stdout=None,
        stderr=None,
    )


def capture(command: list[str], cwd: Path | None = None) -> str:
    result = subprocess.run(
        command,
        cwd=cwd,
        check=True,
        text=True,
        stdout=subprocess.PIPE,
        stderr=None,
    )
    return result.stdout


def normalize_repo_url(url: str) -> str:
    normalized = url.strip().lower()
    normalized = normalized.removesuffix(".git")
    normalized = normalized.replace(":", "/")
    return normalized


def repo_url_matches_expected(url: str) -> bool:
    return EXPECTED_REPO_SLUG.lower() in normalize_repo_url(url)


def validate_source_folder(source_folder: str) -> Path:
    source = Path(source_folder).expanduser().resolve()
    if not source.is_dir():
        fail(f"source folder does not exist: {source}")
    if source == Path("/"):
        fail("refusing to use / as the source folder")
    return source


def validate_repo_url(repo_url: str) -> None:
    if not repo_url_matches_expected(repo_url):
        fail(
            "repo URL does not look like the expected VoxBridge-TTS repo: "
            f"{repo_url}"
        )


def validate_existing_cache(repo_dir: Path) -> None:
    if not repo_dir.exists():
        return

    if not (repo_dir / ".git").is_dir():
        fail(f"cache dir exists but is not a git repo: {repo_dir}")

    remote_url = capture(["git", "remote", "get-url", "origin"], cwd=repo_dir).strip()
    if not repo_url_matches_expected(remote_url):
        fail(f"cache repo origin does not look like VoxBridge-TTS: {remote_url}")


def copy_folder_contents(source: Path, destination: Path, prune: bool) -> None:
    if prune:
        for child in destination.iterdir():
            if child.name == ".git":
                continue
            remove_path(child)

    for child in source.iterdir():
        if child.name == ".git":
            continue

        target = destination / child.name
        if target.exists() or target.is_symlink():
            remove_path(target)

        if child.is_symlink():
            target.symlink_to(os.readlink(child))
        elif child.is_dir():
            shutil.copytree(child, target, symlinks=True)
        else:
            shutil.copy2(child, target, follow_symlinks=False)


def remove_path(path: Path) -> None:
    if path.is_symlink() or path.is_file():
        path.unlink()
    elif path.is_dir():
        shutil.rmtree(path)


def has_changes(repo_dir: Path) -> bool:
    status = capture(["git", "status", "--porcelain"], cwd=repo_dir)
    print(status, end="")
    return bool(status.strip())


def prepare_repo(repo_dir: Path, repo_url: str, branch: str) -> None:
    validate_repo_url(repo_url)
    validate_existing_cache(repo_dir)

    if not repo_dir.exists():
        print(f"Cloning {repo_url} branch {branch}...")
        repo_dir.parent.mkdir(parents=True, exist_ok=True)
        run(["git", "clone", "--depth", "1", "--branch", branch, repo_url, str(repo_dir)])
        return

    print(f"Using cached repo at {repo_dir}")
    run(["git", "fetch", "origin", branch], cwd=repo_dir)
    run(["git", "reset", "--hard", f"origin/{branch}"], cwd=repo_dir)


def main() -> int:
    args = parse_args()

    require_command("git")

    source = validate_source_folder(args.source_folder)
    repo_dir = Path(args.cache_dir).expanduser().resolve()

    prepare_repo(repo_dir, args.repo, args.branch)

    print(f"Copying contents from {source} to repository root...")
    copy_folder_contents(source, repo_dir, args.prune)

    print("\nPending repository changes:")
    if not has_changes(repo_dir):
        print("No changes to commit.")
        return 0

    if args.dry_run:
        print("\nDry run enabled; not committing or pushing.")
        return 0

    run(["git", "add", "-A"], cwd=repo_dir)
    run(["git", "commit", "-m", args.message], cwd=repo_dir)

    print(f"Pushing to {args.branch}...")
    run(["git", "push", "origin", f"HEAD:{args.branch}"], cwd=repo_dir)

    print("Done.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
