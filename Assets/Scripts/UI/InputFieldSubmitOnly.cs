using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class InputFieldSubmitOnly : InputField {
        protected override void Start () {
            base.Start();
 
            for (int i = 0; i < this.onEndEdit.GetPersistentEventCount(); ++i) {
                int index = i; // Local copy for listener delegate
                this.onEndEdit.SetPersistentListenerState(index, UnityEventCallState.Off);
                this.onEndEdit.AddListener(delegate(string text) {
                    if (!EventSystem.current.alreadySelecting)
                    {
                        text = string.IsNullOrEmpty(text) ? "" : text;
                        try
                        {
                            var component = ((Component) this.onEndEdit.GetPersistentTarget(index));
                            component.SendMessage(
                                this.onEndEdit.GetPersistentMethodName(index), text);
                        }
                        catch (Exception e)
                        {
                        
                        }
                    }
                });
            }
        }
    }
}