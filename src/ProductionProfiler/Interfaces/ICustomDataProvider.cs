
using System;
using Castle.Windsor;

namespace ProductionProfiler.Interfaces
{
    public interface ICustomDataProvider
    {
        Type ProfiledRequestDataRepositoryType { get; set; }
        Type ProfiledRequestRepository { get; set; }
        void RegisterDependentComponents(IWindsorContainer container);
    }
}
