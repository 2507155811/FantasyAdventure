using System.Collections.Generic;
using UnityEngine;

public class WorldInteraction : MonoBehaviour
{
    public enum Kind{Npc,Exit}
    public Kind kind;
    public string label;
    public static readonly List<WorldInteraction> Active=new List<WorldInteraction>();
    void OnEnable(){Active.Add(this);}
    void OnDisable(){Active.Remove(this);}
}
