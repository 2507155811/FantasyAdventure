using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameSave
{
    public static int Slot {get;private set;}
    static string Prefix{get{return "FA.slot."+Slot+".";}}
    static string Short(string key){return key.StartsWith("FA.",StringComparison.Ordinal)?key.Substring(3):key;}
    static void Register(string key)
    {
        string registry=PlayerPrefs.GetString(Prefix+"keys","");
        string marker="|"+key+"|";
        if(!registry.Contains(marker))PlayerPrefs.SetString(Prefix+"keys",registry+marker);
    }
    public static void Select(int slot){Slot=Mathf.Clamp(slot,0,2);PlayerPrefs.SetInt("FA.selectedSlot",Slot);}
    public static int GetInt(string key,int fallback=0){return PlayerPrefs.GetInt(Prefix+Short(key),fallback);}
    public static void SetInt(string key,int value){key=Short(key);Register(key);PlayerPrefs.SetInt(Prefix+key,value);}
    public static string GetString(string key,string fallback=""){return PlayerPrefs.GetString(Prefix+Short(key),fallback);}
    public static void SetString(string key,string value){key=Short(key);Register(key);PlayerPrefs.SetString(Prefix+key,value);}
    public static bool Exists(int slot){return PlayerPrefs.GetInt("FA.slot."+slot+".exists",0)!=0;}
    public static string Summary(int slot)
    {
        string prefix="FA.slot."+slot+".";
        return !Exists(slot)?"空存档":"等级 "+Mathf.Clamp(1+PlayerPrefs.GetInt(prefix+"exp",0)/80,1,99)+" · "+PlayerPrefs.GetString(prefix+"mapName","小红帽之家");
    }
    public static void New(int slot)
    {
        Select(slot);
        string[] keys=PlayerPrefs.GetString(Prefix+"keys","").Split('|');
        foreach(string key in keys)if(key.Length>0)PlayerPrefs.DeleteKey(Prefix+key);
        PlayerPrefs.DeleteKey(Prefix+"keys");PlayerPrefs.DeleteKey(Prefix+"exists");
        SetInt("map",41);SetInt("route",0);SetInt("coins",30);SetInt("checkpoint",0);Save();
    }
    public static void Save()
    {
        PlayerPrefs.SetInt(Prefix+"exists",1);PlayerPrefs.SetString(Prefix+"updated",DateTime.UtcNow.ToString("O"));PlayerPrefs.Save();
    }
    public static void MigrateLegacy()
    {
        Select(PlayerPrefs.GetInt("FA.selectedSlot",0));
        if(PlayerPrefs.GetInt("FA.migrated",0)!=0)return;
        int selected=Slot;
        if(!Exists(0)&&(PlayerPrefs.HasKey("FA.coins")||PlayerPrefs.HasKey("FA.exp")))
        {
            Select(0);
            foreach(string key in new[]{"coins","exp","weapon","tier.0","tier.1","tier.2","tier.3"})SetInt(key,PlayerPrefs.GetInt("FA."+key,0));
            SetInt("map",41);SetInt("route",0);SetInt("checkpoint",0);Save();
        }
        Select(selected);PlayerPrefs.SetInt("FA.migrated",1);PlayerPrefs.Save();
    }
}
