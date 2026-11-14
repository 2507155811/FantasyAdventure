using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    string text;
    bool playerDamage;
    float age;
    GUIStyle style;
    public static void Show(Vector3 position,int amount,bool playerDamage=false)
    {
        var go=new GameObject("Damage feedback");go.transform.position=position;
        var popup=go.AddComponent<DamagePopup>();popup.text=amount.ToString();popup.playerDamage=playerDamage;
    }
    void Update(){age+=Time.deltaTime;transform.position+=Vector3.up*Time.deltaTime*.8f;if(age>.7f)Destroy(gameObject);}
    void OnGUI()
    {
        if(!Camera.main)return;
        Vector3 point=Camera.main.WorldToScreenPoint(transform.position);if(point.z<0||point.x<0||point.x>Screen.width)return;
        if(style==null)
        {
            style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
            if(DemoHUD.Instance)style.font=DemoHUD.Instance.interfaceFont;
        }
        style.fontSize=Mathf.Clamp(Mathf.RoundToInt(21*AdventureControls.View.Scale),16,42);
        var matrix=GUI.matrix;var color=GUI.color;GUI.matrix=Matrix4x4.identity;
        float opacity=Mathf.Clamp01((.7f-age)/.25f);
        Rect rect=new Rect(point.x-60,Screen.height-point.y-25,120,55);
        GUI.color=new Color(.08f,.04f,.02f,opacity);GUI.Label(new Rect(rect.x+2,rect.y+2,rect.width,rect.height),text,style);
        GUI.color=playerDamage?new Color(1,.35f,.28f,opacity):new Color(1,.9f,.3f,opacity);GUI.Label(rect,text,style);
        GUI.color=color;GUI.matrix=matrix;
    }
}
