using UnityEngine;
public class WorldPickup2D : MonoBehaviour
{
    public int item;
    public string saveKey;
    bool collected;
    Vector3 initial;
    void Start(){initial=transform.position;}
    void Update(){transform.position=initial+Vector3.up*Mathf.Sin(Time.time*3)*.08f;}
    void OnTriggerEnter2D(Collider2D other)
    {
        if(collected||!other.GetComponent<PlayerController2D>())return;
        collected=true;GameSave.SetInt(saveKey,1);ChapterOne.Instance.AddItem(item,1);Destroy(gameObject);
    }
}
