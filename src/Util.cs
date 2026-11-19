using System.Reflection.Emit;
using UnityEngine.SceneManagement;

namespace UnityExplorer
{
    internal static class Util
    {
        private delegate int SceneHandleGetter(Scene scene);
        private delegate void SceneHandleSetter(ref Scene scene, int handle);

        // Generated once so that getting/setting a scene handle doesn't box or go through reflection.
        // Scene.m_Handle is an int before Unity 6 and a SceneHandle struct (convertible to/from int) from Unity 6 onwards.
        private static readonly SceneHandleGetter getHandle;
        private static readonly SceneHandleSetter setHandle;

        static Util()
        {
            // Private in Unity's own assemblies, but public in Il2CppInterop/Unhollower-generated ones.
            FieldInfo field = typeof(Scene).GetField(nameof(Scene.m_Handle), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new MissingFieldException(typeof(Scene).FullName, nameof(Scene.m_Handle));
            Type handleType = field.FieldType;
            bool needsConversion = handleType != typeof(int);

            // int GetHandle(Scene scene) => (int)scene.m_Handle;
            DynamicMethod getter = new("Scene_GetHandle", typeof(int), new[] { typeof(Scene) }, typeof(Util).Module, true);
            ILGenerator il = getter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            if (needsConversion)
                il.Emit(OpCodes.Call, FindImplicitConversion(handleType, handleType, typeof(int)));
            il.Emit(OpCodes.Ret);
            getHandle = (SceneHandleGetter)getter.CreateDelegate(typeof(SceneHandleGetter));

            // void SetHandle(ref Scene scene, int handle) => scene.m_Handle = (SceneHandle)handle;
            DynamicMethod setter = new("Scene_SetHandle", typeof(void), new[] { typeof(Scene).MakeByRefType(), typeof(int) }, typeof(Util).Module, true);
            il = setter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            if (needsConversion)
                il.Emit(OpCodes.Call, FindImplicitConversion(handleType, typeof(int), handleType));
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);
            setHandle = (SceneHandleSetter)setter.CreateDelegate(typeof(SceneHandleSetter));
        }

        private static MethodInfo FindImplicitConversion(Type declaringType, Type from, Type to)
            => declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "op_Implicit" && m.ReturnType == to && m.GetParameters()[0].ParameterType == from);

        /// <summary>
        /// Converts an int scene handle to the given parameter type (int, or SceneHandle on Unity 6+).
        /// Only used for a reflection call at startup, so returning a boxed value is fine here.
        /// </summary>
        internal static object ToSceneHandleArgument(Type parameterType, int handle)
        {
            if (parameterType == typeof(int))
                return handle;

            return FindImplicitConversion(parameterType, typeof(int), parameterType).Invoke(null, new object[] { handle });
        }

        internal static int GetHandle(this Scene scene) => getHandle(scene);

        internal static void SetHandle(this ref Scene scene, int handle) => setHandle(ref scene, handle);
    }
}
