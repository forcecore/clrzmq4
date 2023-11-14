using System;
using System.Threading;
using ZeroMQ;

namespace Dish
{
    class Program
    {
        static void Main(string[] args)
        {
	        using var ctx = new ZContext();
	        using var dish = new ZSocket(ctx, ZSocketType.DISH);
			dish.ReceiveTimeout = TimeSpan.FromMilliseconds(1000);
			const string url = "udp://224.0.7.77:40705";

			// Subscribe to every single topic from publisher
			dish.Bind(url);
			dish.Join("O");
			dish.Join("T");

			while (true)
			{
				var msg = dish.ReceiveMessage(out var error);
				if (error != null)
				{
					if (error.Equals(ZError.EAGAIN))
					{
						// No message, try again.
						continue;
					}

					throw new ZException(error);
				}

				var json = msg[0].ReadString();
				Console.WriteLine(json);
			}
		}
    }
}