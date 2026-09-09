using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

public class EnemyManager : NetworkBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemySpawnPoints;
    [SerializeField] private float spawnTooFarRange;
    [SerializeField] private float spawnTooCloseRange;
    [SerializeField] private int enemyNumLimit;
    // private int enemyCount;
    private List<GameObject> enemyList;
    [SerializeField] private Transform enemyHaptic;

    private NetworkVariable<float> chancesOfEnemy = new(0f);

    private List<Transform> _closeSpawnPoints;

    private Transform _pcPlayer;

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

    private bool disableEnemies;
    private bool createdAnEnemy;

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

        createdAnEnemy = false;
    }

    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerData;
        EventsManager.OnCreateRandomEnemy += EnemyCreation;

        EventsManager.OnRemoveEnemy += RemoveEnemy;

        EventsManager.OnIncreaseChanceOfSpawningEnemy += AddChanceOfEnemysServerRpc;
        EventsManager.OnDisableEnemySpawning += DisableEnemySpawningServerRpc;

        EventsManager.OnGetAppropriateEnemyAudio += GetAppropriateAudio;

        EventsManager.OnDisableEnemies += DisableEnemies;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerData;
        EventsManager.OnCreateRandomEnemy -= EnemyCreation;

        EventsManager.OnRemoveEnemy -= RemoveEnemy;

        EventsManager.OnIncreaseChanceOfSpawningEnemy -= AddChanceOfEnemysServerRpc;
        EventsManager.OnDisableEnemySpawning -= DisableEnemySpawningServerRpc;

        EventsManager.OnGetAppropriateEnemyAudio -= GetAppropriateAudio;

        EventsManager.OnDisableEnemies -= DisableEnemies;
    }

    private void DisableEnemies()
    {
        disableEnemies = true;
    }

    private void GetPCPlayerData()
    {
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetChanceOfEnemysServerRpc(float _chance)
    {
        if(!_isEnemySpawningDisabled) chancesOfEnemy.Value = _chance;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AddChanceOfEnemysServerRpc(float _chance)
    {
        if(!_isEnemySpawningDisabled) chancesOfEnemy.Value = chancesOfEnemy.Value + _chance;
    }

    //If requested, this can prevent any enemies from spawning
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DisableEnemySpawningServerRpc()
    {
        chancesOfEnemy.Value = 0;
        _isEnemySpawningDisabled = true;
    }

    //If need, this constantly checks if an enemy should be randomly spawned
    void FixedUpdate()
    {       
        FindClosestEnemy();

        if(chancesOfEnemy.Value <= 0 || !IsOwner) return;
        PotentialEnemyCreation();
    }

    private void FindClosestEnemy()
    {
        if(_pcPlayer == null) return;
        if(enemyList.Count <= 0)
        {
            if(enemyHaptic.position.y != 100) enemyHaptic.position = new Vector3(0, 0, 100);
            return;
        }

        float minDistance = 999999f;
        float currDistance;
        int closestEnemyNum = -1;

        for(int i = 0; i < enemyList.Count; i++)
        {
            currDistance = Vector3.Distance(enemyList[i].transform.position, _pcPlayer.position);
            if(currDistance < minDistance)
            {
                minDistance = currDistance;
                closestEnemyNum = i;
            }
        }

        if(closestEnemyNum != -1)
        {
            enemyHaptic.position = enemyList[closestEnemyNum].transform.position;
            enemyHaptic.rotation = enemyList[closestEnemyNum].transform.rotation;
        }
    }

    //This properly removes a specific enemy from the scene, and any hidden meshes from the hidden object list
    private void RemoveEnemy(GameObject removedEnemy)
    {
        enemyList.Remove(removedEnemy);
        if(enemyList.Count <= 0) EventsManager.UseEnemyHaptic(false);

        RevealUnderLight[] revealUnderLightObjects = removedEnemy.GetComponentsInChildren<RevealUnderLight>(true);
        foreach (RevealUnderLight revealUnderLight in revealUnderLightObjects)
        {
            EventsManager.RemoveHiddenObject(revealUnderLight.gameObject);
        }
        Destroy(removedEnemy);
    }

    //Using the requested audio type, this finds a variation of it that has not been recently used
    private void GetAppropriateAudio(GameObject enemy, int _audioNum)
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

            EventsManager.SendAppropriateEnemyAudio(enemy, currentAudio, _audioNum); 
        }
    }

    //Once a specific audio is used, a delay is set so it cannot be immediately used by another/the same enemy
    IEnumerator AudioBreak(AudioClip usedAudio, int usedAudioNum)
    {
        yield return new WaitForSeconds(audioBreak);
        _badAudio[usedAudioNum].Remove(usedAudio);
        _goodAudio[usedAudioNum].Add(usedAudio);
    }

    //By random chance (if a random value is less than a set required value), an enemy is spawned near the PC Player
    private void PotentialEnemyCreation()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= chancesOfEnemy.Value)
        {
            InstantiateNearbyRandomEnemyServerRpc();
        }
    }

    //This creates a random number of enemies (between 1 and the set maximum amount) near the PC Player
    private void EnemyCreation(int maxEnemies)
    {
        int _numberOfEnemies = UnityEngine.Random.Range(1, maxEnemies);

        for (int i = 0; i < _numberOfEnemies; i++)
        {
            InstantiateNearbyRandomEnemyServerRpc();
        }
    }

    //This finds a random enemy spawn point that is close enough to the PC Player, 
    // and spawns an enemy there
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InstantiateNearbyRandomEnemyServerRpc()
    {
        float multiplier = 1f;
        if(!createdAnEnemy)
        {
            createdAnEnemy = true;
            EventsManager.FirstEnemyCreated();
            
            multiplier = 0.5f;
        }


        if(disableEnemies) return;

        if(enemyList.Count < enemyNumLimit)
        {
            float distance = 0;
            for (int i = 0; i < enemySpawnPoints.childCount; i++)
            {
                distance = (enemySpawnPoints.GetChild(i).position - _pcPlayer.position).magnitude;

                if (distance > (spawnTooCloseRange * multiplier) && distance <= (spawnTooFarRange * multiplier))
                {
                    _closeSpawnPoints.Add(enemySpawnPoints.GetChild(i));
                }
            }

            int enemyNum = UnityEngine.Random.Range(1, _closeSpawnPoints.Count) - 1;
            var newEnemy = Instantiate(enemy, _closeSpawnPoints[enemyNum].position, Quaternion.identity);
            newEnemy.GetComponent<NetworkObject>().Spawn();
            
            enemyList.Add(newEnemy);
            if(enemyList.Count > 0) EventsManager.UseEnemyHaptic(true);
        }
    }
}
