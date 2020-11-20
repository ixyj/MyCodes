namespace InstancesProxy
{
    using Castle.DynamicProxy;
    using System;
    using System.Linq;

    public class InstanceProxy<TInterface>
    {
        private static readonly ProxyGenerator ProxyGen = new ProxyGenerator();

        public InstanceProxy(dynamic target)
        {
            Instance = (TInterface)ProxyGen.CreateInterfaceProxyWithoutTarget(typeof(TInterface), new Interceptor(target));
        }

        public TInterface Instance { get; }


        internal class Interceptor : IInterceptor
        {
            private readonly object _target;

            public Interceptor(object target)
            {
                _target = target;
            }

            public void Intercept(IInvocation invocation)
            {
                try
                {
                    var methodName = invocation.Method.Name;
                    if (methodName.StartsWith("get_"))
                    {
                        var propertyName = methodName.Substring("get_".Length);
                        invocation.ReturnValue = _target.GetType().GetProperty(propertyName).GetValue(_target);
                    }
                    else if (methodName.StartsWith("set_"))
                    {
                        var propertyName = methodName.Substring("set_".Length);
                        _target.GetType().GetProperty(propertyName).SetValue(_target, invocation.Arguments[0]);
                    }
                    else
                    {
                        var method = _target.GetType().GetMethod(methodName, invocation.Arguments.Select(m => m.GetType()).ToArray());
                        invocation.ReturnValue = method.Invoke(_target, invocation.Arguments);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

    }
}
