using UnityEngine;
using UnityEditor;

namespace Editor.UI
{
	/// <summary>
	/// ディザリング処理クラス。TextureImportDefinition.csを玄関として、必要なテクスチャの場合こちらの処理が読まれます。
	/// </summary>
	public class Dither16bitModifier
	{
		/// <summary>
		/// 一旦減色・ディザ処理前に32bitのフルカラーにプリプロセスで変更.
		/// </summary>
		public static void SetPreDitherImportSettings(TextureImporter importer)
		{
			importer.textureFormat = TextureImporterFormat.RGBA32;
		}

		public static void Process16bitDither(Texture2D texture, TextureImporter importer)
		{
			var pixels = _FloydSteinberg16bitsDither(texture.GetPixels(), texture.width, texture.height);
			texture.SetPixels(pixels);
			EditorUtility.CompressTexture(texture, TextureFormat.RGBA4444, TextureCompressionQuality.Best);
			//減色・ディザ処理後に16bitにフォーマット設定を変更.
			importer.textureFormat = TextureImporterFormat.Automatic16bit;
		}

		/// <summary>
		/// 16bit減色>FloydSteinbergアルゴリズムのディザ処理.
		/// </summary>
		/// <returns>Color[].</returns>
		/// <param name="pixels">Pixels.</param>
		/// <param name="texw">Texw.</param>
		/// <param name="texh">Texh.</param>
		private static Color[] _FloydSteinberg16bitsDither(Color[] pixels, int texw, int texh)
		{
			var offs = 0;
			var k1Per15 = 1.0f / 15.0f;
			var k1Per16 = 1.0f / 16.0f;
			var k3Per16 = 3.0f / 16.0f;
			var k5Per16 = 5.0f / 16.0f;
			var k7Per16 = 7.0f / 16.0f;

			for (var y = 0; y < texh; y++)
			{
				for (var x = 0; x < texw; x++)
				{
					float a = pixels [offs].a;
					float r = pixels [offs].r;
					float g = pixels [offs].g;
					float b = pixels [offs].b;

					var a2 = Mathf.Clamp01(Mathf.Floor(a * 16) * k1Per15);
					var r2 = Mathf.Clamp01(Mathf.Floor(r * 16) * k1Per15);
					var g2 = Mathf.Clamp01(Mathf.Floor(g * 16) * k1Per15);
					var b2 = Mathf.Clamp01(Mathf.Floor(b * 16) * k1Per15);

					var ae = a - a2;
					var re = r - r2;
					var ge = g - g2;
					var be = b - b2;

					pixels [offs].a = a2;
					pixels [offs].r = r2;
					pixels [offs].g = g2;
					pixels [offs].b = b2;

					var n1 = offs + 1;
					var n2 = offs + texw - 1;
					var n3 = offs + texw;
					var n4 = offs + texw + 1;

					if (x < texw - 1)
					{
						pixels [n1].a += ae * k7Per16;
						pixels [n1].r += re * k7Per16;
						pixels [n1].g += ge * k7Per16;
						pixels [n1].b += be * k7Per16;
					}

					if (y < texh - 1)
					{
						pixels [n3].a += ae * k5Per16;
						pixels [n3].r += re * k5Per16;
						pixels [n3].g += ge * k5Per16;
						pixels [n3].b += be * k5Per16;

						if (x > 0)
						{
							pixels [n2].a += ae * k3Per16;
							pixels [n2].r += re * k3Per16;
							pixels [n2].g += ge * k3Per16;
							pixels [n2].b += be * k3Per16;
						}

						if (x < texw - 1)
						{
							pixels [n4].a += ae * k1Per16;
							pixels [n4].r += re * k1Per16;
							pixels [n4].g += ge * k1Per16;
							pixels [n4].b += be * k1Per16;
						}
					}

					offs++;
				}
			}

			return pixels;
		}
	}
}
