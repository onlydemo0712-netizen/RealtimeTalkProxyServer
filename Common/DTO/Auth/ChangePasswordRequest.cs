using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Auth
{
    public class ChangePasswordRequest
    {
        public string Account { get; set; }         // 帳號
        public string OldPassword { get; set; }     // 舊密碼
        public string NewPassword { get; set; }     // 新密碼
    }
}
