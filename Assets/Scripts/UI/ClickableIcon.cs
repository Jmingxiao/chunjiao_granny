using ACG;
using UnityEngine;
using UnityEngine.EventSystems;
public enum IconType
{
    None,
    Trigger,
    Boolean
}
/// <summary>
/// ClickableIcon is a class that handles the click events on UI icons.
public class ClickableIcon : MObject,IPointerDownHandler 
{
    public string paraName;
    public IconType iconType = IconType.Boolean;
    Animator stateMachine;

    void Start()
    {
        stateMachine = GetComponent<Animator>();
    }

   public void OnPointerDown (PointerEventData eventData) 
	{
		switch (iconType)
        {
            case IconType.Trigger:
                ToggleTrigger(paraName);
                break;
            case IconType.Boolean:
                ToggleBool(paraName);
                break;
            default:
                break;
        }
	}

    public void ToggleBool(string boolName)
    {
        stateMachine.SetBool(boolName, !stateMachine.GetBool(boolName));
        Broadcast<IconType>(EvenDefine.StartMenuUI, iconType);
    }

    public void ToggleTrigger(string triggerName)
    {
        stateMachine.SetTrigger(triggerName);
        Broadcast<IconType>(EvenDefine.StartMenuUI, iconType);
    }
}
