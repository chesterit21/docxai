using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Drawing;
using System.IO;

namespace Api.Extensions
{
	public class OcrImagePreprocessing
	{
		public static void PreprocessImage(string inputPath, string outputPath)
		{
			using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(inputPath))
			{
				// Resize (jika perlu), misal max width 1024 px
				int maxWidth = 1024;
				if (image.Width > maxWidth)
				{
					image.Mutate(x => x.Resize(maxWidth, 0));
				}
				// Grayscale
				image.Mutate(x => x.Grayscale());
				// Thresholding (binarization) - custom threshold 128
				image.Mutate(ctx => ctx.BinaryThreshold(0.5f)); // 0.5f = 128/255
																// Noise removal sederhana dengan blur (median blur tidak tersedia di ImageSharp, gunakan GaussianBlur)
				image.Mutate(x => x.GaussianBlur(1.0f));
				// Simpan hasil preprocessing
				image.Save(outputPath, new PngEncoder());
			}
		}

		/// </summary>
		/// <param name="inputBitmap">The raw Bitmap loaded from the stream.</param>
		/// <returns>A new, processed Bitmap ready for Tesseract.</returns>
		public static Bitmap PreprocessImageInMemory(Bitmap inputBitmap)
		{
			// 1. Convert System.Drawing.Bitmap to ImageSharp (via MemoryStream)
			using var tempStream = new MemoryStream();
			// Save the Bitmap to the stream as PNG format
			inputBitmap.Save(tempStream, System.Drawing.Imaging.ImageFormat.Png);
			tempStream.Seek(0, SeekOrigin.Begin);

			// Load the stream into ImageSharp
			using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(tempStream))
			{
				// 2. Apply your original ImageSharp Mutate logic

				// Resize (max width 1024 px)
				int maxWidth = 1024;
				if (image.Width > maxWidth)
				{
					image.Mutate(x => x.Resize(maxWidth, 0));
				}
				// Grayscale
				image.Mutate(x => x.Grayscale());

				// Thresholding (binarization)
				image.Mutate(ctx => ctx.BinaryThreshold(0.5f)); // 0.5f = 128/255

				// Noise removal with GaussianBlur
				image.Mutate(x => x.GaussianBlur(1.0f));

				// 3. Convert ImageSharp back to System.Drawing.Bitmap (via new MemoryStream)
				using var outputStream = new MemoryStream();

				// Save the processed ImageSharp object to a stream
				image.Save(outputStream, new PngEncoder());
				outputStream.Seek(0, SeekOrigin.Begin);

				// Load the stream back into a System.Drawing.Bitmap and return it
				// We return a clone to allow the input Bitmap to be disposed safely.
				return new Bitmap(outputStream);
			}
		}
	}
}
