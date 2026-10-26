from pathlib import Path
import hashlib,json
root=Path(__file__).resolve().parents[1]
items=[
('UiClick','All UI button presses','Soft UI Click','Universfield','sound-effects/film-special-effects-soft-ui-click-147352/'),
('MenuCalm','Non-level background music','Cinematic Fantasy Calm Theme','Luis_Humanoide','music/main-title-cinematic-fantasy-calm-theme-294221/'),
('Victory','Level won','Brass Fanfare','Universfield','sound-effects/film-special-effects-brass-fanfare-144755/'),
('Defeat','Level lost','Game Over Orchestral Stinger - Cartoon Defeat','Coghezzi','sound-effects/musical-game-over-orchestral-stinger-cartoon-defeat-546515/'),
('AbilityUnlock','Unlock one slot','Unlock stinger','Sunovia','sound-effects/film-special-effects-unlock-stinger-289722/'),
('AbilityRemove','Remove a vehicle','Pop Cartoon','CreatorsHome','sound-effects/film-special-effects-pop-cartoon-328167/'),
('AbilitySort','Rearrange snake colors','Magic Twinkle','Universfield','sound-effects/film-special-effects-magic-twinkle-244951/'),
('AbilityFog','Pierce fog','Magical Whoosh','u_hyed0v3ux9','sound-effects/film-special-effects-magical-whoosh-148459/'),
]
records=[]
for filename,role,title,creator,page in items:
    path=root/'Assets/Sounds/Pixabay'/f'{filename}.mp3'
    records.append(dict(file=str(path.relative_to(root)).replace('\\','/'),role=role,title=title,creator=creator,source='https://pixabay.com/'+page,license='https://pixabay.com/service/license-summary/',downloaded='2026-10-25',sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
out=root/'Docs/AudioRelease20261025';out.mkdir(parents=True,exist_ok=True)
(out/'sources.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
lines=['# Audio sources','', 'Downloaded from the individual Pixabay asset pages on 25 October 2026. The pages identify these assets as free for use under the [Pixabay Content License](https://pixabay.com/service/license-summary/). Files are embedded as part of the game; this project does not offer them as a standalone sound library.','', '| Game use | Track and creator | Source |','|---|---|---|']
for r in records:lines.append(f"| {r['role']} | {r['title']} — {r['creator']} | [Pixabay]({r['source']}) |")
lines.extend(['','The original `Assets/Sounds/Background.mp3` remains the level music. Original coin, spin, flame, shot, docking, movement, and hit assets remain in use. No original sound file was overwritten.','', '`sources.json` records the exact file paths and SHA-256 hashes. The sound files were downloaded through the normal Pixabay download controls.'])
(out/'Credits.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(f'Recorded {len(records)} source receipts')
