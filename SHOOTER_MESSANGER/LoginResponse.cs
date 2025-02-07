using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHOOTER_MESSANGER
{
    public class LoginResponse
    {
        // ID пользователя
        public int UserId { get; set; }

        // Сообщение (если есть от сервера)
        public string Message { get; set; }
    }
}
