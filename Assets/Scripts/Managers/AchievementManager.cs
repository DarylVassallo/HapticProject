using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    private static PlayerProfile _pcPlayerProfile;
    private static PlayerProfile _vrPlayerProfile;

    public Achievement[] achievementList;

    private bool _checkOnAngels;
    private bool _checkOnLevel;

    private void Awake()
    {
        _checkOnAngels = false;
        _checkOnLevel = false;

        _pcPlayerProfile = GameObject.FindGameObjectWithTag("PCPlayer").transform.GetComponent<PlayerProfile>();
        _vrPlayerProfile = GameObject.FindGameObjectWithTag("VRPlayer").transform.GetComponent<PlayerProfile>();

        for (int i = 0; i < achievementList.Length; i++)
        {
            if (achievementList[i].isAboutAngels)
            {
                _checkOnAngels = true;
            }

            if (achievementList[i].isAboutLevel)
            {
                _checkOnLevel = true;
            }
        }
    }

    private void OnEnable()
    {
        if(_checkOnAngels)
        {
            Health.OnKilledEnemy += UpdateAngelAchievements;
        }

        if(_checkOnLevel)
        {
            WinPlatform.OnWinGame += UpdateLevelAchievements;
        }
    }

    private void OnDisable()
    {
        if(_checkOnAngels)
        {
            Health.OnKilledEnemy -= UpdateAngelAchievements;
        }

        if(_checkOnLevel)
        {
            WinPlatform.OnWinGame -= UpdateLevelAchievements;
        }
    }

    private void UpdateAngelAchievements(int _addedScore, int _playerType)
    {       
        for (int i = 0; i < achievementList.Length; i++)
        {
            if (achievementList[i].isAboutAngels)
            {
                if (_playerType == 0)
                {
                    _pcPlayerProfile.SetAngelKillCount(_pcPlayerProfile.GetAngelKillCount() + 1);

                    if (_pcPlayerProfile.GetAngelKillCount() == achievementList[i].goal && _pcPlayerProfile._incompleteAchievements.Contains(achievementList[i])) _pcPlayerProfile.CompleteAchievement(achievementList[i]);
                }
                else if (_playerType == 1)
                {
                    _vrPlayerProfile.SetAngelKillCount(_vrPlayerProfile.GetAngelKillCount() + 1);

                    if (_vrPlayerProfile.GetAngelKillCount() == achievementList[i].goal && _vrPlayerProfile._incompleteAchievements.Contains(achievementList[i])) _vrPlayerProfile.CompleteAchievement(achievementList[i]);
                }
            }
        }
    }

    private void UpdateLevelAchievements()
    {       
        for (int i = 0; i < achievementList.Length; i++)
        {
            if (achievementList[i].isAboutLevel)
            {
                _pcPlayerProfile.CompleteAchievement(achievementList[i]);
                _vrPlayerProfile.CompleteAchievement(achievementList[i]);
            }
        }
    }
}
