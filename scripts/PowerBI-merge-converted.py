import argparse
import json
import sys
from pathlib import Path
from typing import Any, Dict, List, Optional

'''
1. traverse ./converted/*.json
2. merge into 1 json in memory
3. output to SnafflerMerged.merged.json
'''

def load_json(path: Path) -> Optional[Dict[str, Any]]:
    """
    Load JSON from path. Returns dict on success, None on failure.
    """
    try:
        with path.open("r", encoding="utf-8") as f:
            return json.load(f)
    except json.JSONDecodeError as e:
        print(f"[WARN] Skipping invalid JSON {path} ({e})", file=sys.stderr)
        return None
    except OSError as e:
        print(f"[WARN] Skipping unreadable file: {path} ({e})", file=sys.stderr)
        return None


def extract_entries(doc: Dict[str, Any], path: Path, strict: bool) -> List[Dict[str, Any]]:
    """
    Extract entries list from a parsed JSON object.
    If strict=True, abort on schema mismatch.
    Otherwise warn and skip bad files.
    """
    if not isinstance(doc, dict):
        msg = f"Top-level JSON is not an object in {path}"
        if strict:
            raise ValueError(msg)
        print(f"[WARN] {msg}; skipping", file=sys.stderr)
        return []

    if "entries" not in doc:
        msg = f"Missing 'entries' key in {path}"
        if strict:
            raise ValueError(msg)
        print(f"[WARN] {msg}; skipping", file=sys.stderr)
        return []

    entries = doc["entries"]
    if not isinstance(entries, list):
        msg = f"'entries' is not a list in {path}"
        if strict:
            raise ValueError(msg)
        print(f"[WARN] {msg}; skipping", file=sys.stderr)
        return []

    # Optional: ensure each entry is a dict; if not, keep it but warn/skip depending on strict
    out: List[Dict[str, Any]] = []
    for i, item in enumerate(entries):
        if isinstance(item, dict):
            out.append(item)
        else:
            msg = f"entries[{i}] is not an object in {path}"
            if strict:
                raise ValueError(msg)
            print(f"[WARN] {msg}; skipping item", file=sys.stderr)

    return out


def merge_entries(input_dir: Path, pattern: str, strict: bool) -> Dict[str, Any]:
    """
    Traverse input_dir/pattern and merge all entries into {"entries": [...]}
    """
    files = sorted(input_dir.glob(pattern))
    if not files:
        print(f"[WARN] No files matched: {input_dir / pattern}", file=sys.stderr)

    merged: List[Dict[str, Any]] = []

    for p in files:
        doc = load_json(p)
        if doc is None:
            continue
        merged.extend(extract_entries(doc, p, strict=strict))

    return {"entries": merged}


def write_output(output_path: Path, data: Dict[str, Any], pretty: bool) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as f:
        if pretty:
            json.dump(data, f, indent=2, ensure_ascii=False)
        else:
            json.dump(data, f, separators=(",", ":"), ensure_ascii=False)
        f.write("\n")


def parse_args(argv: List[str]) -> argparse.Namespace:
    ap = argparse.ArgumentParser(
        description="Merge Snaffler converted JSON files by concatenating all objects under the `entries` key."
    )
    ap.add_argument(
        "--input-dir",
        default="./converted",
        help="Directory containing converted JSON files (default: ./converted)",
    )
    ap.add_argument(
        "--pattern",
        default="*.json",
        help="Glob pattern within input-dir (default: *.json)",
    )
    ap.add_argument(
        "--output",
        default="SnafflerMerged.merged.json",
        help="Output file path (default: SnafflerMerged.merged.json)",
    )
    ap.add_argument(
        "--pretty",
        action="store_true",
        help="Pretty-print the output JSON (indent=2)",
    )
    ap.add_argument(
        "--strict",
        action="store_true",
        help="Fail fast if any file is missing/invalid schema instead of skipping.",
    )
    return ap.parse_args(argv)


def main(argv: List[str]) -> int:
    args = parse_args(argv)

    input_dir = Path(args.input_dir).expanduser().resolve()
    output_path = Path(args.output).expanduser().resolve()

    if not input_dir.exists() or not input_dir.is_dir():
        print(f"[ERROR] input-dir does not exist or is not a directory: {input_dir}", file=sys.stderr)
        return 2

    try:
        merged_doc = merge_entries(input_dir=input_dir, pattern=args.pattern, strict=args.strict)
        write_output(output_path=output_path, data=merged_doc, pretty=args.pretty)
    except Exception as e:
        print(f"[ERROR] {e}", file=sys.stderr)
        return 1

    print(f"[OK] Wrote {len(merged_doc['entries'])} merged entries to {output_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
