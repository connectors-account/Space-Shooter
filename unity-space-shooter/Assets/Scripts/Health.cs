using System;
using UnityEngine;

namespace Starward
{
    public sealed class Health : MonoBehaviour
    {
        public int Current { get; private set; }
        public int Maximum { get; private set; }
        public bool Invulnerable { get { return Time.time < immuneUntil; } }
        public event Action Died;
        float immuneUntil;
        public void ResetHealth(int maximum, float initialProtection = 0)
        {
            Maximum = Current = maximum;
            immuneUntil = Time.time + initialProtection;
        }
        public bool Damage(int amount, float protection = 0)
        {
            if (Current <= 0 || Invulnerable || amount <= 0) return false;
            Current = GameRules.Damage(Current, amount);
            immuneUntil = Time.time + protection;
            if (Current == 0 && Died != null) Died();
            return true;
        }
        public void Heal(int amount) { if (Current > 0) Current = GameRules.Heal(Current, amount, Maximum); }
    }
}
