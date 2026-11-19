using UnityExplorer.Inspectors;

namespace UnityExplorer.CacheObject
{
    public class CacheProperty : CacheMember
    {
        public PropertyInfo PropertyInfo { get; internal set; }
        public override Type DeclaringType => PropertyInfo.DeclaringType;
        public override bool CanWrite => PropertyInfo.CanWrite;
        public override bool IsStatic => m_isStatic ?? (bool)(m_isStatic = PropertyInfo.GetAccessors(true)[0].IsStatic);
        private bool? m_isStatic;

        public override bool ShouldAutoEvaluate => !HasArguments && !isManualEvaluate;

        /// <summary>True if reading this property is known to have a side effect, so it requires the user to click Evaluate.</summary>
        private readonly bool isManualEvaluate;

        public CacheProperty(PropertyInfo pi)
        {
            this.PropertyInfo = pi;
            this.isManualEvaluate = ManualEvaluateProperties.IsManualEvaluate(pi);
        }

        public override void SetInspectorOwner(ReflectionInspector inspector, MemberInfo member)
        {
            base.SetInspectorOwner(inspector, member);

            Arguments = PropertyInfo.GetIndexParameters();
        }

        protected override object TryEvaluate()
        {
            try
            {
                object ret;
                if (HasArguments)
                    ret = PropertyInfo.GetValue(DeclaringInstance, this.Evaluator.TryParseArguments());
                else
                    ret = PropertyInfo.GetValue(DeclaringInstance, null);
                LastException = null;
                return ret;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return null;
            }
        }

        protected override void TrySetValue(object value)
        {
            if (!CanWrite)
                return;

            try
            {
                bool _static = PropertyInfo.GetAccessors(true)[0].IsStatic;

                if (HasArguments)
                    PropertyInfo.SetValue(DeclaringInstance, value, Evaluator.TryParseArguments());
                else
                    PropertyInfo.SetValue(DeclaringInstance, value, null);
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning(ex);
            }
        }
    }
}
