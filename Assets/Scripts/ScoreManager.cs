using UnityEngine;
using System;
//This script controls the player's score
public class ScoreManager : MonoBehaviour
{
    private int _score;
    public static event Action<int> OnChangedScore;
     private void OnEnable()
    {
        Health.OnKilledEnemy += AddScore;
    }

    private void OnDisable()
    {
        Health.OnKilledEnemy -= AddScore;
    }

    private void AddScore(int _addedScore)
    {
        _score += _addedScore;
        OnChangedScore?.Invoke(_score);
    }
}
