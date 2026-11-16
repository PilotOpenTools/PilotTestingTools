using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Панель чата Pilot (<see cref="PilotIds.Chat"/>): вкладка «Чаты», окно чата объекта, чат объекта в представлении модуля.
    /// Сообщения - строки ленты (Name = текст сообщения), ссылки в тексте - Hyperlink с адресом в Name.
    /// Берётся через <see cref="PilotPage.Chat"/>.
    /// </summary>
    public sealed class PilotChat
    {
        private readonly Func<AutomationElement> _container;
        private readonly string _description;

        internal PilotChat(Func<AutomationElement> container, string description)
        {
            _container = container;
            _description = description;
        }

        /// <summary>Панель чата (ChatView); нет за <see cref="Wait.DefaultTimeout"/> - исключение.</summary>
        public AutomationElement Root => Wait.For(() => _container()?.TryById(PilotIds.Chat.Container), Wait.DefaultTimeout, "панель чата в " + _description);

        /// <summary>Тексты сообщений ленты, которые сейчас есть в дереве UI Automation, в порядке ленты (системные - тоже).</summary>
        public IReadOnlyList<string> Messages()
        {
            return MessageItems().Select(m => m.SafeName()).ToList();
        }

        /// <summary>Дождаться сообщения с точным текстом (Name строки ленты, без нормализации); timeout null - <see cref="Wait.DefaultTimeout"/>.</summary>
        public AutomationElement WaitMessage(string text, TimeSpan? timeout = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return Wait.ForStrict(() => MessageItems().LastOrDefault(m => m.SafeName() == text), timeout ?? Wait.DefaultTimeout,
                $"сообщение «{text}» в чате ({_description})", () => "сообщения: " + string.Join(" | ", Messages().Select(m => "«" + m + "»")));
        }

        /// <summary>
        /// Отправить сообщение: текст в поле нового сообщения (ValuePattern), «Отправить» после того, как она стала доступна,
        /// и дождаться в ленте нового сообщения с этим текстом (прежние сообщения с тем же текстом не засчитываются).
        /// Пустой чат объекта (ещё без сообщений) создаётся первым сообщением. Текст сравнивается точно - без пробелов по краям.
        /// </summary>
        public AutomationElement Send(string text, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim() != text)
            {
                throw new ArgumentException("Нужен текст сообщения без пробелов по краям (сообщение в ленте сравнивается точно)", nameof(text));
            }
            AutomationElement root = Root;
            List<AutomationElement> before = MessageItems().Where(m => m.SafeName() == text).ToList();
            UiActions.SetValue(root.ById(PilotIds.Chat.NewMessage, Wait.DefaultTimeout), text, "поле нового сообщения чата (" + _description + ")");
            AutomationElement send = root.ById(PilotIds.Chat.Send, Wait.DefaultTimeout);
            Wait.Until(() => UiQuery.Read(() => send.IsEnabled, Wait.DefaultTimeout, "доступность «Отправить»"), Wait.DefaultTimeout, "«Отправить» доступна");
            UiActions.Click(send, "«Отправить» в чате (" + _description + ")");
            return Wait.ForStrict(() => MessageItems().LastOrDefault(m => m.SafeName() == text && !before.Any(m.Equals)), timeout ?? Wait.DefaultTimeout,
                $"новое сообщение «{text}» в чате ({_description})", () => "сообщения: " + string.Join(" | ", Messages().Select(m => "«" + m + "»")));
        }

        /// <summary>
        /// Щёлкнуть мышью по ссылке с адресом <paramref name="address"/> в тексте последнего сообщения, где она есть.
        /// Мышью, а не Invoke: Invoke ссылки WPF через UI Automation выполняется синхронно и не возвращает управление, пока
        /// обработчик перехода (модальное окно, долгая навигация) не завершится. Щелчок - в первую строку ссылки (TextPattern
        /// текста сообщения): длинная ссылка переносится, и в центре её общего прямоугольника текста может не быть.
        /// </summary>
        public void ClickLink(string address, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Нужен адрес ссылки", nameof(address));
            }
            (AutomationElement item, AutomationElement text, AutomationElement link) found = (null, null, null);
            Wait.ForStrict(() =>
                {
                    found = MessageItems()
                        .Select(m => (m, m.TryById(PilotIds.Chat.MessageText)))
                        .Where(p => p.Item2 != null)
                        .Select(p => (p.m, p.Item2, p.Item2.FindDescendantBfs(e => e.SafeControlType() == ControlType.Hyperlink && e.SafeName() == address, 3)))
                        .LastOrDefault(p => p.Item3 != null);
                    return found.link;
                },
                timeout ?? Wait.DefaultTimeout, $"ссылка «{address}» в сообщениях чата ({_description})",
                () => "сообщения: " + string.Join(" | ", Messages().Select(m => "«" + m + "»")));
            string description = $"ссылка «{address}» в чате ({_description})";
            // сообщение за краем ленты: строки ссылки были бы вне видимой части, и щелчок попал бы в соседний элемент
            UiQuery.Read(() =>
            {
                found.item.Patterns.ScrollItem.PatternOrDefault?.ScrollIntoView();
                return true;
            }, Wait.DefaultTimeout, "прокрутка к сообщению со ссылкой");
            Rectangle line = FirstLine(found.text, found.link);
            if (line.IsEmpty)
            {
                UiActions.Click(found.link, description);
                return;
            }
            UiAct.ClickAt(found.link, new Point(line.X + Math.Min(line.Width / 2, 20), line.Y + line.Height / 2), description);
        }

        /// <summary>Прямоугольник первой строки ссылки на экране; пустой - текст сообщения не отдаёт строки (нет TextPattern).</summary>
        private static Rectangle FirstLine(AutomationElement text, AutomationElement link)
        {
            Rectangle[] lines = UiQuery.Read(() => text.Patterns.Text.PatternOrDefault?.RangeFromChild(link)?.GetBoundingRectangles(), Wait.DefaultTimeout,
                "строки ссылки в тексте сообщения") ?? new Rectangle[0];
            return lines.FirstOrDefault(r => r.Width > 0 && r.Height > 0);
        }

        // без ожидания панели: вызывается в опросах с таймаутом вызывающего
        private IEnumerable<AutomationElement> MessageItems()
        {
            AutomationElement list = _container()?.TryById(PilotIds.Chat.Container)?.TryById(PilotIds.Chat.Messages);
            return list == null
                ? Enumerable.Empty<AutomationElement>()
                : list.SafeChildren().Where(c => c.SafeControlType() == ControlType.ListItem);
        }
    }
}
