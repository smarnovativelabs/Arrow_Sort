from pathlib import Path
p=Path('Docs/DailyTasks20261022/ValidateDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('Check(v.taskScroll.content.anchoredPosition.y>100,"task list scrolls to the last Amplify task");','Check(v.taskScroll.content.anchoredPosition.y>100||v.taskScroll.content.rect.height<=v.taskScroll.viewport.rect.height+1,"last Amplify task is reachable by scrolling or fits on tall screen");')
p.write_text(s,encoding='utf-8')
