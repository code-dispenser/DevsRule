using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Utilities;

namespace DevsRule.Core.Areas.Events
{
    /// <summary>
    /// Details about the event, when and how it should be raised. Applies to both rules and conditions.
    /// </summary>
    public class EventDetails
    {
        public string           EventTypeName { get; }
        public EventWhenType    EventWhenType { get; }
        public PublishMethod    PublishMethod { get; }

        internal EventDetails(string eventTypeName, EventWhenType eventWnenType = EventWhenType.OnSuccessOrFailure, PublishMethod publishMethod = PublishMethod.FireAndForget)

            => (EventTypeName, EventWhenType, PublishMethod) = (eventTypeName, eventWnenType,publishMethod);

        public static EventDetails Create<TEvent>(EventWhenType eventWnenType = EventWhenType.OnSuccessOrFailure, PublishMethod publishMethod = PublishMethod.FireAndForget) where TEvent : IEvent
        {
            return new EventDetails(typeof(TEvent).AssemblyQualifiedName!, eventWnenType, publishMethod);
        }

        internal static JsonRule.EventDetails? ToJsonRule(EventDetails? eventDetails)
        {
            if (eventDetails == null) return null;

            var jsonEventDetails = new JsonRule.EventDetails();

            jsonEventDetails.EventTypeName = eventDetails.EventTypeName.Split(',')[0];
            jsonEventDetails.EventWhenType = eventDetails.EventWhenType.ToString();
            jsonEventDetails.PublishMethod = eventDetails.PublishMethod.ToString();

            return jsonEventDetails;
        }

        internal static EventDetails? FromJsonRule(JsonRule.EventDetails? eventDetails)
        {
            if (eventDetails == null) return null;

            string searchName = eventDetails.EventTypeName?.Contains('.') == true ? eventDetails.EventTypeName : String.Concat(".", eventDetails?.EventTypeName);

            string? assemblyQualifiedName = (GeneralUtils.EventTypeNames.Where(e => e.fullName.EndsWith(searchName))?.SingleOrDefault())?.assemblyQualifiedName;

     
            PublishMethod publishMethod = Enum.TryParse<PublishMethod>(eventDetails!.PublishMethod, out var publishMethodValue) == true ? publishMethodValue : PublishMethod.FireAndForget;
            EventWhenType eventWhenType = Enum.TryParse<EventWhenType>(eventDetails.EventWhenType, out var eventWhenValue) == true ? eventWhenValue : EventWhenType.Never;

            return String.IsNullOrWhiteSpace(assemblyQualifiedName) == false ? new EventDetails(assemblyQualifiedName, eventWhenType, publishMethod) : null;
        }

    }



}
