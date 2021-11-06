using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class ValueWizardPageElement<T> : WizardPageElementBase, IWizardDataElement
    {
        Func<ValueWizardPageElement<T>, T, T> drawGuiCallback;
        Action<ValueWizardPageElement<T>> valueChangedCallback;
        T value;

        public T Value { get { return value; } }
        public string SerializationKey { get; }

        public ValueWizardPageElement(string serializationKey, Func<ValueWizardPageElement<T>, T, T> drawGuiCallback, 
            Action<ValueWizardPageElement<T>> valueChangedCallback = null)
        {
            this.SerializationKey = serializationKey;
            this.drawGuiCallback = drawGuiCallback;
            this.valueChangedCallback = valueChangedCallback;
        }

        public override void DrawGui()
        {
            if(drawGuiCallback == null)
            {
                Debug.LogError("No gui callback assigned for wizard element: " + SerializationKey);
                State = WizardElementState.Complete;
                return;
            }
            
            T prev = value;
            value = drawGuiCallback(this, value);

            if(value != null && !value.Equals(prev) && valueChangedCallback != null)
            {
                valueChangedCallback(this);
            }
        }

        public string GetValueAsString()
        {
            return ParseHelper.ToParsableString(Value);
        }

        public bool TrySetValue(string input)
        {
            return ParseHelper.TryParse(input, out value);
        }
    }
}
