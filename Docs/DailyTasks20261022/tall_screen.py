from pathlib import Path
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('Gradient("Task board frame",card,','view.taskFrame=Gradient("Task board frame",card,');p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/ValidateDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('{1536,2048,60,40}}','{1536,2048,60,40},{960,2658,85,50}}').replace('for(int i=0;i<3;i++)\n            {\n                int w=sizes','for(int i=0;i<4;i++)\n            {\n                int w=sizes');p.write_text(s,encoding='utf-8')
