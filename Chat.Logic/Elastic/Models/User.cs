using System.Collections.Generic;
using System.Linq;
using Nest;

namespace Chat.Logic.Elastic.Models
{
    public class User
    {
        // Properties
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public HashSet<string> ConnectionIds { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Login { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Password { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string UserName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Token { get; set; }

        [Boolean]
        public bool IsOnline
        {
            get
            {
                return ConnectionIds != null && ConnectionIds.Any();
            }
        } 
    }
}