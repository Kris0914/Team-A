using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Talk2", menuName = "Dialogue System/Talk2")]
public class Talk : ScriptableObject
{
    [SerializeField]
    public List<string> dialogues = new List<string>();
}
