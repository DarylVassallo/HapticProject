using UnityEngine;

using Unity.Netcode;

public class AchievementManager : NetworkBehaviour
{
    private NetworkVariable<NetworkObjectReference> pcPlayerRef = new NetworkVariable<NetworkObjectReference>();
    private NetworkVariable<NetworkObjectReference> vrPlayerRef = new NetworkVariable<NetworkObjectReference>();

    private static PlayerProfile _pcPlayerProfile;
    private static PlayerProfile _vrPlayerProfile;

    public Achievement[] achievementList;

    private bool _checkOnAngels;
    private bool _checkOnLevel;

    private NetworkVariable<bool> canPCFunction = new (false);
    private NetworkVariable<bool> canVRFunction = new (false);

    private void Awake()
    {
        _checkOnAngels = false;
        _checkOnLevel = false;

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
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerDataServerRpc;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerDataServerRpc;

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
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerDataServerRpc;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerDataServerRpc;

        if(_checkOnAngels)
        {
            Health.OnKilledEnemy -= UpdateAngelAchievements;
        }

        if(_checkOnLevel)
        {
            WinPlatform.OnWinGame -= UpdateLevelAchievements;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetPCPlayerDataServerRpc()
    {
        GameObject pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer");

        if (pcPlayer != null)
        {
            NetworkObject networkObject = pcPlayer.GetComponent<NetworkObject>();
            pcPlayerRef.Value = networkObject;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetVRPlayerDataServerRpc()
    {
        GameObject vrPlayer = GameObject.FindGameObjectWithTag("VRPlayer");

        if (vrPlayer != null)
        {
            NetworkObject networkObject = vrPlayer.GetComponent<NetworkObject>();
            vrPlayerRef.Value = networkObject;
        }
    }

    private PlayerProfile GetPCProfile()
    {
        if (pcPlayerRef.Value.TryGet(out NetworkObject networkObject))
        {
            return networkObject.GetComponent<PlayerProfile>();
        }

        return null;
    }

    private PlayerProfile GetVRProfile()
    {
        if (vrPlayerRef.Value.TryGet(out NetworkObject networkObject))
        {
            return networkObject.GetComponent<PlayerProfile>();
        }

        return null;
    }

    private void UpdateAngelAchievements(int _addedScore, int _playerType)
    {       
        if(_pcPlayerProfile == null) _pcPlayerProfile = GetPCProfile();
        if(_vrPlayerProfile == null) _vrPlayerProfile = GetVRProfile();

        // GetPlayerData();
        Debug.Log("canVRFunction: " + canVRFunction.Value);
        Debug.Log("canPCFunction: " + canPCFunction.Value);
        
        for (int i = 0; i < achievementList.Length; i++)
        {
            if (achievementList[i].isAboutAngels)
            {
                if (_playerType == 0)
                {
                    Debug.Log("_pcPlayerProfile: " + _pcPlayerProfile);
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

    // private void GetPlayerData()
    // {
    //     if(GameObject.FindGameObjectWithTag("PCPlayer") != null && _pcPlayerProfile == null)
    //     {
    //         _pcPlayerProfile = GameObject.FindGameObjectWithTag("PCPlayer").transform.GetComponent<PlayerProfile>();
    //         canPCFunction = true;
    //     }

    //     if(GameObject.FindGameObjectWithTag("VRPlayer") != null && _vrPlayerProfile == null)
    //     {
    //         _vrPlayerProfile = GameObject.FindGameObjectWithTag("VRPlayer").transform.GetComponent<PlayerProfile>();
    //         canVRFunction = true;
    //     }
    // }

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
