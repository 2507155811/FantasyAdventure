using UnityEngine;
public class TimeGateEffect : MonoBehaviour
{
    LineRenderer ring;Material material;
    void Start()
    {
        var go=new GameObject("Time gate ring");go.transform.SetParent(transform,false);
        ring=go.AddComponent<LineRenderer>();ring.useWorldSpace=false;ring.positionCount=49;ring.loop=false;
        ring.startWidth=ring.endWidth=.06f;ring.sortingOrder=1;
        material=new Material(Shader.Find("Sprites/Default"));ring.sharedMaterial=material;
        ring.startColor=new Color(.45f,.9f,1,.8f);ring.endColor=new Color(.8f,.45f,1,.8f);
        for(int i=0;i<49;i++){float a=i/48f*Mathf.PI*2;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*.9f,Mathf.Sin(a)*1.2f,0));}
    }
    void Update(){if(ring)ring.transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time)*8);}
    void OnDestroy(){if(material)Destroy(material);}
}
