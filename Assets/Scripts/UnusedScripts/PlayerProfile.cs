using UnityEngine;
using System.Collections.Generic;
using System;
//This is where all the incompleted and completed achievements are stored for a player, as well as the player data
public class PlayerProfile : MonoBehaviour
{
    private int _score;
    private int _bodyType;

    public List<Achievement> _incompleteAchievements;
    public List<Achievement> _completeAchievements;

    private int _angelKillCount;
    private bool _hasCompletedSauronLevel;

    public static event Action OnUpdateProgress;

    public void SetAngelKillCount(int _newAngelKillCount)
    {
        _angelKillCount = _newAngelKillCount;
    }

    public int GetAngelKillCount()
    {
        return _angelKillCount;
    }

    public void SetScore(int _newScore)
    {
        _score = _newScore;
    }

    public int GetScore()
    {
        return _score;
    }

    public void SetBodyType(int _newBodyTypeNumber, Mesh _newBodyType)
    {
        _bodyType = _newBodyTypeNumber;
        this.gameObject.transform.GetComponent<MeshFilter>().mesh = _newBodyType;
    }

    public int GetBodyType()
    {
        return _bodyType;
    }

    public bool HasAchieved(string _achievementId)
    {
        if (_completeAchievements.Count <= 0) return false;

        for (int i = 0; i < _completeAchievements.Count; i++)
        {
            if (_completeAchievements[i].id == _achievementId)
            {
                return true;
            }
        }

        return false;
    }

    public void AddCompleteAchievement(Achievement achievement)
    {
        _completeAchievements.Add(achievement);
    }

    public void CompleteAchievement(Achievement achievement)
    {
        _incompleteAchievements.Remove(achievement);
        _completeAchievements.Add(achievement);

        OnUpdateProgress?.Invoke();
    }

    public void AddIncompleteAchievement(Achievement achievement)
    {
        _incompleteAchievements.Add(achievement);
    }
}
