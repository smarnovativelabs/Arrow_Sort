using UnityEngine;
namespace WoolGame {
public sealed class WoolHomeRewardBadges:MonoBehaviour {
 public GameObject tasks,shop,freeCoins,spin;float next;
 void OnEnable(){Refresh();}
 void Update(){if(Time.unscaledTime>=next){next=Time.unscaledTime+.25f;Refresh();}}
 public void Refresh(){if(tasks)tasks.SetActive(WoolProgress.HasDailyRewardToCollect);bool gift=WoolProgress.CanClaimShopDailyGift;if(shop)shop.SetActive(gift||(!WoolProgress.DailyClaimed(1)&&WoolProgress.DailyCount(1)>=WoolProgress.DailyTargets[1]));if(freeCoins)freeCoins.SetActive(gift);if(spin)spin.SetActive(WoolProgress.AvailableSpins>0);}
}}
