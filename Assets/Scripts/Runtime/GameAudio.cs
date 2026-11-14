using UnityEngine;

public enum GameSound { Sword,Hammer,Spear,Magic,Hit,Jump,Hurt,Coin,Menu,Upgrade,Clear,Step }
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance {get;private set;}
    public AudioClip[] effects;
    public AudioClip music;
    public AudioClip[] themes;
    AudioSource voice,background;
    public bool Enabled {get;private set;}
    void Awake()
    {
        Instance=this;Enabled=PlayerPrefs.GetInt("FA.audio",1)!=0;
        voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.spatialBlend=0;voice.volume=.65f;
        background=gameObject.AddComponent<AudioSource>();background.playOnAwake=false;background.spatialBlend=0;background.volume=.18f;background.loop=true;
        background.clip=music;if(Enabled&&music)background.Play();
    }
    public static void Play(GameSound sound)
    {
        if(!Instance||!Instance.Enabled)return;
        int index=(int)sound;
        if(Instance.effects!=null&&index<Instance.effects.Length&&Instance.effects[index])Instance.voice.PlayOneShot(Instance.effects[index]);
    }
    public void ChangeTheme(int theme)
    {
        if(themes==null||theme<0||theme>=themes.Length||!themes[theme]||background.clip==themes[theme])return;
        background.Stop();background.clip=themes[theme];if(Enabled)background.Play();
    }
    public void Toggle()
    {
        Enabled=!Enabled;PlayerPrefs.SetInt("FA.audio",Enabled?1:0);PlayerPrefs.Save();
        if(Enabled){if(background.clip&&!background.isPlaying)background.Play();Play(GameSound.Menu);}
        else{voice.Stop();background.Pause();}
    }
    void OnDestroy(){if(Instance==this)Instance=null;}
}
