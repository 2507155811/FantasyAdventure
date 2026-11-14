using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public float maximumX=122,worldLeft=-4,worldRight=132,smoothTime=.14f;
    public Transform target;
    public Vector3 offset=new Vector3(0,1.4f,-10);
    Vector3 velocity;
    Camera view;
    void Start()
    {
        view=GetComponent<Camera>();
        if(!target){var player=FindFirstObjectByType<PlayerController2D>();if(player)target=player.transform;}
    }
    Vector3 Desired(bool snap)
    {
        if(!view)view=GetComponent<Camera>();
        var player=target.GetComponent<PlayerController2D>();
        float x=target.position.x+(player?player.Facing*.6f:0);
        float halfWidth=view.orthographicSize*view.aspect;
        float minX=worldLeft+halfWidth,maxX=Mathf.Min(maximumX,worldRight-halfWidth);
        x=maxX>=minX?Mathf.Clamp(x,minX,maxX):(worldLeft+worldRight)*.5f;
        float y=target.position.y+offset.y;
        if(!snap&&player&&!player.IsGrounded)y=Mathf.Clamp(transform.position.y,y-1.5f,y+1.5f);
        return new Vector3(x,Mathf.Clamp(y,2,6),offset.z);
    }
    public void SnapToTarget(){if(target){transform.position=Desired(true);velocity=Vector3.zero;}}
    void LateUpdate()
    {
        if(!target||Time.deltaTime<=0)return;
        transform.position=Vector3.SmoothDamp(transform.position,Desired(false),ref velocity,smoothTime,Mathf.Infinity,Time.deltaTime);
    }
}
