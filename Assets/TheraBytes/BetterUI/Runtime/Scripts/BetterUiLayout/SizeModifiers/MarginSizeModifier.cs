using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class MarginSizeConfigCollection : SizeConfigCollection<MarginSizeModifier> { }

    /// <summary>
    /// This is for RectOffset objects. 
    /// But Unity doesn't like it to play around with RectOffset objects, so we have a wrapper class for it.
    /// </summary>
    [Serializable]
    public class MarginSizeModifier : ScreenDependentSize<Margin>
    {
        public SizeModifierCollection ModLeft;
        public SizeModifierCollection ModRight;
        public SizeModifierCollection ModTop;
        public SizeModifierCollection ModBottom;

        public MarginSizeModifier(Margin optimizedSize, Margin minSize, Margin maxSize)
            : base(optimizedSize, minSize, maxSize, optimizedSize.Clone())
        {
            ModLeft = new SizeModifierCollection(new SizeModifierCollection.SizeModifier(ImpactMode.PixelWidth, 1));
            ModRight = new SizeModifierCollection(new SizeModifierCollection.SizeModifier(ImpactMode.PixelWidth, 1));
            ModTop = new SizeModifierCollection(new SizeModifierCollection.SizeModifier(ImpactMode.PixelWidth, 1));
            ModBottom = new SizeModifierCollection(new SizeModifierCollection.SizeModifier(ImpactMode.PixelWidth, 1));
        }

        public override void DynamicInitialization()
        {
            if (this.value == null)
                this.value = new Margin();
        }

        public override IEnumerable<SizeModifierCollection> GetModifiers()
        {
            yield return ModLeft;
            yield return ModRight;
            yield return ModTop;
            yield return ModBottom;
        }

        protected override void AdjustSize(float factor, SizeModifierCollection mod, int index)
        {
            if (this.value == null)
                this.value = new Margin();

            value[index] = Mathf.RoundToInt(GetSize(factor, OptimizedSize[index], MinSize[index], MaxSize[index]));
        }

        protected override void CalculateOptimizedSize(Margin baseValue, float factor, SizeModifierCollection mod, int index)
        {
            OptimizedSize[index] = Mathf.RoundToInt(factor * baseValue[index]);
        }
    }
}
