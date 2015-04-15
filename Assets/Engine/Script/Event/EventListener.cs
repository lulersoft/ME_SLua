using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
public class EventListener : MonoBehaviour, IEventSystemHandler, IPointerClickHandler, ISubmitHandler// UnityEngine.EventSystems.EventTrigger
{
	public delegate void VoidDelegate (GameObject go);
    public delegate void BaseEventDelegate(BaseEventData eventData);

	public VoidDelegate onClick;
	public VoidDelegate onDown;
	public VoidDelegate onEnter;
	public VoidDelegate onExit;
	public VoidDelegate onUp;
	public VoidDelegate onSelect;
	public VoidDelegate onUpdateSelect;
    public BaseEventDelegate onSubmit;
    
    static public EventListener Get(GameObject go)
	{  
        EventListener listener = go.GetComponent<EventListener>();
        if (listener == null) listener = go.AddComponent<EventListener>();
		return listener;
	}
	public  void OnPointerClick(PointerEventData eventData)
    {       
        onClick(gameObject);        
	}
	public  void OnPointerDown (PointerEventData eventData){
		if(onDown != null) onDown(gameObject);    
	}
	public  void OnPointerEnter (PointerEventData eventData){
		if(onEnter != null) onEnter(gameObject);
	}
	public  void OnPointerExit (PointerEventData eventData){
		if(onExit != null) onExit(gameObject);
        
	}
	public  void OnPointerUp (PointerEventData eventData){
		if(onUp != null) onUp(gameObject);  
	}
	public  void OnSelect (BaseEventData eventData){
		if(onSelect != null) onSelect(gameObject);
	}

    public void OnSubmit(BaseEventData eventData)
    {
        if (onSubmit != null) onSubmit(eventData);
    }
}
