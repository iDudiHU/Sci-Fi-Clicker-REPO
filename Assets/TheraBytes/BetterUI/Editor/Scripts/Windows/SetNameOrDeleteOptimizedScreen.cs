using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheraBytes.BetterUi.Editor
{
    public class SetNameOrDeleteOptimizedScreen : PopupWindowContent
    {
        const float MARGIN = 10;

        bool deleteConfirm = false;

        bool renameMode;
        ScreenTypeConditions condition;
        string cachedName;

        public event Action CloseCallback;


        public SetNameOrDeleteOptimizedScreen(ScreenTypeConditions condition, Action closeCallback)
        {
            this.renameMode = condition != null;
            this.condition = condition;
            this.cachedName = (renameMode) ? condition.Name : "";
            this.CloseCallback = closeCallback;
        }


        public override Vector2 GetWindowSize()
        {
            return new Vector2(400, (renameMode) ? 300 : 175);
        }

        public override void OnGUI(Rect rect)
        {
            Rect inner = new Rect(rect.x + MARGIN, rect.y + MARGIN, rect.width - 2 * MARGIN, rect.height - 2 * MARGIN);

            float y = inner.y;
            float h = 20;

            EditorGUI.LabelField(new Rect(inner.x, y, inner.width - 15, h),
                (renameMode) ? "Rename or Delete" : "Create", EditorStyles.boldLabel);

            if(GUI.Button(new Rect(inner.x + inner.width - h, y, h, h), "X"))
            {
                if (CloseCallback != null)
                    CloseCallback();
            }

            y += h;

            if (renameMode)
            {
                h = 100;

                EditorGUI.LabelField(new Rect(inner.x, y, inner.width, h),
                @"Attention!
Settings for certain screen conditions are mapped by name.
If you change the name, your already configured UI Elements
may not look as expected.
You should only rename if there are no UI Elements connected
to this configuration yet or if you are willing 
to adjust it everywhere.");

                y += h;
                h = 16;

                cachedName = EditorGUI.TextField(new Rect(inner.x, y, inner.width, h), "New Name", cachedName);

                y += h + 10;
                h = 20;

                if (CheckNameValidity())
                {
                    if (GUI.Button(new Rect(inner.x, y, inner.width, h), "Rename"))
                    {
                        condition.Name = cachedName;

                        if (CloseCallback != null)
                            CloseCallback();
                    }
                }
                else
                {
                    EditorGUI.LabelField(new Rect(inner.x, y, inner.width, h), "Name is not valid", EditorStyles.helpBox);
                }


                y += h + 20;

                if (deleteConfirm)
                {
                    h = 40;
                    EditorGUI.LabelField(new Rect(inner.x, y, inner.width, h),
                   string.Format(@"Are you sure you want to delete the screen condition
'{0}'?.", condition.Name));

                    y += h;
                    h = 20;

                    if (GUI.Button(new Rect(inner.x, y, 0.5f * inner.width - 4, h), "Yes"))
                    {
                        ResolutionMonitor.Instance.OptimizedScreens.Remove(condition);

                        if (CloseCallback != null)
                            CloseCallback();
                    }

                    if (GUI.Button(new Rect(inner.x + 0.5f * inner.width + 4, y, 0.5f * inner.width - 4, h), "No"))
                    {
                        deleteConfirm = false;
                    }
                }
                else
                {
                    h = 60;
                    EditorGUI.LabelField(new Rect(inner.x, y, inner.width, h),
                   @"You May also delete this screen condition 
but the mapping to its name will stay connected
to the controls which references it.");

                    y += h;
                    h = 20;

                    if (GUI.Button(new Rect(inner.x, y, inner.width, h), string.Format("Delete '{0}'", condition.Name)))
                    {
                        deleteConfirm = true;
                    }
                }

            }
            else // CREATE MODE
            {
                h = 80;

                EditorGUI.LabelField(new Rect(inner.x, y, inner.width, h),
                @"Give your new Condition a short and clear name.

Choose the name wisely. It identifies your condition.
When it is in use in the project you should not change it anymore.");

                y += h;
                h = 16;

                cachedName = EditorGUI.TextField(new Rect(inner.x, y, inner.width, h), "Name", cachedName);

                y += h + 10;
                h = 20;

                if (CheckNameValidity())
                {
                    if (GUI.Button(new Rect(inner.x, y, inner.width, h), "Create"))
                    {
                        condition = new ScreenTypeConditions(cachedName);
                        ResolutionMonitor.Instance.OptimizedScreens.Add(condition);

                        if (CloseCallback != null)
                            CloseCallback();
                    }
                }
                else
                {
                    EditorGUI.LabelField(new Rect(inner.x, y, inner.width, h), "Name is not valid", EditorStyles.helpBox);
                }
            }
        }

        bool CheckNameValidity()
        {
            return !(string.IsNullOrEmpty(cachedName))
                && (ResolutionMonitor.Instance.OptimizedScreens.FirstOrDefault(o => o != condition && o.Name == cachedName) == null);
        }
    }
}
