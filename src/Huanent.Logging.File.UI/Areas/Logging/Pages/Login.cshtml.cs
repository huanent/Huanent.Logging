using Huanent.Logging.File.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Huanent.Logging.File.UI.Areas.Logging.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ConfigService _configService;

        public LoginModel(ConfigService configService)
        {
            _configService = configService;
        }

        public string Btn { get; set; } = "��¼";

        [BindProperty]
        public string Pwd { get; set; }

        public string Tip { get; set; } = string.Empty;

        public string Title { get; set; } = "��־��ѯ��¼";

        public void OnGet()
        {
            if (_configService.Get() == null)
            {
                Title = "�������ʼ����";
                Btn = "����";
            }
        }

        public IActionResult OnPost()
        {
            var config = _configService.Get();
            if (config != null)
            {
                if (config.CanLoginDate > DateTime.Now)
                {
                    Tip = "������󳬹�3�Σ�����5����";
                    return Page();
                }

                if (config.Pwd.Trim() != Pwd)
                {
                    Tip = "�������";
                    config.PwdErrorCount += 1;
                    if (config.PwdErrorCount > 3)
                    {
                        config.PwdErrorCount = 0;
                        config.CanLoginDate = DateTime.Now.AddMinutes(5);
                        Tip = "������󳬹�3�Σ�����5����";
                    }
                    _configService.Save(config);
                    return Page();
                }
            }
            else
            {
                var rijndaelManaged = Rijndael.Create();
                rijndaelManaged.Mode = CipherMode.ECB;
                rijndaelManaged.Padding = PaddingMode.PKCS7;
                rijndaelManaged.GenerateKey();
                config = new ConfigModel
                {
                    Pwd = Pwd,
                    AESKey = rijndaelManaged.Key
                };
                _configService.Save(config);
            }
            string token = AES256Helper.Encrypt(DateTime.Now.AddMinutes(1).ToString(), config.AESKey);
            Response.Cookies.Append(Constants.CookieName, token);
            return RedirectToPage("Index");
        }
    }
}