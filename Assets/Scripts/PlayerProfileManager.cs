using UnityEngine;
using System;
using System.IO;

//This script controls the player's score, body type
public class PlayerProfileManager : MonoBehaviour
{
    
    private string _pcPlayerFilePath;
    private static PlayerProfile _pcPlayerProfile;

    private string _vrPlayerFilePath;
    private static PlayerProfile _vrPlayerProfile;

    public static event Action OnUpdateScore;
    
    private string[] lines;

    [SerializeField] private Mesh[] meshList;

    [SerializeField] private Achievement[] achievementList;
    void Awake()
    {
        _pcPlayerFilePath = Application.persistentDataPath + "/pcPlayerProgress.txt";
        _pcPlayerProfile = GameObject.FindGameObjectWithTag("PCPlayer").transform.GetComponent<PlayerProfile>();
        LoadProgress(_pcPlayerFilePath, _pcPlayerProfile, 0);

        _vrPlayerFilePath = Application.persistentDataPath + "/vrPlayerProgress.txt";
        _vrPlayerProfile = GameObject.FindGameObjectWithTag("VRPlayer").transform.GetComponent<PlayerProfile>();
        LoadProgress(_vrPlayerFilePath, _vrPlayerProfile, 1);
    }

    private void OnEnable()
    {
        WinPlatform.OnWinGame += SaveProgress;
        Health.OnKilledEnemy += AddScore;
    }

    private void OnDisable()
    {
        WinPlatform.OnWinGame -= SaveProgress;
        Health.OnKilledEnemy -= AddScore;
    }

    private void AddScore(int _addedScore, int _playerType)
    {        
        switch (_playerType)
        {
            case 0:
                _pcPlayerProfile.SetScore(_pcPlayerProfile.GetScore() + _addedScore);
                break;
            case 1:
                _vrPlayerProfile.SetScore(_vrPlayerProfile.GetScore() + _addedScore);
                break;
        }
        OnUpdateScore?.Invoke();
    }

    public static int GetScore(int _playerType)
    {
        if (_playerType == 0)  return _pcPlayerProfile.GetScore();

        return -1;
    }

    private void LoadProgress(string _filePath, PlayerProfile _playerProfile, int _playerType)
    {
        if (!File.Exists(_filePath))
        {
            SavePlayerProgress(_filePath, _playerProfile);

            _playerProfile.SetScore(0);
            AddScore(0, _playerType);

            _playerProfile.SetBodyType(0, meshList[0]);

            for (int i = 0; i < achievementList.Length; i++)
            {
                _playerProfile.SetIncompleteAchievment(achievementList[i]);
            }
            return;
        }

        StreamReader reader = new StreamReader(_filePath); 

        while(!reader.EndOfStream)
        {
            string[] lines =  reader.ReadLine().Split(char.Parse(":"));
            if (lines[0].Equals("Score"))
            {
                _playerProfile.SetScore(int.Parse(lines[1]));
                AddScore(0, _playerType);
            }else if (lines[0].Equals("BodyType"))
            {
                _playerProfile.SetBodyType(int.Parse(lines[1]), meshList[int.Parse(lines[1])]);
            }
            else
            {
                for (int i = 0; i < achievementList.Length; i++)
                {
                    if(lines[0].Equals(achievementList[i].title))
                    {
                        if (bool.Parse(lines[1]) == true)
                        {
                            _playerProfile.SetCompleteAchievment(achievementList[i]);
                        }else if (bool.Parse(lines[1]) == false)
                        {
                            _playerProfile.SetIncompleteAchievment(achievementList[i]);
                        }
                    }
                }
            }
        }
        reader.Close();
    }

    public void SaveProgress()
    {
        SavePlayerProgress(_pcPlayerFilePath, _pcPlayerProfile);
        SavePlayerProgress(_vrPlayerFilePath, _vrPlayerProfile);
    }
    private void SavePlayerProgress(string _filePath, PlayerProfile _playerProfile) 
    {
        using (StreamWriter writer = new StreamWriter(_filePath)) 
        {
            writer.WriteLine("Score:" + _playerProfile.GetScore());
            writer.WriteLine("BodyType:" + _playerProfile.GetBodyType());

            for (int i = 0; i < achievementList.Length; i++)
            {
                writer.WriteLine(achievementList[i].title + ":" + _playerProfile.HasAchieved(achievementList[i].id));
            }
        }
    }
}
