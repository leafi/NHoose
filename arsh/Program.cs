using NetMQ;
using System;
using System.Collections.Generic;

namespace arsh
{
  class MainClass
  {
    static NetMQContext ctx;

    private static string oneShot(string addr, string msg)
    {
      using (var rq = ctx.CreateRequestSocket()) {
        rq.Options.Linger = TimeSpan.FromSeconds(0.5);
        rq.Options.SendTimeout = TimeSpan.FromSeconds(0.5);

        Func<Action, string, string> catching = (x, err) => {
          try {
            x();
            return null;
          } catch (TimeoutException) {
            return err;
          }
        };

        return catching(() => rq.Connect(addr), "ERR connect took too long (anyone home?)")
          ?? catching(() => rq.Send(msg), "ERR send took too long (busy?)")
          ?? rq.ReceiveString();
      }
    }

    private static string getDire(string key)
    {
      return oneShot("tcp://127.0.0.1:32770", "? " + key);
    }

    private static string setDire(string strong)
    {
      return oneShot("tcp://127.0.0.1:32770", "! " + strong);
    }

    public static void Main(string[] args)
    {
      ctx = NetMQContext.Create();

      var d = new Dictionary<string, Func<string, string>>() {
        { "?", getDire },
        { "!", setDire }
      };

      Action<string> w = (l) => Console.WriteLine(l);

      while (true) {

        var s = Console.ReadLine();
        var parts = s.Split(new char[] { ' ' }, 2);

        try {
          var r = d[parts[0]](parts[1]);
          w(r.Length == 0 ? "OK" : r);
        } catch (IndexOutOfRangeException) {
          w("ERR from arsh: num args");
        } catch (KeyNotFoundException) {
          w("ERR from arsh: not impl");
        }

      }
    }
  }
}
