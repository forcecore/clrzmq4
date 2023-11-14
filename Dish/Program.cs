using System;
using System.Threading;
using ZeroMQ;

namespace Dish
{
    class Program
    {
        static void DishMain(string[] args)
        {
	        using var ctx = new ZContext();
	        using var dish = new ZSocket(ctx, ZSocketType.DISH);
			dish.ReceiveTimeout = TimeSpan.FromMilliseconds(1000);
			const string url = "udp://224.0.7.77:40705";

			// Subscribe to every single topic from publisher
			dish.Bind(url);
			dish.Join("O");
			dish.Join("T");

			for (var i = 0; i < 10; i++)
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

				var frame = msg[0];
				var group = frame.Group();
				var json = msg[0].ReadString();
				Console.WriteLine(group + " " + json);
			}

			Console.WriteLine("Done");
		}

        static void RadioMain(string[] args)
        {
	        const string url = "udp://224.0.7.77:40705";
	        const string topic = "T";
	        using var ctx = new ZContext();
	        using var radio = new ZSocket(ctx, ZSocketType.RADIO);
	        radio.Connect(url);

	        for (var i = 0; i < 10; i++)
	        {
		        var now = DateTime.UtcNow;
		        var timestamp = ((DateTimeOffset)now).ToUnixTimeSeconds() * 1000;

		        var msg = $"{{ \"data\": {i}, \"timestamp\": {timestamp}, \"dt\": {timestamp} }}";
		        Console.WriteLine(msg);

		        var frame = new ZFrame(msg);
		        frame.SetGroup(topic);
		        using var zMessage = new ZMessage();
		        zMessage.Add(frame);

		        radio.SendMessage(zMessage, out var error);
		        if (error != null)
		        {
			        throw new ZException(error);
		        }

		        Thread.Sleep(1000);
	        }

	        Console.WriteLine("Done");
        }

        static void Main(string[] args)
        {
	        RadioMain(args);
        }
    }
}