using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [HelpURL("https://documentation.therabytes.de/better-ui/BetterScrollRect.html")]
    [AddComponentMenu("Better UI/Controls/Better Scroll Rect", 30)]
    public class BetterScrollRect : ScrollRect, IResolutionDependency
    {
#if UNITY_5_5_OR_NEWER
        /// <summary>
        /// Exposes the "m_ContentStartPosition" variable which is used as reference position during drag.
        /// It is a variable of the base ScrollRect class which is not accessible by default. 
        /// Use the setter at your own risk.
        /// </summary>
        public Vector2 DragStartPosition
        {
            get { return base.m_ContentStartPosition; }
            set { base.m_ContentStartPosition = value; }
        }

        /// <summary>
        /// Exposes the "m_ContentBounds" variable which is used to evaluate the size of the content.
        /// It is a variable of the base ScrollRect class which is not accessible by default. 
        /// Use ther setter at your own risk.
        /// </summary>
        public Bounds ContentBounds
        {
            get { return base.m_ContentBounds; }
            set { base.m_ContentBounds = value; }
        }
#endif

        public float HorizontalStartPosition
        {
            get { return this.horizontalStartPosition; }
            set { this.horizontalStartPosition = value; }
        }
        
        public float VerticalStartPosition
        {
            get { return this.verticalStartPosition; }
            set { this.verticalStartPosition = value; }
        }


        public FloatSizeModifier HorizontalSpacingSizer { get { return customHorizontalSpacingSizers.GetCurrentItem(horizontalSpacingFallback); } }
        public FloatSizeModifier VerticalSpacingSizer { get { return customVerticalSpacingSizers.GetCurrentItem(verticalSpacingFallback); } }


        [SerializeField]
        [Range(0, 1)]
        float horizontalStartPosition = 0;

        [SerializeField]
        [Range(0, 1)]
        float verticalStartPosition = 1;

        [SerializeField]
        FloatSizeModifier horizontalSpacingFallback = new FloatSizeModifier(-3, -500, 500);

        [SerializeField]
        FloatSizeConfigCollection customHorizontalSpacingSizers = new FloatSizeConfigCollection();


        [SerializeField]
        FloatSizeModifier verticalSpacingFallback = new FloatSizeModifier(-3, -500, 500);

        [SerializeField]
        FloatSizeConfigCollection customVerticalSpacingSizers = new FloatSizeConfigCollection();

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateSize();
        }

        public void OnResolutionChanged()
        {
            CalculateSize();
        }


        protected override void Start()
        {
            base.Start();

            if(Application.isPlaying)
            {
                ResetToStartPosition();
            }
        }

        public void ResetToStartPosition()
        { 
            if (horizontalScrollbar != null)
            {
                horizontalScrollbar.value = horizontalStartPosition;
            }
            else if (horizontal)
            {
                horizontalNormalizedPosition = horizontalStartPosition;
            }

            if (verticalScrollbar != null)
            {
                verticalScrollbar.value = verticalStartPosition;
            }
            else if (vertical)
            {
                verticalNormalizedPosition = verticalStartPosition;
            }
        }

        private void CalculateSize()
        {
            base.horizontalScrollbarSpacing = HorizontalSpacingSizer.CalculateSize(this);
            base.verticalScrollbarSpacing = VerticalSpacingSizer.CalculateSize(this);
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            CalculateSize();
            base.OnValidate();
        }
#endif
    }
}
