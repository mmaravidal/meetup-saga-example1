using System;
using System.Runtime.Serialization;

namespace Example2.Events
{
    [Serializable]
    public class CompensationEventDetectedException : Exception
    {
        private object Event;

        public CompensationEventDetectedException()
        {
        }

        public CompensationEventDetectedException(string message)
            : base(message)
        {
        }

        public CompensationEventDetectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CompensationEventDetectedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));

            Event = info.GetValue("Event", typeof(object));
        }

        public CompensationEventDetectedException(object @event)
            : base($"A '{@event?.GetType().Name}' comepnsation event was detected")
        {
            Event = @event;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));

            info.AddValue("Event", Event);

            base.GetObjectData(info, context);
        }
    }
}
