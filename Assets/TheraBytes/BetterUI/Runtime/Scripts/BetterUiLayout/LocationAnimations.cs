using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649 // disable "never assigned" warnings
namespace TheraBytes.BetterUi
{
    [HelpURL("https://documentation.therabytes.de/better-ui/LocationAnimations.html")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Better UI/Animation/Location Animations", 30)]
    public class LocationAnimations : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        public class LocationAnimationEvent : UnityEvent
        {
            public LocationAnimationEvent() { }

            public LocationAnimationEvent(params UnityAction[] actions)
            {
                foreach (UnityAction act in actions)
                {
                    this.AddListener(act);
                }
            }
        }

        [Serializable]
        public class LocationAnimationUpdateEvent : UnityEvent<float>
        {
            public LocationAnimationUpdateEvent() { }

            public LocationAnimationUpdateEvent(params UnityAction<float>[] actions)
            {
                foreach (UnityAction<float> act in actions)
                {
                    this.AddListener(act);
                }
            }
        }

        [Serializable]
        public class RectTransformDataConfigCollection : SizeConfigCollection<RectTransformData> { }

        [Serializable]
        public class LocationData
        {
            [SerializeField]
            string name;

            [SerializeField]
            RectTransformData transformFallback = new RectTransformData();

            [SerializeField]
            RectTransformDataConfigCollection transformConfigs = new RectTransformDataConfigCollection();

            public string Name { get { return name; } internal set { name = value; } }
            public RectTransformData CurrentTransformData { get { return transformConfigs.GetCurrentItem(transformFallback); } }
        }

        [Serializable]
        public class Animation
        {
            [SerializeField]
            string name;

            [SerializeField]
            string from;

            [SerializeField]
            string to;

            [SerializeField]
            AnimationCurve curve;

            [SerializeField]
            LocationAnimationEvent actionBeforeStart = new LocationAnimationEvent();

            [SerializeField]
            LocationAnimationEvent actionAfterFinish = new LocationAnimationEvent();

            [SerializeField]
            LocationAnimationUpdateEvent actionOnUpdating = new LocationAnimationUpdateEvent();

            [SerializeField]
            bool animateWithEulerRotation = true;

            [SerializeField]
            float timeScale = 1;

            public string Name { get { return name; } internal set { name = value; } }
            public string From { get { return from; } set { from = value; } }
            public string To { get { return to; } set { to = value; } }

            // advanced
            public AnimationCurve Curve { get { return curve; } internal set { curve = value; } }
            public bool AnimateWithEulerRotation { get { return animateWithEulerRotation; } set { animateWithEulerRotation = value; } }
            public float TimeScale { get { return timeScale; } set { timeScale = value; } }
            public LocationAnimationEvent ActionBeforeStart { get { return actionBeforeStart; } }
            public LocationAnimationEvent ActionAfterFinish { get { return actionAfterFinish; } }
            public LocationAnimationUpdateEvent ActionOnUpdating { get { return actionOnUpdating; } }
        }

        [Serializable]
        public class AnimationState
        {
            public Animation Animation { get; internal set; }
            public RectTransformData From { get; internal set; }
            public RectTransformData To { get; internal set; }
            public float Time { get; set; }
            public float Duration { get; set; }
            public bool Loop { get; internal set; }
            public float TimeScale { get; set; }
            public LocationAnimationEvent ActionAfterFinish { get; internal set; }
        }

        #endregion

        public RectTransform RectTransform { get { return this.transform as RectTransform; } }

        public List<LocationData> Locations { get { return locations; } }
        public List<Animation> Animations { get { return animations; } }
        public string StartUpAnimation { get { return startUpAnimation; } set { startUpAnimation = value; } }
        public string StartLocation { get { return startLocation; } set { startLocation = value; } }
        public bool IsAnimating { get { return runningAnimation != null; } }
        public bool UseRelativeLocations
        {
            get { return useRelativeLocations; }
#if UNITY_EDITOR
            set { useRelativeLocations = value; }
#endif
        }

        [SerializeField]
        bool useRelativeLocations;

        [SerializeField]
        List<LocationData> locations = new List<LocationData>();

        [SerializeField]
        List<Animation> animations = new List<Animation>();


        [SerializeField]
        string startLocation;

        [SerializeField]
        string startUpAnimation;

        [SerializeField]
        LocationAnimationEvent actionOnInit;

        RectTransformData referenceLocation;
        AnimationState runningAnimation;

        public AnimationState RunningAnimation { get { return runningAnimation; } }
        public RectTransformData ReferenceLocation { get { EnsureReferenceLocation(true); return referenceLocation; } }

        private void Start()
        {
            ResetReferenceLocation();
            SetToLocation(startLocation);

            actionOnInit.Invoke();

            StartAnimation(startUpAnimation);
        }

        public void StopCurrentAnimation()
        {
            runningAnimation = null;
        }

        public void StartAnimation(string name) { StartAnimation(GetAnimation(name), null, null); }
        public void StartAnimation(string name, float timeScale) { StartAnimation(GetAnimation(name), timeScale, null); }
        public void StartAnimation(string name, LocationAnimationEvent onFinish) { StartAnimation(GetAnimation(name), null, onFinish); }
        public void StartAnimation(string name, float timeScale, LocationAnimationEvent onFinish) { StartAnimation(GetAnimation(name), timeScale, onFinish); }

        public void StartAnimation(Animation ani, float? timeScale, LocationAnimationEvent onFinish)
        {
            if (ani == null || ani.To == null || (runningAnimation != null && ani == runningAnimation.Animation))
                return;

            if (runningAnimation != null)
            {
                StopCurrentAnimation();
            }

            if (ani.Curve == null || ani.Curve.keys.Length <= 1)
            {
                SetToLocation(ani.To);
                return;
            }

            float speed = timeScale ?? ani.TimeScale;

            bool forever =
                (speed > 0
                    && (ani.Curve.postWrapMode == WrapMode.Loop
                    || ani.Curve.postWrapMode == WrapMode.PingPong))
                || (speed < 0
                    && (ani.Curve.preWrapMode == WrapMode.Loop
                    || ani.Curve.preWrapMode == WrapMode.PingPong));

            runningAnimation = new AnimationState()
            {
                Animation = ani,
                From = GetLocationTransformFallbackCurrent(ani.From),
                To = GetLocationTransformFallbackCurrent(ani.To),
                ActionAfterFinish = onFinish ?? ani.ActionAfterFinish,
                Duration = ani.Curve.keys[ani.Curve.keys.Length - 1].time,
                Loop = forever,
                TimeScale = speed,
                Time = 0,
            };

            ani.ActionBeforeStart.Invoke();
        }

        private void Update()
        {
            UpdateCurrentAnimation(Time.unscaledDeltaTime);
        }

        public void UpdateCurrentAnimation(float deltaTime)
        {
            if (runningAnimation == null || runningAnimation.Animation == null ||
                runningAnimation.Animation.Curve == null || runningAnimation.Animation.Curve.length == 0)
                return;

            bool animationTimeIsOver = (!runningAnimation.Loop && runningAnimation.Time >= runningAnimation.Duration);

            if (animationTimeIsOver)
                runningAnimation.Time = runningAnimation.Duration;

            float amount = runningAnimation.Animation.Curve.Evaluate(runningAnimation.Time);
            var rtd = RectTransformData.LerpUnclamped(runningAnimation.From, runningAnimation.To, amount, runningAnimation.Animation.AnimateWithEulerRotation);
            rtd.PushToTransform(RectTransform);

            runningAnimation.Animation.ActionOnUpdating.Invoke(amount);

            runningAnimation.Time += deltaTime * runningAnimation.TimeScale;
            if (animationTimeIsOver)
            {
                runningAnimation.ActionAfterFinish.Invoke();
                runningAnimation = null;
            }
        }

        public void SetToLocation(string name)
        {
            LocationData loc = GetLocation(name);
            if (loc == null)
                return;

            PushTransformData(loc);
        }

        public LocationData GetLocation(string name)
        {
            return locations.FirstOrDefault(o => o.Name == name);
        }

        public void ResetReferenceLocation() { ResetReferenceLocation(this.RectTransform); }
        public void ResetReferenceLocation(RectTransform rectTransform) { ResetReferenceLocation(new RectTransformData(rectTransform)); }
        public void ResetReferenceLocation(RectTransformData reference) { referenceLocation = reference; }

        void PushTransformData(LocationData loc)
        {
            if (useRelativeLocations)
            {
                EnsureReferenceLocation();

                var cur = loc.CurrentTransformData;
                var transformData = RectTransformData.Combine(cur, referenceLocation);
                transformData.PushToTransform(RectTransform);
            }
            else
            {
                loc.CurrentTransformData.PushToTransform(RectTransform);
            }
        }

        private void EnsureReferenceLocation(bool force = false)
        {
            if (referenceLocation == null
                || ((force || useRelativeLocations) && referenceLocation == RectTransformData.Invalid))
            {
                ResetReferenceLocation();
            }
        }

        private RectTransformData GetLocationTransformFallbackCurrent(string name)
        {
            EnsureReferenceLocation();

            var loc = locations.FirstOrDefault(o => o.Name == name);
            RectTransformData cur = (loc == null)
                ? new RectTransformData(RectTransform)
                : loc.CurrentTransformData;

            RectTransformData result = (useRelativeLocations && loc != null)
                ? RectTransformData.Combine(cur, referenceLocation)
                : cur;

            result.SaveRotationAsEuler = true;

            return result;
        }

        public Animation GetAnimation(string name)
        {
            return animations.FirstOrDefault(o => o.Name == name);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            for (int i = 0; i < animations.Count; i++)
            {
                Animation ani = animations[i];

                if (ani.Curve == null || ani.Curve.keys.Length < 2)
                {
                    ani.Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
                }
            }

            for (int i = 0; i < locations.Count; i++)
            {
                LocationData loc = locations[i];
                var cur = loc.CurrentTransformData;

                if (cur == RectTransformData.Invalid)
                {
                    cur.PullFromTransform(RectTransform);

                    if (useRelativeLocations)
                    {
                        EnsureReferenceLocation();
                        cur.PullFromData(RectTransformData.Separate(cur, referenceLocation));
                    }
                }
            }

        }
#endif
    }
}

#pragma warning restore 0649
