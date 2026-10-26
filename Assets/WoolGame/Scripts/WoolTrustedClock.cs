using System;
using UnityEngine;
namespace WoolGame {
 public sealed class WoolTrustedClock:MonoBehaviour {
  public static bool Ready=>true;
  public static long UnixNow=>DateTimeOffset.Now.ToUnixTimeSeconds();
  public static string Status=>"Device local time";
  public static long LocalDay(long unix){var date=DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime.Date;return new DateTimeOffset(DateTime.SpecifyKind(date,DateTimeKind.Utc)).ToUnixTimeSeconds()/86400;}
  public static TimeSpan UntilLocalMidnight=>DateTime.Today.AddDays(1)-DateTime.Now;
  public static void Ensure(){}
 }
}