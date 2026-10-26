from pathlib import Path
p=Path('Docs/DailyTasks20261022/WoolHomeRewardBadges.cs');s=p.read_text(encoding='utf-8-sig').replace('if(shop)shop.SetActive(gift);','if(shop)shop.SetActive(gift||(!WoolProgress.DailyClaimed(1)&&WoolProgress.DailyCount(1)>=WoolProgress.DailyTargets[1]));');p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('six task rows','eight task rows');p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/ValidateDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('wins=2,coins=500,unlocks=1,spins=1,videos=1','version=2,wins=2,coins=500,unlocks=1,removes=1,sorts=1,amplifies=1').replace('six completed tasks','eight completed tasks')
s=s.replace('v.claims[4].onClick.Invoke();v.claims[4].onClick.Invoke();','v.claims[0].onClick.Invoke();v.claims[0].onClick.Invoke();')
s=s.replace('Check(WoolProgress.DailyCredits==25&&WoolProgress.DailyChestCredits==700,','Check(WoolProgress.DailyCredits==10,"login awards exactly 10 credits");v.claims[1].onClick.Invoke();yield return new WaitForSecondsRealtime(1.65f);\n            Check(WoolProgress.DailyCredits==25&&WoolProgress.DailyChestCredits==700,')
s=s.replace('v.rows[4].anchoredPosition.y<v.rows.Where((r,i)=>i!=4)','v.rows[1].anchoredPosition.y<v.rows.Where((r,i)=>i!=1)').replace('v.rowArt[4].color.r<.8f&&!v.claims[4].interactable','v.rowArt[1].color.r<.8f&&!v.claims[1].interactable')
s=s.replace('foreach(int task in new[]{0,1,2,3,5})','foreach(int task in new[]{2,3,4,5,6,7})')
s=s.replace('if(min.x<','if(min.x<')
s=s.replace('Check(WoolProgress.Coins==700,"four gifts total 200 plus chest 500");','''Check(WoolProgress.Coins==700,"four gifts total 200 plus chest 500");
            var badges=hud.GetComponent<WoolHomeRewardBadges>();badges.Refresh();Check(!badges.tasks.activeSelf,"daily task badge clears after all rewards collected");
            type.GetField("lastShopGiftDay").SetValue(campaign,"");badges.Refresh();Check(badges.shop.activeSelf&&badges.freeCoins.activeSelf,"shop and daily free-coin markers show available gift");
            Check(WoolProgress.ClaimShopDailyGift(),"real shop gift claim succeeds");badges.Refresh();Check(!badges.shop.activeSelf&&!badges.freeCoins.activeSelf,"shop and free-coin markers clear after claim");
            Check(badges.spin.activeSelf==(WoolProgress.AvailableSpins>0),"home spin marker follows actual available spins");''')
p.write_text(s,encoding='utf-8')
