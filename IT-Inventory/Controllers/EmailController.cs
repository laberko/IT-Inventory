using System.Threading.Tasks;
using ActionMailer.Net.Mvc4;

namespace IT_Inventory.Controllers
{
    public class EmailController : MailerBase
    {
        private readonly string[] _supportUsers = { "m_laberko@rivs.ru", "P_Petrov@rivs.ru", "a_lobyzenko@rivs.ru",  };
        private readonly string[] _supportManagers = { "m_laberko@rivs.ru" };

        //user created new request
        public async Task<EmailResult> NewFromUser(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail))
                return null;
            model.Header = "Новая заявка";
            foreach (var mail in _supportUsers)
                To.Add(mail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = model.Header;
            return Email("Email", model);
        }

        //IT user created new request or changed existing
        public async Task<EmailResult> EditByIt(int requestId, string editorName)
        {
            To.Clear();
            var model = await StaticData.GetMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = editorName + " адресовал заявку";
            To.Add(model.ToMail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = "Адресована заявка";
            return Email("Email", model);
        }

        public async Task<EmailResult> Accepted(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = model.To + " приступил к выполнению Вашей заявки";
            To.Add(model.FromMail);
            From = model.To + "<" + model.ToMail + ">";
            model.FromIt = true;
            Subject = "Заявка принята к исполнению";
            return Email("Email", model);
        }

        public async Task<EmailResult> EditByUser(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = "Заявка изменена";
            To.Add(model.ToMail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = model.Header;
            return Email("Email", model);
        }

        public async Task<EmailResult> Finished(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = model.To + " завершил выполнение Вашей заявки";
            To.Add(model.FromMail);
            From = model.To + "<" + model.ToMail + ">";
            model.FromIt = true;
            Subject = "Выполнение заявки завершено";
            return Email("Email", model);
        }

        public async Task<EmailResult> Feedback(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail))
                return null;
            model.Header = model.From + " оценил(а) работу над заявкой";
            foreach(var mail in _supportManagers)
                To.Add(mail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = "Оценка работы над заявкой";
            return Email("Email", model);
        }
    }
}