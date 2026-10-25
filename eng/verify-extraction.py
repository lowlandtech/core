from pathlib import Path
import hashlib,json
root=Path(__file__).resolve().parents[1]
snapshot=json.loads((root/'docs/source-snapshot.json').read_text(encoding='utf-8'))
count=0
for entry in snapshot['files']:
    if entry['target']=='src/lib/LowlandTech.Core.csproj':
        continue
    raw=(root/entry['target']).read_bytes().replace(b'\r\n',b'\n')
    assert hashlib.sha256(raw).hexdigest()==entry['normalizedSha256'],entry['target']
    count+=1
print(f'PASS: {count} imported library files unchanged; project wiring is the sole documented adaptation.')
