from pathlib import Path
p=Path('Docs/DailyTasks20261022/ValidateDailyTasks.cs');s=p.read_text(encoding='utf-8').replace('yield return new WaitForSecondsRealtime(1.65f);','yield return Settle(v);').replace('yield return new WaitForSecondsRealtime(1.7f);','yield return Settle(v);').replace('Check(WoolProgress.DailyCredits==100,"all tasks total exactly 100 credits");','Check(WoolProgress.DailyCredits==100,"all tasks total exactly 100 credits; got "+WoolProgress.DailyCredits);')
a=s.index('    public static string Main()')
s=s[:a]+'''    static IEnumerator Settle(WoolDailyTasks view)
    {
        var field=typeof(WoolDailyTasks).GetField("collecting",BindingFlags.Instance|BindingFlags.NonPublic);float until=Time.realtimeSinceStartup+6;
        while((bool)field.GetValue(view)&&Time.realtimeSinceStartup<until)yield return null;
        Check(!(bool)field.GetValue(view),"collection animation completed");yield return null;
    }
'''+s[a:]
p.write_text(s,encoding='utf-8')
