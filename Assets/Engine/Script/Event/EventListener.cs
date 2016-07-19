using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using SLua;

[CustomLuaClassAttribute]
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
        
        public VoidDelegate onTriggerEnter;
    	public VoidDelegate onTriggerStay;
    	public VoidDelegate onTriggerExit;
    
    	public VoidDelegate onCollisionEnter;
    	public VoidDelegate onCollisionStay;
    	public VoidDelegate onCollisionExit;
    
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
    	
    	void OnCollisionEnter(Collision col)
    	{
        	if (onCollisionEnter != null)
            	onCollisionEnter(col.gameObject);
    	}
    	void OnCollisionStay(Collision col)
    	{
        	if (onCollisionStay != null)
            		onCollisionStay(col.gameObject);
    	}

    	void OnCollisionExit(Collision col)
    	{
        	if (onCollisionExit != null)
            		onCollisionExit(col.gameObject);
    	}

    	void OnTriggerEnter(Collider col)
    	{
        	if (onTriggerEnter != null)
            		onTriggerEnter(col.gameObject);
    	}
    	void OnTriggerStay(Collider col)
    	{
        	if (onTriggerStay != null)
            		onTriggerStay(col.gameObject);
    	}

    	void OnTriggerExit(Collider col)
    	{
        	if (onTriggerExit != null)
            		onTriggerExit(col.gameObject);
    	}
}
