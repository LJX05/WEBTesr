
using Microsoft.VisualBasic;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOPframework
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        static void Main()
        {
            var assembly = System.IO.File.ReadAllBytes(@"D:\检测核心工作\图像审核软件\ChaYanApp\Run\bin\Debug\Run.dll");
            var assembly1=Assembly.Load(assembly);
            var  assemblies= AppDomain.CurrentDomain.GetAssemblies();
           // Activator.CreateInstance();
           dynamic obj= assembly1.CreateInstance("Run.RunTest", true);
            obj.Main();
            IList<int> list=new List<int> ();
            var agreement = new ReadWriteAgreement<int>(list);
            for (int k=0;k<1000;k++) { 
                Task.Run(() =>
                {
                    agreement.Write((source) => source.Add(2)); });
                Task.Run(() =>
                {
                    agreement.Read((source) =>
                    {
                        return source.ToList();
                    });

                });
            }
            Console.WriteLine("完毕");
            Console.ReadKey();
            //TestProxy
            //new ServiceReference1.TmriOutAccessSoapClient();
            //var req = new CheckAppearanceRequestEntity();
            //var ss = req.GetType().GetProperties().Where(p => p.PropertyType.Equals(typeof(byte[]))).Select(o => o.Name).ToArray();
            //Expression1();
            //var type = ProxyFactory.ProxyTypeBuilder(typeof(Test), typeof(InterfaceTest));
            //var time1 = Environment.TickCount;

            //InterfaceTest obj = (InterfaceTest)Activator.CreateInstance(type, new Test(3));

            //obj.add1(15, 46, "999999");
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("");
            //request.GetResponseAsync();

            var headFaucet = new HeadFaucet();
            var dd = new Handle();
            var dd1 = new Handle2();
            var ff = new Pteris();
            headFaucet.init(dd).assemble(dd1).assemble(ff);
            var random = new Random(1000);
            while (true)
            {

                var i = random.Next(1, 100);
                if (i == 99)
                {
                    Console.WriteLine("等待99");
                    Thread.Sleep(i * 30);
                }
                headFaucet.Eat(i);
            }


        }
        /// <summary>
        /// 龙头
        /// </summary>
        public class HeadFaucet
        {

            private IHandle _next;

            public HeadFaucet()
            {

            }
            public IHandle init(IHandle next)
            {

                _next = next;
                return _next;
            }

            /// <summary>
            /// 吃
            /// </summary>
            /// <param name="obj"></param>
            public void Eat(object obj)
            {
                _next.Eat(obj);
            }
        }
        public class Handle2 : IHandle
        {

            private IHandle _next;

            private  Queue<object> storage = new Queue<object>();

            public Handle2()
            {

            }

            private bool Check(object obj)
            {
                if (obj is int)
                {
                    if ((int)obj % 3 == 0)
                    {
                        return true;
                    }

                }
                return false;
            }

            public void Eat(object obj)
            {
                //如果可以吃的化
                if (Check(obj))
                {
                    Console.WriteLine("交给了2号处理器");
                    //就要处理了
                    Task.Run(() =>
                    {
                        var random = new Random(1000);
                        var i = random.Next(50, 500);
                        Thread.Sleep(i * 2);
                        Console.WriteLine("我是2号,以处理完毕");
                    });
                }
                else
                {
                    _next.Eat(obj);
                }
            }

            public IHandle assemble(IHandle _next)
            {
                this._next = _next;
                return this._next;
            }
        }
        public class Handle : IHandle, IRun
        {

            private IHandle _next;

            private Queue<object> storage = new Queue<object>();

            private bool Check(object obj)
            {
                if (obj is int)
                {
                    if ((int)obj % 9 == 0)
                    {
                        return true;
                    }

                }
                return false;
            }

            public void Eat(object obj)
            {
                //如果可以吃的化
                if (Check(obj))
                {
                    Console.WriteLine("交给了1号处理器");
                    Add(obj);
                }
                else
                {
                    _next.Eat(obj);
                }
            }

            public IHandle assemble(IHandle _next)
            {
                this._next = _next;
                Run();
                return this._next;
            }

            public void Add(object obj)
            {
                storage.Enqueue(obj);
            }

            public void Run()
            {
                var obj = storage.Dequeue();
                if (obj == null)
                {
                    return;
                }
                //就要处理了
                var task = Task.Run(() =>
                  {
                      var _obj = obj;
                      var random = new Random(1000);
                      var i = random.Next(50, 500);
                      Thread.Sleep(i * 2);
                      Console.WriteLine("我是1号,以处理完毕");
                  });
                task.ContinueWith((after) =>
                {
                    //var _obj = storage.Dequeue();
                    //_obj = null;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }
        /// <summary>
        /// 尾
        /// </summary>
        public class Pteris : IHandle
        {
            public IHandle assemble(IHandle _next)
            {
                throw new NotImplementedException();
            }

            public void Eat(object obj)
            {
                Console.WriteLine("无人处理obj,释放obj");
                return;
            }
        }

        public interface IHandle
        {
            IHandle assemble(IHandle _next);

            void Eat(object obj);
        }

        public interface IRun
        {
            void Run();
        }
        
        private static async Task NewMethod()
        {
            var task1 = Task.Run(() =>
            {
                Console.WriteLine("task1--" + Thread.CurrentThread.ManagedThreadId);
            });
            await task1;

            Console.WriteLine("await1--" + Thread.CurrentThread.ManagedThreadId);
            var task2 = task1.ContinueWith((bref) =>
            {

                Console.WriteLine("task2--" + Thread.CurrentThread.ManagedThreadId);
            });
            await task2;
            Console.WriteLine("await2--" + Thread.CurrentThread.ManagedThreadId);

        }

        public static void Expression1()
        {
            var t1 = CustomAttributeExtensions.GetCustomAttribute(typeof(ProxyAttribute), typeof(ProxyAttribute));
            // Add the following directive to your file:
            // using System.Linq.Expressions;  

            // The block expression allows for executing several expressions sequentually.
            // When the block expression is executed,
            // it returns the value of the last expression in the sequence.
            BlockExpression blockExpr = Expression.Block(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("Write", new Type[] { typeof(String) }),
                    Expression.Constant("Hello ")
                   ),
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("World!")
                    ),
                Expression.Constant(42)
            );
            var mehoth = Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("World!")
                    );

            Console.WriteLine("The result of executing the expression tree:");
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.  

            var func = Expression.Lambda<Func<int>>(blockExpr).Compile();
            var result = func();
            var ss = blockExpr.ToString();
            // Print out the expressions from the block expression.
            Console.WriteLine("The expressions from the block expression:");
            foreach (var expr in blockExpr.Expressions)
                Console.WriteLine(expr.ToString());
            // Print out the result of the tree execution.
            Console.WriteLine("The return value of the block expression:");
            Console.WriteLine(result);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="interfaceType">源类型实现的接口(规范)</param>
        /// <returns>代理类型</returns>
        public static Type ProxyTypeBuilder(Type sourceType, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new Exception("不是接口类型");
            }

            if (!sourceType.IsClass)
            {
                throw new Exception("不是Class类型");
            }

            TypeBuilder defClassBuilder = CreateDynamicTypeBuild(sourceType.Name + "Proxy");
            defClassBuilder.AddInterfaceImplementation(interfaceType);


            #region 编写字段 代理字段

            FieldBuilder _testfb = defClassBuilder.DefineField("_" + sourceType.Name, sourceType, FieldAttributes.Private);

            #endregion

            #region 编写构造 默认模式

            var constructor = sourceType.GetConstructors()[0];
            var sourceTypeCP = constructor.GetParameters().Select(o => o.ParameterType);
            var constructorParameters = new List<Type>();
            //constructorParameters.Add(sourceType);

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

            #region 编写构造 注入模式

            ConstructorBuilder ConstrBld_1 = defClassBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                new Type[] { sourceType }
                );
            var ConstrBld_IL = ConstrBld_1.GetILGenerator();

            ConstrBld_IL.Emit(OpCodes.Ldarg_0);//this
            ConstrBld_IL.Emit(OpCodes.Ldarg_1);//
            ConstrBld_IL.Emit(OpCodes.Stfld, _testfb);
            ConstrBld_IL.Emit(OpCodes.Ret); //函数返回
            #endregion

            #region 定义方法
            var Methods = interfaceType.GetMethods();
            //排除obj基类方法
            var methods = Methods.Where(m => m.Name != "GetHashCode" || m.Name != "Equals" || m.Name != "ToString" || m.Name != "ReferenceEquals");

            foreach (var method in methods)
            {
                ProxyAttribute proxyAttribute = null;
                var attributes = method.GetCustomAttributes(true);
                var attribute = attributes.FirstOrDefault(o => { return o is ProxyAttribute; });
                if (attribute != null)
                {
                    proxyAttribute = (ProxyAttribute)attribute;
                }
                //定义一个方法
                var parameterTypes = method.GetParameters().Select(o => o.ParameterType).ToArray();
                var methodBldr = defClassBuilder.DefineMethod(method.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    method.ReturnType,
                    parameterTypes
                  );
                Expression<Func<int, int, string, int>> add = (x, y, z) => 1 + 2;
                //add.CompileToMethod(methodBldr);

                ILGenerator ilOfShow = methodBldr.GetILGenerator();

                //方法调之前

                LocalBuilder returnType = ilOfShow.DeclareLocal(method.ReturnType);
                //非静态方法参数索引从1开始
                //ilOfShow.Emit(OpCodes.Ldstr, "姓名：{1} 年龄：{0}");
                #region  调用方法之前
                //ilOfShow.Emit(OpCodes.Ldarg_1);
                //ilOfShow.Emit(OpCodes.Ldarg_2);
                //ilOfShow.Emit(OpCodes.Add)
                //ilOfShow.Emit(OpCodes.Ldstr, "调用方法之前");
                //ilOfShow.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                if (proxyAttribute != null)
                {
                    ilOfShow.Emit(OpCodes.Newobj, proxyAttribute.GetType().GetConstructor(new Type[0]));
                    ilOfShow.Emit(OpCodes.Call, proxyAttribute.GetType().GetMethod("AfterCall", new Type[0]));
                    //ilOfShow.Emit(OpCodes.Pop);
                }

                #endregion

                #region 调用方法等同于  ReturnType result= method(arg1,arg2,....);
                ilOfShow.Emit(OpCodes.Ldarg_0);
                ilOfShow.Emit(OpCodes.Ldfld, _testfb);
                for (short i = 0; i < parameterTypes.Length; i++)
                {
                    ilOfShow.Emit(OpCodes.Ldarg, i + 1);
                }
                //ilOfShow.Emit(OpCodes.Ldarg_1);
                //ilOfShow.Emit(OpCodes.Ldarg_2);

                ilOfShow.Emit(OpCodes.Call, sourceType.GetMethod(method.Name, parameterTypes));
                ilOfShow.Emit(OpCodes.Stloc, returnType);//赋值语句
                #endregion

                #region  调用方法之后
                //ilOfShow.Emit(OpCodes.Ldstr, "调用方法之后");
                //ilOfShow.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                if (proxyAttribute != null)
                {
                    ilOfShow.Emit(OpCodes.Newobj, proxyAttribute.GetType().GetConstructor(new Type[0]));
                    ilOfShow.Emit(OpCodes.Call, proxyAttribute.GetType().GetMethod("AfterCall", new Type[0]));
                    //ilOfShow.Emit(OpCodes.Pop);
                }
                #endregion
                ilOfShow.Emit(OpCodes.Ldloc, returnType);//加载局部变量
                ilOfShow.Emit(OpCodes.Ret);//函数返回

            }

            #endregion

            Type t = defClassBuilder.CreateType();
            assemblyBuilder.Save("MyClass.dll");
            //assembly = assemblyBuilder.
            return t;
        }
        /// <summary>
        /// 创建一个动态类型的build
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static TypeBuilder CreateDynamicTypeBuild(string className)
        {
            //定义一个程序集的名称
            var asmName = new AssemblyName("MyClass");
            //首先就需要定义一个程序集
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            //定义一个构建模块
            var defModuleBuilder = assemblyBuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");
            //定义类
            var defClassBuilder = defModuleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class);

            return defClassBuilder;
        }

        public static AssemblyBuilder assemblyBuilder;
    }

    public class Test : InterfaceTest
    {
        private int me;
        public Test(int me)
        {
            this.me = me;
        }


        [My]
        public int add(int x, int y, string z)
        {
            return this.me + x + y + Convert.ToInt32(z);
        }
        [My]
        public void add1(int x, int y, string z)
        {

            Type type = GetType();
            ProxyAttribute customAttribute = type.GetCustomAttribute<ProxyAttribute>();
            if (customAttribute != null)
            {
                customAttribute.AfterCall();
            }
            Console.WriteLine("结果:" + (this.me + x + y + Convert.ToInt32(z)));
        }
    }

    public class TestProxy1 : InterfaceTest
    {
        private Test me;
        public TestProxy1(Test me)
        {
            this.me = me;
        }
        public TestProxy1(int x)
        {
            this.me = new Test(x);
        }

        public int add(int x, int y, string z)
        {
            var type = this.me.GetType();
            var t1 = type.GetCustomAttribute<ProxyAttribute>();
            return me.add(x, y, z);
        }
        [My]
        public void add1(int x, int y, string z)
        {
            throw new NotImplementedException();
        }
    }
    public interface InterfaceTest
    {

        int add(int x, int y, string z);
        [My]
        void add1(int x, int y, string z);
    }


    /// <summary>
    /// Aop
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ProxyAttribute : Attribute
    {

        public abstract void AfterCall();
        public abstract void FrontCall();
    }

    public class MyAttribute : ProxyAttribute
    {
        public override void AfterCall()
        {

            Console.WriteLine("调用方法之前");

        }

        public override void FrontCall()
        {
            Console.WriteLine("调用方法之后");

        }
    }
}
