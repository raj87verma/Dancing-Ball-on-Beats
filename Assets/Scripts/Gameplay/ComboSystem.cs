using System;
using UnityEngine;

namespace DancingBallOnBeats.Gameplay
{
    /// <summary>How well-timed a hit was, relative to the beat.</summary>
    public enum HitJudgement
    {
        Perfect,
        Good,
        Miss
    }

    /// <summary>
    /// Tracks combo streak and converts beat-timing accuracy into a score multiplier.
    /// Pure logic, no MonoBehaviour dependency, so it's trivial to unit-reason-about and reuse.
    /// </summary>
    public class ComboSystem
    {
        public int CurrentCombo { get; private set; }
        public int BestCombo { get; private set; }
        public float CurrentMultiplier { get; private set; } = 1f;

        private readonly float _multiplierStep;
        private const float MaxMultiplier = 4f;

        public event Action<int> OnComboChanged;
        public event Action OnComboBroken;

        public ComboSystem(float multiplierStep)
        {
            _multiplierStep = Mathf.Max(0.01f, multiplierStep);
        }

        /// <summary>Registers a hit and returns the score to award (base score * multiplier), or 0 on Miss.</summary>
        public float RegisterHit(HitJudgement judgement, float baseScore)
        {
            if (judgement == HitJudgement.Miss)
            {
                Reset();
                return 0f;
            }

            CurrentCombo++;
            if (CurrentCombo > BestCombo) BestCombo = CurrentCombo;

            CurrentMultiplier = Mathf.Min(MaxMultiplier, 1f + CurrentCombo * _multiplierStep);
            OnComboChanged?.Invoke(CurrentCombo);

            float judgementScale = judgement == HitJudgement.Perfect ? 1.5f : 1f;
            return baseScore * CurrentMultiplier * judgementScale;
        }

        public void Reset()
        {
            if (CurrentCombo > 0)
            {
                CurrentCombo = 0;
                CurrentMultiplier = 1f;
                OnComboBroken?.Invoke();
            }
        }
    }
}
