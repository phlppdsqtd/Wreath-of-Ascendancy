using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill Tree/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite icon;
    public Skill[] prerequisites;
    public bool isUnlocked;
}