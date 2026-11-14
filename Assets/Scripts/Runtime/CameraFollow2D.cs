using UnityEngine;
public class CameraFollow2D:MonoBehaviour{
 public Transform target;public float smooth=5;public Vector3 offset=new Vector3(0,1.4f,-10);
 void Start(){if(!target){var p=FindObjectOfType<PlayerController2D>();if(p)target=p.transform;}}
 void LateUpdate(){if(!target)return;Vector3 wanted=target.position+offset;wanted.x=Mathf.Clamp(wanted.x,0,42);transform.position=Vector3.Lerp(transform.position,wanted,smooth*Time.deltaTime);}
}