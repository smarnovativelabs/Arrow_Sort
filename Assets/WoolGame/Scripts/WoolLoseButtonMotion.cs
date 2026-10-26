using UnityEngine;
using UnityEngine.EventSystems;
namespace WoolGame {
public sealed class WoolLoseButtonMotion:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
 bool pressed;float clock;Vector3 scale=Vector3.one;
 void OnEnable(){clock=0;pressed=false;transform.localScale=Vector3.one;}
 public void OnPointerDown(PointerEventData e){pressed=true;}
 public void OnPointerUp(PointerEventData e){pressed=false;}
 public void OnPointerExit(PointerEventData e){pressed=false;}
 void Update(){clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);float target=pressed?.94f:1+Mathf.Sin(clock*2.3f)*.012f;scale=Vector3.Lerp(scale,Vector3.one*target,Mathf.Min(1,Time.unscaledDeltaTime*18));transform.localScale=scale;}
}

}