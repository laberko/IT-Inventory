using System;
using System.Linq;
using System.Threading.Tasks;
using ActionMailer.Net.Mvc4;
using IT_Inventory.Models;
using IT_Inventory.ViewModels;

namespace IT_Inventory.Controllers
{
    public class EmailController : MailerBase
    {
        private readonly InventoryModel _db = new InventoryModel();

        //support requests recievers
        //private readonly string[] _supportUsers = { "m_laberko@rivs.ru", "P_Petrov@rivs.ru", "a_lobyzenko@rivs.ru"  };
        private readonly string[] _supportUsers = { "m_laberko@rivs.ru" };
        //support feedback recievers
        private readonly string[] _supportManagers = { "m_laberko@rivs.ru" };
        //urgent items warning recievers
        private readonly string[] _inventoryManagers = { "m_laberko@rivs.ru", "a_lobyzenko@rivs.ru", "L_Zimin@rivs.ru" };

        //user created new request
        public async Task<EmailResult> NewFromUser(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetSupportMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail))
                return null;
            model.Header = "Новая заявка";
            foreach (var mail in _supportUsers)
                To.Add(mail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = model.Header;
            AddMailToDb(StaticData.MailType.Support);
            return Email("SupportEmail", model);
        }

        //IT user created new request or changed existing
        public async Task<EmailResult> EditByIt(int requestId, string editorName)
        {
            To.Clear();
            var model = await StaticData.GetSupportMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = editorName + " адресовал заявку";
            To.Add(model.ToMail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = "Адресована заявка";
            AddMailToDb(StaticData.MailType.Support);
            return Email("SupportEmail", model);
        }

        public async Task<EmailResult> Accepted(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetSupportMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = model.To + " приступил к выполнению Вашей заявки";
            To.Add(model.FromMail);
            From = model.To + "<" + model.ToMail + ">";
            model.FromIt = true;
            Subject = "Заявка принята к исполнению";
            AddMailToDb(StaticData.MailType.Support);
            return Email("SupportEmail", model);
        }

        public async Task<EmailResult> EditByUser(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetSupportMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = "Заявка изменена";
            To.Add(model.ToMail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = model.Header;
            AddMailToDb(StaticData.MailType.Support);
            return Email("SupportEmail", model);
        }

        public async Task<EmailResult> Finished(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetSupportMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail) || string.IsNullOrEmpty(model.ToMail))
                return null;
            model.Header = model.To + " завершил выполнение Вашей заявки";
            To.Add(model.FromMail);
            From = model.To + "<" + model.ToMail + ">";
            model.FromIt = true;
            Subject = "Выполнение заявки завершено";
            AddMailToDb(StaticData.MailType.Support);
            return Email("SupportEmail", model);
        }

        public async Task<EmailResult> Feedback(int requestId)
        {
            To.Clear();
            var model = await StaticData.GetSupportMailViewModel(requestId);
            if (string.IsNullOrEmpty(model?.FromMail))
                return null;
            model.Header = model.From + " оценил(а) работу над заявкой";
            foreach(var mail in _supportManagers)
                To.Add(mail);
            To.Add(model.ToMail);
            From = model.From + "<" + model.FromMail + ">";
            Subject = "Оценка работы над заявкой";
            AddMailToDb(StaticData.MailType.Support);
            return Email("SupportEmail", model);
        }

        public EmailResult UrgentItemsWarning()
        {
            To.Clear();
            var items = _db.Items.Where(i => i.Quantity <= i.MinQuantity)
                .OrderBy(i => i.ItemType.Name).ThenBy(i => i.Name).ToList();
            if (items.Count == 0)
                return null;
            var models = items.Select(item => new ItemViewModel
            {
                ItemTypeName = item.ItemType.Name,
                Name = item.Name,
                Quantity = item.Quantity,
                AttributeValues = null
            }).ToList();
            foreach (var mail in _inventoryManagers)
                To.Add(mail);
            //To.Add("m_laberko@rivs.ru");
            From = "Инвентаризация <inventory@rivs.ru>";
            Subject = "Нехватка оборудования на складе";
            AddMailToDb(StaticData.MailType.Inventory);
            return Email("UrgentItems", models);
        }

        private void AddMailToDb(StaticData.MailType type)
        {
            _db.SentMails.Add(new SentMail
            {
                From = From,
                To = string.Join(",", To),
                Date = DateTime.Now,
                Subject = Subject,
                Type = type
            });
            _db.SaveChanges();
        }
    }
}