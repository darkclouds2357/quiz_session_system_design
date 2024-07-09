
using System.Diagnostics;
using System.Globalization;

namespace NotifierWorker.Observability
{
    public static class ActivityExtension
    {
        public static void RecordException(this Activity activity, Exception? ex, in TagList tags)
        {
            if (ex == null || activity == null)
            {
                return;
            }

            var tagsCollection = new ActivityTagsCollection
        {
            { AttributeExceptionType, ex.GetType().FullName },
            { AttributeExceptionStacktrace, ex.ToInvariantString() },
        };

            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                tagsCollection.Add(AttributeExceptionMessage, ex.Message);
            }

            foreach (var tag in tags)
            {
                tagsCollection[tag.Key] = tag.Value;
            }

            activity.AddEvent(new ActivityEvent(AttributeExceptionEventName, default, tagsCollection));
        }
        public static string ToInvariantString(this Exception exception)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                return exception.ToString();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }

        private const string AttributeExceptionEventName = "exception";
        private const string AttributeExceptionType = "exception.type";
        private const string AttributeExceptionMessage = "exception.message";
        private const string AttributeExceptionStacktrace = "exception.stacktrace";
    }
}
