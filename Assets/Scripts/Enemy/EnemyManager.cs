using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

public class EnemyManager : NetworkBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemySpawnPoints;
    [SerializeField] private float spawnTooFarRange;
    [SerializeField] private float spawnTooCloseRange;
    [SerializeField] private int enemyNumLimit;
    // private int enemyCount;
    private List<GameObject> enemyList;

    private NetworkVariable<float> chancesOfEnemy = new(0f);

    private List<Transform> _closeSpawnPoints;

    private Transform _pcPlayer;
    private bool canPCFunction = false;

    public static event Action<GameObject> OnRemoveHiddenObject;
    
    

    [Header("Audio")]
    [SerializeField] private List<AudioClip> damageAudios;
    private List<AudioClip> goodDamageAudios = new List<AudioClip>();
    private List<AudioClip> badDamageAudios = new List<AudioClip>();

    [SerializeField] private List<AudioClip> footstepAudios;
    private List<AudioClip> goodFootstepAudios = new List<AudioClip>();
    private List<AudioClip> badFootstepAudios = new List<AudioClip>();

    [SerializeField] private List<AudioClip> deathAudio;
    private List<AudioClip> goodDeathAudio = new List<AudioClip>();
    private List<AudioClip> badDeathAudio = new List<AudioClip>();

    [SerializeField] private List<AudioClip> attackingAudios;
    private List<AudioClip> goodAttackingAudios = new List<AudioClip>();
    private List<AudioClip> badAttackingAudios = new List<AudioClip>();

    private List<List<AudioClip>> _goodAudio;
    private List<List<AudioClip>> _badAudio;

    [SerializeField] private float audioBreak;

    private bool _isEnemySpawningDisabled = false;

    [SerializeField] private bool disableEnemies;

    void Awake()
    {
        goodDamageAudios = damageAudios;
        goodFootstepAudios = footstepAudios;
        goodDeathAudio = deathAudio;
        goodAttackingAudios = attackingAudios;

        _goodAudio = new List<List<AudioClip>> {goodDamageAudios, goodFootstepAudios, goodDeathAudio, goodAttackingAudios};
        _badAudio = new List<List<AudioClip>> {badDamageAudios, badFootstepAudios, badDeathAudio, badAttackingAudios};

        enemyList = new List<GameObject>();
        _closeSpawnPoints = new List<Transform>();
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        MazeManager.OnCreateRandomEnemy += EnemyCreation;

        EnemyWithSpotlight.OnRemoveEnemy += RemoveEnemy;

        TeleportPad.OnIncreaseChanceOfSpawningEnemy += AddChanceOfEnemysServerRpc;
        CheckpointManager.OnDisableEnemySpawning += DisableEnemySpawningServerRpc;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        MazeManager.OnCreateRandomEnemy -= EnemyCreation;

        EnemyWithSpotlight.OnRemoveEnemy -= RemoveEnemy;

        TeleportPad.OnIncreaseChanceOfSpawningEnemy -= AddChanceOfEnemysServerRpc;
        CheckpointManager.OnDisableEnemySpawning -= DisableEnemySpawningServerRpc;
    }

    private void GetPCPlayerData()
    {
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        canPCFunction = true;
    }

    void FixedUpdate()
    {       
        if(chancesOfEnemy.Value <= 0 || !IsOwner) return;
        PotentialEnemyCreation();
    }

    private void RemoveEnemy(GameObject removedEnemy)
    {
        enemyList.Remove(removedEnemy);
        RevealUnderLight[] revealUnderLightObjects = removedEnemy.GetComponentsInChildren<RevealUnderLight>(true);
        foreach (RevealUnderLight revealUnderLight in revealUnderLightObjects)
        {
            OnRemoveHiddenObject?.Invoke(revealUnderLight.gameObject);
        }
        Destroy(removedEnemy);
    }

    public AudioClip GetAppropriateAudio(int _audioNum)
    {        
        int _length = _goodAudio[_audioNum].Count;
        if(_length != 0)
        {
            int _currentNum = UnityEngine.Random.Range(0, _length);
            AudioClip currentAudio = _goodAudio[_audioNum][_currentNum];


            if(_audioNum != 2)
            {
                _badAudio[_audioNum].Add(currentAudio);
                _goodAudio[_audioNum].Remove(currentAudio);

                StartCoroutine(AudioBreak(currentAudio, _audioNum));
            }

            return currentAudio; 
        }

        // PlayAudioClientRpc(1, Random.Range(0, footstepAudios.Length));
        return null;
    }

    IEnumerator AudioBreak(AudioClip usedAudio, int usedAudioNum)
    {
        yield return new WaitForSeconds(audioBreak);
        _badAudio[usedAudioNum].Remove(usedAudio);
        _goodAudio[usedAudioNum].Add(usedAudio);
    }

    // private void UpdateChancesOfEnemy(bool previous, bool current)
    // {
    //     float newChances =  0.01f * (squareWheelEnemyActive.Value ? 1 : 0) + 
    //                         0f * (diamondLeverEnemyActive.Value ? 1 : 0);
    //     SetChanceOfEnemysServerRpc(newChances);
    // }

    private void PotentialEnemyCreation()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= chancesOfEnemy.Value)
        {
            // InstantiateRandomEnemyServerRpc();
            InstantiateNearbyRandomEnemyServerRpc();
        }
    }

    private void EnemyCreation(int maxEnemies)
    {
        int _numberOfEnemies = UnityEngine.Random.Range(1, maxEnemies);

        for (int i = 0; i < _numberOfEnemies; i++)
        {
            // InstantiateRandomEnemyServerRpc();
            InstantiateNearbyRandomEnemyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetChanceOfEnemysServerRpc(float _chance)
    {
        if(!_isEnemySpawningDisabled) chancesOfEnemy.Value = _chance;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddChanceOfEnemysServerRpc(float _chance)
    {
        if(!_isEnemySpawningDisabled) chancesOfEnemy.Value = chancesOfEnemy.Value + _chance;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisableEnemySpawningServerRpc()
    {
        chancesOfEnemy.Value = 0;
        _isEnemySpawningDisabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiateRandomEnemyServerRpc()
    {
        int enemyNum = UnityEngine.Random.Range(1, enemySpawnPoints.childCount) - 1;
        // enemyCount++;
        var newEnemy = Instantiate(enemy, enemySpawnPoints.GetChild(enemyNum).position, Quaternion.identity);
        newEnemy.GetComponent<NetworkObject>().Spawn();
        enemyList.Add(newEnemy);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiateNearbyRandomEnemyServerRpc()
    {
        if(disableEnemies) return;

        if(enemyList.Count < enemyNumLimit)
        {
            float distance = 0;
            for (int i = 0; i < enemySpawnPoints.childCount; i++)
            {
                distance = (enemySpawnPoints.GetChild(i).position - _pcPlayer.position).magnitude;

                if (distance > spawnTooCloseRange && distance <= spawnTooFarRange)
                {
                    _closeSpawnPoints.Add(enemySpawnPoints.GetChild(i));
                }
            }

            int enemyNum = UnityEngine.Random.Range(1, _closeSpawnPoints.Count) - 1;
            // enemyCount++;
            var newEnemy = Instantiate(enemy, _closeSpawnPoints[enemyNum].position, Quaternion.identity);
            newEnemy.GetComponent<NetworkObject>().Spawn();
            enemyList.Add(newEnemy);
        }
    }

    // [ClientRpc]
    // public void RemoveEnemyClientRpc()
    // {
    //     float maxEnemyDistance = 0f;
    //     float currentEnemyDistance = 0f;
    //     GameObject maxEnemy = null;

    //     for(int i = 0; i < enemyList.Count; i++)
    //     {
    //         currentEnemyDistance = Vector3.Distance(enemyList[i].transform.position, _pcPlayer.position);
    //         if(currentEnemyDistance > maxEnemyDistance)
    //         {
    //             maxEnemy = enemyList[i];
    //             maxEnemyDistance = currentEnemyDistance;
    //         }
    //     }

    //     if(maxEnemy != null) RemoveEnemy(maxEnemy);
    // }

}
