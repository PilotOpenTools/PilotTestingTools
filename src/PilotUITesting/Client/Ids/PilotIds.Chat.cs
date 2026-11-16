namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>
        /// Панель чата (ChatView): вкладка «Чаты», окно чата объекта, чат объекта в представлении модуля (IChatControlsProvider.GetObjectChatsControl).
        /// [Д] дамп Pilot-BIM 26.20, чат замечания в представлении модуля.
        /// </summary>
        public static class Chat
        {
            /// <summary>(Custom, класс ChatView) [Д] дамп.</summary>
            public const string Container = "AUIDChatView";

            /// <summary>Лента сообщений (List, ListView); сообщения - ListItem, Name = текст сообщения [Д] дамп.</summary>
            public const string Messages = "messagesListControl";

            /// <summary>Текст сообщения (Document, RichTextBox); ссылки в нём - Hyperlink, Name = адрес [Д] дамп.</summary>
            public const string MessageText = "messageText";

            /// <summary>Автор сообщения (Text) с Hyperlink имени пользователя [Д] дамп.</summary>
            public const string MessageAuthor = "authorTextBlock";

            /// <summary>Системное сообщение («… добавил(а) … в чат») (Text) [Д] дамп.</summary>
            public const string SystemMessage = "chatSystemTextBlock";

            /// <summary>Поле нового сообщения (Edit, ValuePattern) [Д] дамп.</summary>
            public const string NewMessage = "newMessageTextBox";

            /// <summary>«Отправить (Ctrl+Enter)» (Button); недоступна, пока поле сообщения пусто [Д] дамп.</summary>
            public const string Send = "sendButton";

            /// <summary>Добавить вложения (Button) [Д] дамп.</summary>
            public const string AddAttachment = "addAttachmentButton";
        }
    }
}
