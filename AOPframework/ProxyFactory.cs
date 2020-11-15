using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AOPframework
{
    /// <summary>
    /// 代理工厂
    /// </summary>
    public class ProxyFactory
    {
        /// <summary>
        /// 返回代理的类型
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="interfaceType">源类型实现的接口(规范)</param>
        /// <returns>代理类型</returns>
        public static Type ProxyTypeBuilder(Type sourceType, Type interfaceType)
        {
            TypeVerify(sourceType, interfaceType);

            TypeBuilder defClassBuilder = CreateDynamicTypeBuild(sourceType.Name + "Proxy");
            
            defClassBuilder.AddInterfaceImplementation(interfaceType);
            
            #region 编写字段 代理字段

            FieldBuilder _testfb = defClassBuilder.DefineField("_" + sourceType.Name, 
                interfaceType, FieldAttributes.Private );

            #endregion

            #region 编写构造 注入模式
           
            ConstructorInfo objCtor = typeof(object).GetConstructor(new Type[0]);

            ConstructorBuilder ConstrBld_1 = defClassBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { interfaceType }
                );

            var ctorIL = ConstrBld_1.GetILGenerator();
            //先调用下父类Object的构造
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, objCtor);
            ctorIL.Emit(OpCodes.Ldarg_0);//this
            ctorIL.Emit(OpCodes.Ldarg_1);//
            ctorIL.Emit(OpCodes.Stfld, _testfb);
            ctorIL.Emit(OpCodes.Ret); //函数返回
            #endregion

            #region 定义方法
            var Methods = interfaceType.GetMethods();
            //排除obj基类方法
            var methods = Methods.Where(m => m.Name != "GetHashCode" || m.Name != "Equals" || m.Name != "ToString" || m.Name != "ReferenceEquals");

            foreach (var method in methods)
            {
                //定义一个方法
                var parameterTypes = method.GetParameters().Select(o => o.ParameterType).ToArray();
                var methodBldr = defClassBuilder.DefineMethod(method.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual |MethodAttributes.Final,
                    method.ReturnType,
                    parameterTypes);
                var souceMethod=sourceType.GetMethod(method.Name);
                ILGenerator _ILmethod = methodBldr.GetILGenerator();
                if (method.ReturnType == typeof(void))
                {
                    CreateVoidMethod(_ILmethod, method, souceMethod,_testfb);
                }
                else
                {
                    CreateRetMethod(_ILmethod, method, souceMethod,_testfb);
                }
                //实现接口
                defClassBuilder.DefineMethodOverride(methodBldr, method);
            }

            #endregion

            Type t = defClassBuilder.CreateType();
            assemblyBuilder.Save("MyClass.dll");
            return t;
        }
        /// <summary>
        /// 编织创建有返回值的方法
        /// </summary>
        /// <param name="methodIL">方法指令器</param>
        /// <param name="proxyMethod">代理的方法</param>
        /// <param name="parameterTypes">方法的参数</param>
        /// <param name="_testfb"></param>
        private static void CreateRetMethod(ILGenerator methodIL,MethodInfo proxyMethod, MethodInfo souceMethod, FieldBuilder _testfb)
        {
            Type[] parameterTypes = proxyMethod.GetParameters().Select(o => o.ParameterType).ToArray();
            callMethodAfter(methodIL,proxyMethod);
            #region 调用方法等同于  ReturnType result= method(arg1,arg2,....);
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, _testfb);
            for (short i = 0; i < parameterTypes.Length; i++)
            {
                methodIL.Emit(OpCodes.Ldarg, i + 1);
            }
            LocalBuilder returnType = methodIL.DeclareLocal(proxyMethod.ReturnType);
            methodIL.Emit(OpCodes.Callvirt, proxyMethod);
            methodIL.Emit(OpCodes.Stloc, returnType);//赋值语句
            #endregion
            callMethodBefore(methodIL, _testfb, proxyMethod);
            methodIL.Emit(OpCodes.Ldloc, returnType);//加载局部变量
            methodIL.Emit(OpCodes.Ret);//函数返回
        }
        /// <summary>
        /// 编织创建无返回值的方法
        /// </summary>
        /// <param name="methodIL">方法指令器</param>
        /// <param name="proxyMethod">代理的方法</param>
        /// <param name="parameterTypes">方法的参数</param>
        /// <param name="_testfb"></param>
        private static void CreateVoidMethod(ILGenerator methodIL, MethodInfo proxyMethod, MethodInfo souceMethod, FieldBuilder _testfb)
        {
            Type[] parameterTypes = proxyMethod.GetParameters().Select(o => o.ParameterType).ToArray();
            var _try = methodIL.BeginExceptionBlock();

            callMethodAfter(methodIL, souceMethod);

            #region 调用方法等同于  ReturnType result= method(arg1,arg2,....);
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, _testfb);
            for (short i = 0; i < parameterTypes.Length; i++)
            {
                methodIL.Emit(OpCodes.Ldarg, i + 1);
            }
            methodIL.Emit(OpCodes.Callvirt, proxyMethod);
            #endregion
            
            callMethodBefore(methodIL, _testfb, souceMethod);
            methodIL.BeginCatchBlock(typeof(Exception));
            methodIL.EmitWriteLine("异常");
            methodIL.BeginFinallyBlock();
            methodIL.EmitWriteLine("一定");
            methodIL.EndExceptionBlock(); 
            methodIL.Emit(OpCodes.Ret);
        }
        /// <summary>
        /// 编织调用方法之后
        /// </summary>
        /// <param name="_ILmethod"></param>
        /// <param name="proxyMethod">代理的方法</param>
        private static void callMethodAfter(ILGenerator _ILmethod,MethodInfo proxyMethod)
        {
            var proxyAttribute = proxyMethod.GetCustomAttribute<ProxyAttribute>();
            #region  调用方法之后
            if (proxyAttribute != null)
            {
                _ILmethod.Emit(OpCodes.Newobj, proxyAttribute.GetType().GetConstructor(new Type[0]));
                _ILmethod.Emit(OpCodes.Call, proxyAttribute.GetType().GetMethod("AfterCall", new Type[0]));
            }
            #endregion
        }
        /// <summary>
        /// 编织调用方法之前
        /// </summary>
        /// <param name="_ILmethod"></param>
        /// <param name="proxyMethod">代理的方法</param>
        private static void callMethodBefore(ILGenerator _ILmethod, FieldBuilder _field, MethodInfo proxyMethod)
        {

            //var t1 = type.GetCustomAttribute<ProxyAttribute>();
            proxyMethod.GetCustomAttribute<ProxyAttribute>();
            Label lbTrue = _ILmethod.DefineLabel();
            Label lbRet = _ILmethod.DefineLabel();

            _ILmethod.Emit(OpCodes.Ldarg_0);
            _ILmethod.Emit(OpCodes.Ldfld, _field);
            _ILmethod.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType", new Type[0]));
             LocalBuilder _type = _ILmethod.DeclareLocal(typeof(Type));
            _ILmethod.Emit(OpCodes.Stloc, _type);//赋值操作
            _ILmethod.Emit(OpCodes.Ldloc, _type);
          
           
            //_ILmethod.Emit(OpCodes.Ldtoken, typeof(ProxyAttribute));
            //_ILmethod.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle",new Type[] { typeof(RuntimeTypeHandle)}));
            // var me = typeof(CustomAttributeExtensions).GetMethod("GetCustomAttribute", new Type[] { typeof(MemberInfo), typeof(Type) });
            //_ILmethod.Emit(OpCodes.Call, me);
            var me = typeof(CustomAttributeExtensions).GetMethod("GetCustomAttribute", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Standard, new Type[] { typeof(MemberInfo) }, null);
            var me1 = me.MakeGenericMethod(typeof(ProxyAttribute));
            _ILmethod.Emit(OpCodes.Call, me1);
          
             LocalBuilder _t1 = _ILmethod.DeclareLocal(typeof(ProxyAttribute));
            _ILmethod.Emit(OpCodes.Stloc, _t1);//赋值操作
            _ILmethod.Emit(OpCodes.Ldloc, _t1);
            _ILmethod.Emit(OpCodes.Brfalse, lbRet);
            //调用器方法
            _ILmethod.Emit(OpCodes.Ldloc, _t1);
            _ILmethod.Emit(OpCodes.Callvirt, typeof(ProxyAttribute).GetMethod("AfterCall", new Type[0]));
            _ILmethod.MarkLabel(lbRet);
            
            //var proxyAttribute = proxyMethod.GetCustomAttribute<ProxyAttribute>();
            //#region  调用方法之前 
            //if (proxyAttribute != null)
            //{
            //    _ILmethod.Emit(OpCodes.Newobj, proxyAttribute.GetType().GetConstructor(new Type[0]));
            //    _ILmethod.Emit(OpCodes.Call, proxyAttribute.GetType().GetMethod("AfterCall", new Type[0]));
            //}
            //#endregion
        }

        /// <summary>Creates new method.</summary>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="defClassBuilder">The definition class builder.</param>
        /// <param name="_testfb">The testfb.</param>
        private static void CreateDefaultConstruct(Type sourceType, TypeBuilder defClassBuilder, FieldBuilder _testfb)
        {
            #region 编写构造 默认模式

            var constructor = sourceType.GetConstructors()[0];
            var sourceTypeCP = constructor.GetParameters().Select(o => o.ParameterType);
            var constructorParameters = new List<Type>();
            constructorParameters.AddRange(sourceTypeCP);
            ConstructorBuilder dfltConstrBld = defClassBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                constructorParameters.ToArray()
                );
            var constructorIL = dfltConstrBld.GetILGenerator();

            LocalBuilder _obj = constructorIL.DeclareLocal(sourceType);

            for (short i = 0; i < constructorParameters.Count; i++)
            {
                constructorIL.Emit(OpCodes.Ldarg, i + 1);
            }
            constructorIL.Emit(OpCodes.Newobj, sourceType.GetConstructor(sourceTypeCP.ToArray()));
            constructorIL.Emit(OpCodes.Stloc, _obj);//局部变量赋值


            constructorIL.Emit(OpCodes.Ldarg_0);//this
            constructorIL.Emit(OpCodes.Ldloc, _obj);//局部变量入栈
            constructorIL.Emit(OpCodes.Stfld, _testfb);

            constructorIL.Emit(OpCodes.Ret); //函数返回
            #endregion
        }

        /// <summary>
        /// 验证合法性
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="interfaceType"></param>
        private static void TypeVerify(Type sourceType, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new Exception("不是接口类型");
            }
            if (!sourceType.IsClass)
            {
                throw new Exception("不是Class类型");
            }
            var type1 = sourceType.GetInterface(interfaceType.Name, true);
            if (type1 == null)
            {
                throw new Exception("没有实现接口");
            }
        }

        /// <summary>
        /// 创建一个动态类型的build
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static TypeBuilder CreateDynamicTypeBuild(string className)
        {
            if (defModuleBuilder == null)
            {
                //首先就需要定义一个程序集
                assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
                //定义一个构建模块
                defModuleBuilder = assemblyBuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");
            }
            //定义类
            var defClassBuilder = defModuleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class);
            return defClassBuilder;
        }

        //定义一个程序集的名称
        private static AssemblyName asmName = new AssemblyName("MyClass");
        private static ModuleBuilder defModuleBuilder;
        private static AssemblyBuilder assemblyBuilder;
    }
}
