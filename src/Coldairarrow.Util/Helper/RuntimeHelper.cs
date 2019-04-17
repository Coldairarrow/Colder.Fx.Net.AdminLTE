using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 运行时帮助类
    /// </summary>
    public static class RuntimeHelper
    {
        /// <summary>
        /// 在运行时替换方法
        /// </summary>
        /// <param name="oldMethod">老方法</param>
        /// <param name="newMethod">新方法</param>
        public static void ReplaceMethod(MethodInfo oldMethod, MethodInfo newMethod)
        {
            RuntimeHelpers.PrepareMethod(oldMethod.MethodHandle);
            RuntimeHelpers.PrepareMethod(newMethod.MethodHandle);

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* inj = (int*)newMethod.MethodHandle.Value.ToPointer() + 2;
                    int* tar = (int*)oldMethod.MethodHandle.Value.ToPointer() + 2;
                    if (Debugger.IsAttached)
                    {
                        byte* injInst = (byte*)*inj;
                        byte* tarInst = (byte*)*tar;

                        int* injSrc = (int*)(injInst + 1);
                        int* tarSrc = (int*)(tarInst + 1);

                        *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
                    }
                    else
                    {
                        *tar = *inj;
                    }
                }
                else
                {
                    long* inj = (long*)newMethod.MethodHandle.Value.ToPointer() + 1;
                    long* tar = (long*)oldMethod.MethodHandle.Value.ToPointer() + 1;

                    if (Debugger.IsAttached)
                    {
                        byte* injInst = (byte*)*inj;
                        byte* tarInst = (byte*)*tar;

                        int* injSrc = (int*)(injInst + 1);
                        int* tarSrc = (int*)(tarInst + 1);

                        *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
                    }
                    else
                    {
                        *tar = *inj;
                    }
                }
            }
        }
    }
}
