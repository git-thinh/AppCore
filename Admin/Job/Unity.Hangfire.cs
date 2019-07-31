using Hangfire;
using System; 
using Unity;

namespace Hangfire
{
    public class UnityJobActivator : Hangfire.JobActivator
    {
        private readonly IUnityContainer _container;

        public UnityJobActivator(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public override object ActivateJob(Type jobType)
        {
            return _container.Resolve(jobType);
        }

        public override Hangfire.JobActivatorScope BeginScope(Hangfire.JobActivatorContext context)
        {
            return new UnityScope(_container.CreateChildContainer());
        }
    }

    public class UnityScope : Hangfire.JobActivatorScope
    {
        private static IUnityContainer _container;

        public UnityScope(IUnityContainer container)
        {
            _container = container;
        }

        public override object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public override void DisposeScope()
        {
            _container.Dispose();
        }
    }













}
