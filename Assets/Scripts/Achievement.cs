using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievement")]
public class Achievement : ScriptableObject
{
    public string id;
    public string title;
    public string description;
    public int goal;

    public bool isAboutAngels;
    public bool isAboutLevel;
}
