from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Dict, List


def load_json(path: Path) -> Any:
    """Load JSON from a file."""
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: Path, payload: Any) -> None:
    """Write JSON to a file (pretty printed)."""
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def normalize_entry(
    entry: Dict[str, Any],
    *,
    keep_raw_event_properties: bool,
    drop_raw_event_properties_field: bool,
) -> Dict[str, Any]:
    """
    Flatten eventProperties.* color buckets into a common event object.

    Example:
      {"eventProperties": {"Green": {"DateTime": "...", ...}}}
    becomes:
      {"event": {"severity": "Green", "DateTime": "...", ...}}

    Also optionally drops:
      entries[].rawEventProperties
    """
    event_props = entry.get("eventProperties") or {}

    # Take the first severity bucket.
    severity, payload = (next(iter(event_props.items())) if event_props else (None, None))

    # Copy base fields to avoid mutating the original dict.
    excluded_keys = {"eventProperties"}
    if drop_raw_event_properties_field:
        excluded_keys.add("rawEventProperties")

    normalized: Dict[str, Any] = {k: v for k, v in entry.items() if k not in excluded_keys}

    if severity:
        normalized["event"] = {"severity": severity, **(payload or {})}
        if keep_raw_event_properties:
            # Keep original buckets for traceability if requested.
            normalized["rawEventProperties"] = event_props

    return normalized


def transform_document(
    document: Any,
    *,
    keep_raw_event_properties: bool,
    drop_raw_event_properties_field: bool,
) -> Any:
    """
    Apply normalization to every entry if this looks like snaffler output.
    If document isn't the expected shape, return it unchanged.
    """
    if not isinstance(document, dict):
        return document

    entries = document.get("entries")
    if not isinstance(entries, list):
        # Not a snaffler output (or not the format we expect); return unchanged.
        return document

    transformed = [
        normalize_entry(
            entry,
            keep_raw_event_properties=keep_raw_event_properties,
            drop_raw_event_properties_field=drop_raw_event_properties_field,
        )
        if isinstance(entry, dict)
        else entry
        for entry in entries
    ]

    # Matches your original: only output {"entries": ...}
    return {"entries": transformed}


def iter_input_files(input_dir: Path, pattern: str, recursive: bool) -> List[Path]:
    if recursive:
        return sorted(p for p in input_dir.rglob(pattern) if p.is_file())
    return sorted(p for p in input_dir.glob(pattern) if p.is_file())


def make_output_path(input_file: Path, output_dir: Path, suffix: str) -> Path:
    return output_dir / f"{input_file.stem}{suffix}"


def parse_args(argv: List[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Convert Snaffler JSON output(s) into a PowerBI-friendly nested format."
    )
    parser.add_argument(
        "input_dir",
        nargs="?",
        default=".",
        help="Folder containing .json files to convert (default: current directory).",
    )
    parser.add_argument(
        "-o",
        "--output-dir",
        default="converted",
        help='Output folder (default: "converted").',
    )
    parser.add_argument(
        "--pattern",
        default="*.json",
        help='Glob pattern for input files (default: "*.json").',
    )
    parser.add_argument(
        "-r",
        "--recursive",
        action="store_true",
        help="Search for input files recursively.",
    )
    parser.add_argument(
        "--suffix",
        default=".snafflerconverted.json",
        help='Suffix appended to each converted filename (default: ".snafflerconverted.json").',
    )

    # Default behavior: drop entries[].rawEventProperties (PowerBI-friendly)
    group = parser.add_mutually_exclusive_group()
    group.add_argument(
        "--keep-raw-event-properties",
        action="store_true",
        help="Keep entries[].rawEventProperties in output (not recommended for PowerBI import).",
    )
    group.add_argument(
        "--drop-raw-event-properties",
        action="store_true",
        help="Explicitly drop entries[].rawEventProperties in output (default behavior).",
    )

    return parser.parse_args(argv)


def main(argv: List[str]) -> int:
    args = parse_args(argv)

    input_dir = Path(args.input_dir).expanduser().resolve()
    output_dir = Path(args.output_dir).expanduser().resolve()

    if not input_dir.exists() or not input_dir.is_dir():
        print(f"[!] input_dir is not a directory: {input_dir}", file=sys.stderr)
        return 2

    files = iter_input_files(input_dir, args.pattern, args.recursive)
    if not files:
        print(f"[!] No files matched pattern '{args.pattern}' in {input_dir}", file=sys.stderr)
        return 1

    # Default drop unless explicitly kept.
    keep_raw = bool(args.keep_raw_event_properties)
    drop_raw_field = not keep_raw

    converted_count = 0
    skipped_count = 0

    for src in files:
        # Avoid re-processing already converted outputs if user points input_dir at converted/
        if src.name.endswith(args.suffix):
            skipped_count += 1
            continue

        try:
            raw = load_json(src)
        except json.JSONDecodeError as e:
            print(f"[!] Skipping invalid JSON: {src} ({e})", file=sys.stderr)
            skipped_count += 1
            continue
        except Exception as e:
            print(f"[!] Skipping unreadable file: {src} ({e})", file=sys.stderr)
            skipped_count += 1
            continue

        transformed = transform_document(
            raw,
            keep_raw_event_properties=keep_raw,
            drop_raw_event_properties_field=drop_raw_field,
        )

        out_path = make_output_path(src, output_dir, args.suffix)
        write_json(out_path, transformed)

        # Lightweight validation parity check for snaffler-shaped docs.
        if isinstance(raw, dict) and isinstance(raw.get("entries"), list):
            raw_count = len(raw.get("entries", []))
            new_entries = transformed.get("entries") if isinstance(transformed, dict) else None
            new_count = len(new_entries) if isinstance(new_entries, list) else None
            if new_count is None or raw_count != new_count:
                raise AssertionError(
                    f"entry count changed during transform for {src.name}: {raw_count} -> {new_count}"
                )

        # Confirm output is valid JSON by re-loading (cheap sanity check).
        _ = load_json(out_path)

        converted_count += 1

    print(f"[+] Converted: {converted_count}")
    print(f"[~] Skipped:   {skipped_count}")
    print(f"[+] Output dir: {output_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
