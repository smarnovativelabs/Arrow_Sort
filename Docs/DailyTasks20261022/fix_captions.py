from pathlib import Path
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs')
s=p.read_text(encoding='utf-8')
lines=[]
for line in s.splitlines():
 if 'view.close=Button("Close daily tasks"' in line: line='        view.close=Button("Close daily tasks",card,790,21,112,112,"X",H("EF3325"));'
 if 'view.previewClose=Button(' in line: line='        view.previewClose=Button("Close gift preview",view.previewCard,268,0,52,44,"X",H("EF5B2C"));'
 lines.append(line)
p.write_text('\n'.join(lines)+'\n',encoding='utf-8')
