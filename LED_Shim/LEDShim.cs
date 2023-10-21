using System.Device.I2c;

public class LEDShim
{
	private I2cDevice i2c;
	private bool isSetup;
	private bool clearOnExit;
	private double brightness;
	private int currentFrame;
	private int width;
	private List<int[]> buf;
	private int Addr;

	private const byte MODE_REGISTER = 0x00;
	private const byte FRAME_REGISTER = 0x01;
	private const byte AUTOPLAY1_REGISTER = 0x02;
	private const byte AUTOPLAY2_REGISTER = 0x03;
	private const byte BLINK_REGISTER = 0x05;
	private const byte AUDIOSYNC_REGISTER = 0x06;
	private const byte BREATH1_REGISTER = 0x08;
	private const byte BREATH2_REGISTER = 0x09;
	private const byte SHUTDOWN_REGISTER = 0x0a;
	private const byte GAIN_REGISTER = 0x0b;
	private const byte ADC_REGISTER = 0x0c;

	private const byte CONFIG_BANK = 0x0b;
	private const byte BANK_ADDRESS = 0xfd;

	private const byte PICTURE_MODE = 0x00;
	private const byte AUTOPLAY_MODE = 0x08;
	private const byte AUDIOPLAY_MODE = 0x18;

	private const byte ENABLE_OFFSET = 0x00;
	private const byte BLINK_OFFSET = 0x12;
	private const byte COLOR_OFFSET = 0x24;

	private static readonly int[][] LOOKUP = {
		new[] {118, 69, 85},
		new[] {117, 68, 101},
		new[] {116, 84, 100},
		new[] {115, 83, 99},
		new[] {114, 82, 98},
		new[] {113, 81, 97},
		new[] {112, 80, 96},
		new[] {134, 21, 37},
		new[] {133, 20, 36},
		new[] {132, 19, 35},
		new[] {131, 18, 34},
		new[] {130, 17, 50},
		new[] {129, 33, 49},
		new[] {128, 32, 48},
		new[] {127, 47, 63},
		new[] {121, 41, 57},
		new[] {122, 25, 58},
		new[] {123, 26, 42},
		new[] {124, 27, 43},
		new[] {125, 28, 44},
		new[] {126, 29, 45},
		new[] {15, 95, 111},
		new[] {8, 89, 105},
		new[] {9, 90, 106},
		new[] {10, 91, 107},
		new[] {11, 92, 108},
		new[] {12, 76, 109},
		new[] {13, 77, 93}
	};

	private static readonly int WIDTH = 28;
	private static readonly int HEIGHT = 1;

	private static readonly byte[] LED_GAMMA = {
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2,
		2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5,
		6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 11, 11,
		11, 12, 12, 13, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18,
		19, 19, 20, 21, 21, 22, 22, 23, 23, 24, 25, 25, 26, 27, 27, 28,
		29, 29, 30, 31, 31, 32, 33, 34, 34, 35, 36, 37, 37, 38, 39, 40,
		40, 41, 42, 43, 44, 45, 46, 46, 47, 48, 49, 50, 51, 52, 53, 54,
		55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
		71, 72, 73, 74, 76, 77, 78, 79, 80, 81, 83, 84, 85, 86, 88, 89,
		90, 91, 93, 94, 95, 96, 98, 99, 100, 102, 103, 104, 106, 107, 109, 110,
		111, 113, 114, 116, 117, 119, 120, 121, 123, 124, 126, 128, 129, 131, 132, 134,
		135, 137, 138, 140, 142, 143, 145, 146, 148, 150, 151, 153, 155, 157, 158, 160,
		162, 163, 165, 167, 169, 170, 172, 174, 176, 178, 179, 181, 183, 185, 187, 189,
		191, 193, 194, 196, 198, 200, 202, 204, 206, 208, 210, 212, 214, 216, 218, 220,
		222, 224, 227, 229, 231, 233, 235, 237, 239, 241, 244, 246, 248, 250, 252, 255
	};

	public LEDShim(int addr = 0x74, int width = 28)
	{
		this.width = width;
		this.isSetup = false;
		this.clearOnExit = true;
		this.brightness = 1.0;
		this.buf = new List<int[]>();
		for (int i = 0; i < width; i++)
		{
			this.buf.Add(new int[] { 0, 0, 0, 255 });
		}
		this.Clear();
		this.i2c = null;
	}

	private void Setup()
	{
		if (this.isSetup)
		{
			return;
		}
		this.isSetup = true;
		this.i2c = I2cDevice.Create(new I2cConnectionSettings(1, this.Addr));
		this.Reset();
		this.Show();
		this.Bank(CONFIG_BANK);
		this.Write(this.Addr, MODE_REGISTER, new byte[] { PICTURE_MODE });
		this.Write(this.Addr, AUDIOSYNC_REGISTER, new byte[] { 0 });

		byte[] enablePattern = new byte[]
		{
			0b00000000, 0b10111111,
			0b00111110, 0b00111110,
			0b00111111, 0b10111110,
			0b00000111, 0b10000110,
			0b00110000, 0b00110000,
			0b00111111, 0b10111110,
			0b00111111, 0b10111110,
			0b01111111, 0b11111110,
			0b01111111, 0b00000000,
		};

		this.Bank(1);
		this.Write(this.Addr, 0x00, enablePattern);

		this.Bank(0);
		this.Write(this.Addr, 0x00, enablePattern);

		AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
		{
			this.ClearOnExit();
		};

		Console.CancelKeyPress += (sender, e) =>
		{
			Environment.Exit(0);
		};
	}

	public void Clear()
	{
		this.currentFrame = 0;
		this.buf = new List<int[]>();
		for (int i = 0; i < this.width; i++)
		{
			this.buf.Add(new int[] { 0, 0, 0, 255 });
		}
	}

	public void ClearOnExit()
	{
		if (this.clearOnExit)
		{
			this.Clear();
			this.Show();
		}
	}

	public void SetClearOnExit(bool v)
	{
		this.clearOnExit = v;
	}

	public void SetBrightness(double br)
	{
		this.brightness = br;
	}

	public void SetAll(int r, int g, int b, double br = 1.0)
	{
		for (int i = 0; i < this.width; i++)
		{
			this.SetPixel(i, r, g, b, br);
		}
	}

	public void SetPixel(int x, int r, int g, int b, double br = 1.0)
	{
		//r = Math.Truncate(r);
		//g = Math.Truncate(g);
		//b = Math.Truncate(b);

		if (double.IsNaN(r) || r > 255 || r < 0 || double.IsNaN(g) || g > 255 || g < 0 || double.IsNaN(b) || b > 255 || b < 0)
		{
			throw new Exception("Invalid RGB value. Must be 0 <= value <= 255");
		}

		if (x < 0 || x >= this.width)
		{
			throw new Exception("Invalid pixel index");
		}

		this.buf[x] = new int[] { r, g, b, (int)(br * 255) };
	}

	private List<List<int>> Chunk(List<int> array, int count)
	{
		if (count == 0 || count < 1)
		{
			return new List<List<int>>();
		}

		List<List<int>> result = new List<List<int>>();
		int i = 0;
		int length = array.Count;
		while (i < length)
		{
			result.Add(array.GetRange(i, Math.Min(count, length - i)));
			i += count;
		}

		return result;
	}

	public void Show()
	{
		this.Setup();

		int nextFrame = this.currentFrame == 1 ? 0 : 1;
		int[] output = new int[144];

		for (int x = 0; x < this.width; x++)
		{
			int[] rgbbr = this.buf[x];
			int r = rgbbr[0];
			int g = rgbbr[1];
			int b = rgbbr[2];
			double br = rgbbr[3] / 255.0;

			r = LED_GAMMA[(int)(r * this.brightness * br)];
			g = LED_GAMMA[(int)(g * this.brightness * br)];
			b = LED_GAMMA[(int)(b * this.brightness * br)];

			int[][] lookup = LOOKUP;
			for (int i = 0; i < 3; i++)
			{
				output[lookup[x][i]] = i switch
				{
					0 => r,
					1 => g,
					2 => b,
					_ => 0,
				};
			}
		}

		this.Bank(((byte)nextFrame));
		int offset = 0;
		List<List<int>> chunks = this.Chunk(new List<int>(output), 32);

		foreach (List<int> chk in chunks)
		{
			this.Write(this.Addr, ((byte)(COLOR_OFFSET + offset)), chk.Select(c => ((byte)c)).ToArray());
			offset += 32;
		}

		this.Frame(nextFrame);
	}

	public void Reset()
	{
		this.Sleep(true);
		this.Sleep(false);
	}

	public bool Sleep(bool value)
	{
		return this.Register(CONFIG_BANK, SHUTDOWN_REGISTER, !value ? (byte)1 : (byte)0);
	}

	private void Frame(int frame, bool show = true)
	{
		if (frame < 0 || frame > 8)
		{
			throw new Exception("Invalid frame value");
		}

		this.currentFrame = frame;

		if (show)
		{
			this.Register(CONFIG_BANK, FRAME_REGISTER, (byte)frame);
		}
	}

	private void Bank(byte bank)
	{
		this.Write(this.Addr, BANK_ADDRESS, new byte[] { bank });
	}

	private void Write(int addr, byte cmd, byte[] data)
	{
		Span<byte> buffer = data;
		this.i2c.Write(new byte[] { cmd });
		this.i2c.Write(buffer);
	}

	private bool Register(byte bank, byte register, byte value)
	{
		this.Bank(bank);
		this.Write(this.Addr, register, new byte[] { value });
		return true;
	}
}
