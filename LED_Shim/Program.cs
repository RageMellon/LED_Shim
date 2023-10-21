namespace LED_Shim
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var shim = new LEDShim();
			shim.SetPixel(1, 50, 50, 50);
		}
	}
}