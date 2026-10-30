using UnityEngine;
namespace WoolGame {
[CreateAssetMenu(menuName="Arrow Sort/Google Play purchases")]
public sealed class WoolIapConfiguration:ScriptableObject {
 [Tooltip("Enable after the matching products are active in Google Play Console.")] public bool productsPublished;
 [TextArea(3,8),Tooltip("Google Play Console licensing Base64 RSA public key, not a Firebase API key.")] public string googlePlayPublicKey="";
}
public sealed class WoolIapProduct {
 public readonly string id,name,displayPrice,description,tag;public readonly int coins,powers;public readonly bool removeAds,purchaseOnce;
 public WoolIapProduct(string id,string name,int coins,int powers=0,bool removeAds=false,string displayPrice="",bool purchaseOnce=false,string description="",string tag=""){this.id=id;this.name=name;this.coins=coins;this.powers=powers;this.removeAds=removeAds;this.displayPrice=displayPrice;this.purchaseOnce=purchaseOnce||removeAds;this.description=description;this.tag=tag;}
}
public static class WoolIapCatalog {
 public const string RemoveAds="com.peri.arrow.sort.puzzle.rescue.duck.removeads6000coins",Intro="com.peri.arrow.sort.puzzle.rescue.duck.ducklingstarterpack",Special=Intro;
 public static readonly WoolIapProduct[] Products={
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.removeads6000coins","Remove Ads + 6,000 Coins",6000,0,true,"$7.99",true,"6,000 coins; removes forced interstitial ads. Rewarded ads remain optional.","ad_free"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.ducklingstarterpack","Duckling Starter Pack",2000,1,false,"$3.99",false,"Get 2,000 coins and 1 of each of the 4 boosters to begin your duck rescue adventure.","starter_bundle"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.ducklingboosterpack","Duckling Booster Pack",2000,2,false,"$5.99",false,"Get 2,000 coins and 2 of each of the 4 boosters to help rescue more ducks.","feather_boost"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.pondrescuepack","Pond Rescue Pack",6000,4,false,"$8.99",false,"Get 6,000 coins and 4 of each of the 4 boosters for tougher rescue puzzles.","pond_explorer"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.flockrescuepack","Flock Rescue Pack",14000,10,false,"$11.99",false,"Get 14,000 coins and 10 of each of the 4 boosters for your duck rescue missions.","duck_rescue"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.goldenflockpack","Golden Flock Pack",20000,16,false,"$15.99",false,"Get 20,000 coins and 16 of each of the 4 boosters to take on tricky rescue levels.","golden_nest"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.ultimateduckrescuepack","Ultimate Duck Rescue Pack",60000,32,false,"$19.99",false,"Get 60,000 coins and 32 of each of the 4 boosters for your biggest rescue challenges.","royal_duck"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.1000duckcoins","1,000 Duck Coins",1000,0,false,"$3.49",false,"Get 1,000 coins to spend on helpful boosters during your duck rescue adventure.","coins_1000"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.5000duckcoins","5,000 Duck Coins",5000,0,false,"$5.49",false,"Get 5,000 coins to stock up on boosters and keep rescuing ducks.","coins_5000"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.10000duckcoins","10,000 Duck Coins",10000,0,false,"$7.99",false,"Get 10,000 coins to prepare for more challenging duck rescue puzzles.","coins_10000"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.25000duckcoins","25,000 Duck Coins",25000,0,false,"$12.99",false,"Get 25,000 coins to buy boosters and help more ducks reach safety.","coins_25000"),
  new WoolIapProduct("com.peri.arrow.sort.puzzle.rescue.duck.100000duckcoins","100,000 Duck Coins",100000,0,false,"$19.99",false,"Get 100,000 coins for a huge supply of booster purchases in your duck rescue game.","coins_100000"),
 };
 public static readonly string[] BundleIds={"com.peri.arrow.sort.puzzle.rescue.duck.ducklingstarterpack","com.peri.arrow.sort.puzzle.rescue.duck.ducklingboosterpack","com.peri.arrow.sort.puzzle.rescue.duck.pondrescuepack","com.peri.arrow.sort.puzzle.rescue.duck.flockrescuepack","com.peri.arrow.sort.puzzle.rescue.duck.goldenflockpack","com.peri.arrow.sort.puzzle.rescue.duck.ultimateduckrescuepack"};
 public static readonly string[] CoinIds={"com.peri.arrow.sort.puzzle.rescue.duck.1000duckcoins","com.peri.arrow.sort.puzzle.rescue.duck.5000duckcoins","com.peri.arrow.sort.puzzle.rescue.duck.10000duckcoins","com.peri.arrow.sort.puzzle.rescue.duck.25000duckcoins","com.peri.arrow.sort.puzzle.rescue.duck.100000duckcoins"};
 public static WoolIapProduct Find(string id){foreach(var p in Products)if(p.id==id)return p;return null;}
}
}
