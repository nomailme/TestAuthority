using System;
using Proto;

namespace TestAuthority.Web.Actors
{
    /// <summary>
    /// Actor manager.
    /// </summary>
    public class ActorManager
    {
        private readonly IActorFactory factory;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="factory">Factory.</param>
        public ActorManager(IActorFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Returns actor of certain type.
        /// </summary>
        /// <typeparam name="TActor">Actor.</typeparam>
        /// <returns>Actors pid.</returns>
        public PID GetActor<TActor>()
            where TActor : IActor
        {
            return factory.GetActor<TActor>();
        }
    }
}
