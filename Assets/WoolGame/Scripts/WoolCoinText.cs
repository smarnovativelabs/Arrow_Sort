using System;using System.Globalization;
namespace WoolGame {
public static class WoolCoinText {
 static readonly string[] Suffix={"","K","M","B","T","Qa","Qi"};
 public static string Format(long value){decimal number=value;bool negative=number<0;if(negative)number=-number;int unit=0;while(number>=1000&&unit<Suffix.Length-1){number/=1000;unit++;}if(unit>0)number=Math.Floor(number*10)/10;return (negative?"-":"")+number.ToString(unit==0?"0":"0.#",CultureInfo.InvariantCulture)+Suffix[unit];}
}
}
