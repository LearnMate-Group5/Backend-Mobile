using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using SharedLibrary.Contracts.UserCreating;

namespace Application.Sagas
{
    public class UserCreatingSaga : MassTransitStateMachine<UserCreatingSagaData>
    {
        public State Completed { get; set; }
        public State Failed { get; set; }


        public Event<UserCreatingSagaStart> userCreated { get; set; }

        public UserCreatingSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => userCreated, e => e.CorrelateById(m => m.Message.CorrelationId));

            Initially(
                When(userCreated)
                    .ThenAsync(async context =>
                    {
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.UserCreated = true;

                        await context.Publish(new UserCreatedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Name = context.Message.Name,
                            Email = context.Message.Email
                        });
                    })
                    .TransitionTo(Completed)
            );

            SetCompletedWhenFinalized();
        }
    }
}
