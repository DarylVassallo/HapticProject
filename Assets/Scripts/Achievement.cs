using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievement")]
public class Achievement : ScriptableObject
{
    public string id;
    public string title;
    public string description;
    public int goal;

    [SerializeField] private bool isAboutAngels;
    [SerializeField] private bool isAboutPlayers;

    private void OnEnable()
    {
        if (isAboutAngels)
        {
            Health.OnKilledEnemy += CheckAngelAchievements;
        }
    }

    private void OnDisable()
    {
        if (isAboutAngels)
        {
            Health.OnKilledEnemy -= CheckAngelAchievements;
        }
    }

    private void CheckAngelAchievements()
    {
        
    }
}
