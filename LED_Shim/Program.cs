namespace LED_Shim
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Running...");

			var shim = new LEDShim();
			shim.SetPixel(1, 50, 50, 50);
			shim.SetPixel(2, 50, 50, 50);
			shim.SetPixel(3, 50, 50, 50);
			shim.SetPixel(4, 50, 50, 50);
			shim.SetPixel(5, 50, 50, 50);
			shim.SetPixel(6, 50, 50, 50);
			shim.SetPixel(7, 50, 50, 50);
			shim.SetPixel(8, 50, 50, 50);

			shim.Show();

			Console.ReadLine();
		}
	}
}