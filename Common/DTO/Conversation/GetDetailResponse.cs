using Common.DTO.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Conversation
{
    public class GetDetailResponse
    {
        public string ConversationId { get; set; }
        public List<MessagePair> PairList { get; set; }                     // 使用者與AI訊息
    }
}
