namespace LED_Shim
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Running...");

			var shim = new LEDShim();
			shim.SetPixel(1, 255,255,255);
			shim.SetPixel(2, 255,255,255);
			shim.SetPixel(3, 255,255,255);
			shim.SetPixel(4, 255,255,255);
			shim.SetPixel(5, 255,255,255);
			shim.SetPixel(6, 255,255,255);
			shim.SetPixel(7, 255,255,255);
			shim.SetPixel(8, 255,255,255);
			shim.SetPixel(9, 255, 255, 255);
			shim.SetPixel(10, 255, 255, 255);

			shim.Show();

			Console.ReadLine();
		}
	}
}