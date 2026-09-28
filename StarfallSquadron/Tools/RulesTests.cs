using System;
using Starfall;

internal static class RulesTests
{
    private static int count;
    private static void Check(string name, bool pass)
    {
        if (!pass) throw new Exception("FAIL: "+name);
        count++; Console.WriteLine("PASS: "+name);
    }
    public static void Main()
    {
        Check("five-wave campaign", GameRules.WaveCount==5);
        Check("opening wave size", GameRules.EnemyCount(1)==8);
        Check("fourth wave size", GameRules.EnemyCount(4)==17);
        Check("one final boss", GameRules.EnemyCount(5)==1);
        Check("opening scout health", GameRules.EnemyHealth(0,1)==2);
        Check("gunship health scales", GameRules.EnemyHealth(2,4)==8);
        Check("boss health", GameRules.EnemyHealth(3,5)==110);
        Check("score tiers", GameRules.Points(0)==100 && GameRules.Points(1)==175 && GameRules.Points(2)==250 && GameRules.Points(3)==2000);
        Check("repair adds two", GameRules.Heal(1)==3);
        Check("repair caps at max", GameRules.Heal(4)==5 && GameRules.Heal(5)==5);
        Check("clamp low", GameRules.Clamp(-9,-8,8)==-8);
        Check("clamp high", GameRules.Clamp(9,-8,8)==8);
        Check("clamp interior", GameRules.Clamp(2,-8,8)==2);
        Check("fast bullet crosses target", GameRules.SegmentHit(0,-10,0,10,0,0,.3f));
        Check("fast bullet misses target", !GameRules.SegmentHit(1,-10,1,10,0,0,.3f));
        Check("zero-length inside circle", GameRules.SegmentHit(0,0,0,0,0,0,.3f));
        Check("zero-length outside circle", !GameRules.SegmentHit(1,0,1,0,0,0,.3f));
        Check("boundary contact", GameRules.SegmentHit(-1,1,1,1,0,0,1));
        Check("circle behind segment", !GameRules.SegmentHit(2,0,3,0,0,0,1));
        Check("circle past segment", !GameRules.SegmentHit(-3,0,-2,0,0,0,1));
        Check("relative moving target collision", GameRules.SegmentHit(-2,0,2,0,0,0,.5f));
        Console.WriteLine(count+" engine-independent checks passed. This is not a Unity runtime test.");
    }
}
