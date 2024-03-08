using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace App.Bot
{
    // Used for checking permissions to run commands
    internal class Checks
    {

        // Checks if <T> is in list
        internal class InPermissionList<T>
        {
            private List<T>? list;

            public InPermissionList(List<T> list) { this.list = list; }

            public bool Check(T id)
            {

                if (list == null)
                {
                    throw new ArgumentNullException();
                }
                T user_id = id;
                return list.Contains(user_id);
            }
        }
    }
}
