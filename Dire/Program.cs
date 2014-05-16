using NetMQ;
using System;
using System.Collections.Generic;

/* Directory server. Really just a key-value string store.
 * 
 * Always hosted on :32770. No ACL yet. TODO: send dontWait option?
 * 
 * `? something` looks up 'something', `! one two` sets entry `one` to `two`.
 */

namespace Dire
{
  class MainClass
  {
    static Dictionary<string, string> dir = new Dictionary<string, string>();

    public static void Main(string[] args)
    {
      using (var ctx = NetMQContext.Create())
      using (var server = ctx.CreateResponseSocket()) {
        server.Bind("tcp://0.0.0.0:32770");

        while (true) {
          var parts = server.ReceiveString().Split(new char[] { ' ' }, 3);
          var ret = "ERR from Dire";

          try {

            if (parts[0] == "?") {
              ret = dir[parts[1]];
            } else if (parts[0] == "!") {
              if (dir.ContainsKey(parts[1]))
                dir.Remove(parts[1]);

              dir.Add(parts[1], parts[2]);
              ret = null;
            }

          } catch (Exception) {
          } finally {
            server.Send(ret ?? "");
          }
        }

      }
    }

  }
}
