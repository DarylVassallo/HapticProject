using UnityEngine;
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

    void Awake()
    {
        goodDamageAudios = damageAudios;
        goodFootstepAudios = footstepAudios;
        goodDeathAudio = deathAudio;
        goodAttackingAudios = attackingAudios;

        _goodAudio = new List<List<AudioClip>> {goodDamageAudios, goodFootstepAudios, goodDeathAudio, goodAttackingAudios};
        _badAudio = new List<List<AudioClip>> {badDamageAudios, badFootstepAudios, badDeathAudio, badAttackingAudios};
    }

    public AudioClip GetAppropriateAudio(int _audioNum)
    {
        Debug.Log("---------------");
        Debug.Log("_goodAudio: " + _goodAudio);
        Debug.Log("_goodAudio[_audioNum]: " + _goodAudio[_audioNum]);
        Debug.Log("_goodAudio[_audioNum].Count: " + _goodAudio[_audioNum].Count);

        int _length = _goodAudio[_audioNum].Count;
        if(_length != 0)
        {
            int _currentNum = Random.Range(0, _length);
            AudioClip currentAudio = _goodAudio[_audioNum][_currentNum];

            _badAudio[_audioNum].Add(currentAudio);
            _goodAudio[_audioNum].Remove(currentAudio);

            Debug.Log("before usedAudio: " + currentAudio);
            Debug.Log("before usedAudioNum: " + _audioNum);
            StartCoroutine(AudioBreak(currentAudio, _audioNum));

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

        Debug.Log("===============");
        Debug.Log("after usedAudio: " + usedAudio);
        Debug.Log("after usedAudioNum: " + usedAudioNum);
    }

    // private void UpdateChancesOfEnemy(bool previous, bool current)
    // {
    //     Debug.Log("UpdateChancesOfEnemy");
    //     Debug.Log("squareWheelEnemyActive.Value: " + squareWheelEnemyActive.Value);
    //     Debug.Log("diamondLeverEnemyActive.Value: " + diamondLeverEnemyActive.Value);
    //     float newChances =  0.01f * (squareWheelEnemyActive.Value ? 1 : 0) + 
    //                         0f * (diamondLeverEnemyActive.Value ? 1 : 0);
    //     Debug.Log("newChances : " + newChances);

    //     SetChanceOfEnemysServerRpc(newChances);
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetChanceOfEnemysServerRpc(float _chance)
    // {
    //     chancesOfEnemy.Value = _chance;
    // }
}
