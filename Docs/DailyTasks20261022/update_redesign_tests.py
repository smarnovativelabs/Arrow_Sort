from pathlib import Path
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('GIFT_FILE','exec-55916cb3-d87d-4c8e-848c-b8eafdc1aec3.png').replace('ROOM_FILE','exec-c91c8598-b51b-43a3-9959-3918bf2ade2f.png');p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/WoolDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('gift==4?-105:-408','gift==4?-130:-495');p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/ValidateDailyTasks.cs');s=p.read_text(encoding='utf-8')
s=s.replace('Check(v.gifts.All(b=>!b.interactable),"all gifts locked before 25 credits");','Check(v.gifts.All(b=>b.interactable),"all gifts can preview even before 25 credits");v.gifts[1].onClick.Invoke();Check(v.preview.activeSelf&&v.previewAmount.text=="x30"&&!v.previewCollect.interactable,"locked gift previews 30 coins without claiming");v.previewClose.onClick.Invoke();')
s=s.replace('Check(v.gifts[0].interactable&&!v.gifts[1].interactable,"only the 25-credit gift unlocks");','Check(v.gifts.All(b=>b.interactable),"gift previews remain available");')
s=s.replace('v.previewAmount.text=="20 COINS"','v.previewAmount.text=="x20"').replace('v.previewAmount.text=="500 COINS"','v.previewAmount.text=="x500"').replace('v.previewAmount.text==WoolDailyTaskState.GiftCoins[i]+" COINS"','v.previewAmount.text=="x"+WoolDailyTaskState.GiftCoins[i]')
s=s.replace('Check(WoolProgress.Coins==20,"gift collection awards 20 coins once");','Check(WoolProgress.Coins==20,"gift collection awards 20 coins once");Check(v.giftArt[0].sprite==v.giftOpen[0]&&v.giftGlows[0].activeSelf,"collected gift lid opens and shines");')
s=s.replace('((RectTransform)button.transform).GetWorldCorners(corners);','''if(button.transform.IsChildOf(v.taskScroll.content))
                    {
                        var world=button.transform.position;var local=v.taskScroll.viewport.InverseTransformPoint(world);
                        if(!v.taskScroll.viewport.rect.Contains(local))continue;
                    }
                    ((RectTransform)button.transform).GetWorldCorners(corners);''')
s=s.replace('Check(tested==13,w+"x"+h+" expected 13 daily controls checked");','Check(tested>=9,w+"x"+h+" visible daily controls checked");')
s=s.replace('results.Clear();WoolProgress.BeginPreview();','results.Clear();WoolProgress.BeginPreview();Check(v.info==null&&!v.card.Find("Gold title banner"),"top banner and info button removed");')
s=s.replace('yield return new WaitForSecondsRealtime(.3f);\n            Check(v.claims.All','yield return new WaitForSecondsRealtime(.3f);v.taskScroll.verticalNormalizedPosition=1;\n            Check(v.claims.All')
s=s.replace('results.Add("COMPLETE:', 'v.taskScroll.verticalNormalizedPosition=0;yield return new WaitForSecondsRealtime(.2f);Check(v.taskScroll.content.anchoredPosition.y>100,"task list scrolls to the last reward-video task");Capture("scrolled-bottom");\n            results.Add("COMPLETE:')
p.write_text(s,encoding='utf-8')
