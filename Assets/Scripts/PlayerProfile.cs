using UnityEngine;
using System.Collections.Generic;

public class PlayerProfile : MonoBehaviour
{
    private int _score;
    private int _bodyType;

    public List<Achievement> _incompleteAchievements;
    public List<Achievement> _completeAchievements;
    private bool _hasCompletedSauronLevel;
    private bool _hasKilledOneAngel;

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

    public void SetCompleteAchievment(Achievement achievement)
    {
        _completeAchievements.Add(achievement);
    }
    public void SetIncompleteAchievment(Achievement achievement)
    {
        Debug.Log("achievement: " + achievement);
        Debug.Log("1 _incompleteAchievements.Count: " + _incompleteAchievements.Count);
        _incompleteAchievements.Add(achievement);
        Debug.Log("2 _incompleteAchievements.Count: " + _incompleteAchievements.Count);
    }


    public bool GetHasKilledOneAngel()
    {
        return _hasKilledOneAngel;
    }
}
