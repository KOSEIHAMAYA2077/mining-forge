"""Create and verify the shelved prototype ZIP from a clean repository."""
from pathlib import Path
import hashlib
import json
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]
BUILDS = ['ForgeAssets-archive', 'P0a-01-final', 'P0a-02-final',
          'P0a-03-final', 'MiningAssets-dev']


def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT)


def digest(stream):
    h = hashlib.sha256()
    for block in iter(lambda: stream.read(1024 * 1024), b''):
        h.update(block)
    return h.hexdigest()


def main():
    if git('status', '--porcelain').strip():
        raise RuntimeError('Commit the archive snapshot before packaging.')
    revision = git('rev-parse', 'HEAD').decode().strip()
    out = ROOT / 'archives/mining-forge-shelved-2026-09-07.zip'
    out.parent.mkdir(exist_ok=True)
    if out.exists():
        raise FileExistsError(out)
    bundle = ROOT / 'tmp/archive-repository.bundle'
    bundle.parent.mkdir(exist_ok=True)
    subprocess.run(['git', 'bundle', 'create', str(bundle), '--all'], cwd=ROOT, check=True)
    subprocess.run(['git', 'bundle', 'verify', str(bundle)], cwd=ROOT, check=True,
                   stdout=subprocess.DEVNULL)
    items = []
    for name in git('ls-files', '-z').decode().split('\0'):
        if not name:
            continue
        p = Path(name)
        if (p.is_absolute() or '..' in p.parts or
                any(x in p.parts for x in ['.git', 'Library', 'Temp', 'archives']) or
                p.name.startswith('.env') or p.suffix in ['.key', '.pfx', '.p12']):
            raise RuntimeError('Unexpected archive path: ' + name)
        items.append((ROOT / name, 'source/' + name))
    for name in BUILDS:
        folder = ROOT / 'builds' / name
        if not (folder / 'MiningForge.exe').is_file():
            raise FileNotFoundError(folder)
        for p in sorted(folder.rglob('*')):
            if p.is_file():
                items.append((p, 'playable/' + name + '/' + p.relative_to(folder).as_posix()))
    items += [(bundle, 'history/repository.bundle'),
              (ROOT / 'ARCHIVE_README.md', 'START_HERE.md')]
    manifest = {'repository': 'KOSEIHAMAYA2077/mining-forge',
                'snapshot_commit': revision, 'build_sources': {}, 'files': []}
    for name in BUILDS:
        source = ROOT / 'builds' / name / 'SOURCE.txt'
        manifest['build_sources'][name] = source.read_text(encoding='utf-8-sig') if source.exists() else 'Historical build; source provenance not recorded.'
    with zipfile.ZipFile(out, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as z:
        for p, name in items:
            with p.open('rb') as f:
                sha = digest(f)
            manifest['files'].append({'path': name, 'size': p.stat().st_size, 'sha256': sha})
            z.write(p, name)
        z.writestr('MANIFEST.json', json.dumps(manifest, ensure_ascii=False, indent=2))
    with zipfile.ZipFile(out) as z:
        for entry in manifest['files']:
            with z.open(entry['path']) as f:
                if digest(f) != entry['sha256']:
                    raise RuntimeError('Verification failed: ' + entry['path'])
            assert z.getinfo(entry['path']).file_size == entry['size']
        assert len(z.namelist()) == len(manifest['files']) + 1
    with out.open('rb') as f:
        sha = digest(f)
    out.with_suffix('.zip.sha256').write_text(sha + '  ' + out.name + '\n', encoding='ascii')
    receipt = {'zip': out.name, 'bytes': out.stat().st_size, 'sha256': sha,
               'snapshot_commit': revision, 'entries': len(items) + 1,
               'verification': 'Every entry read back; SHA256, sizes and ZIP CRC checked.'}
    out.with_suffix('.receipt.json').write_text(json.dumps(receipt, indent=2), encoding='utf-8')
    print(json.dumps(receipt, indent=2), flush=True)


if __name__ == '__main__':
    main()
