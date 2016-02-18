using UnityEngine;
using UnityEditor;

using System.IO;

namespace Editor.UI
{
	/// <summary>
	/// UI系のテクスチャー設定の自動化処理です。
	/// テクスチャーをインポートするパス名の末尾が/Battle/Default/xx.pngとなっている場合は
	/// Battleタグをつけた後、16bitフォーマットに変換し、ディザ処理を自動で行います。
	/// もし/Battle/Compressed/xx.pngとなっている場合はBattleタグをつけた後、Compressedフォーマットに変換します。
	/// その他のUIの基本的なテクスチャー設定も自動で行います。
	/// </summary>
	public class UITextureImporter : AssetPostprocessor
	{
		private bool            _isDitherAsset = false;
		private TextureImporter _importer;

		private const int    ANISO_LEVEL               = 0;
		//UIディレクトリ.
		private const string UI_ROOT_DIRECTORY         = "Assets/Images/";
		//UIディレクトリ内の16bitとディザをかけるディレクトリ.
		private const string DITHER_16BIT_DIRECTORY    = "Dither";
		//UIディレクトリ内のディザをかけない16bitディレクトリ.
		private const string DEFAULT_16BIT_DIRECTORY   = "Default";
		//UIディレクトリ内のCompressed設定にするディレクトリ.
		private const string COMPRESSE_DIRECTORY       = "Compressed";
		private const string DIRECTORY_WARNING_MESSAGE = "UIテクスチャーは" + DEFAULT_16BIT_DIRECTORY + "か" + DITHER_16BIT_DIRECTORY + "か" + COMPRESSE_DIRECTORY + "ディレクトリに入れてください";

		public enum MAX_TEXTURE_SIZE
		{
			Size16   = 16,
			Size32   = 32,
			Size64   = 64,
			Size128  = 128,
			Size256  = 256,
			Size512  = 512,
			Size1024 = 1024,
			Size2048 = 2048
		}

		/// <summary>
		/// アセットインポート開始前にcall.
		/// </summary>
		void OnPreprocessTexture()
		{
			if (assetPath.StartsWith(UI_ROOT_DIRECTORY) == false)
			{
				return;
			}

			string directory = Path.GetDirectoryName(assetPath);

			if (!directory.EndsWith(DEFAULT_16BIT_DIRECTORY) & !directory.EndsWith(DITHER_16BIT_DIRECTORY) & !directory.EndsWith(COMPRESSE_DIRECTORY))
			{
				Debug.LogWarning(DIRECTORY_WARNING_MESSAGE);
				return;
			}

			_importer = (TextureImporter)assetImporter;

			//インポートセッティング処理.
			_SetImportSettings(Path.GetFileName(Path.GetDirectoryName(directory)));

			//フォーマット処理.
			if (directory.EndsWith(DITHER_16BIT_DIRECTORY))
			{
				_isDitherAsset = true;
				Dither16bitModifier.SetPreDitherImportSettings(_importer);
			}
			else if (directory.EndsWith(DEFAULT_16BIT_DIRECTORY))
			{
				_Set16bitFormat();
			}
			else if (directory.EndsWith(COMPRESSE_DIRECTORY))
			{
				_SetCompressedFormat();
			}
		}

		/// <summary>
		/// アセットインポート完了直後にcall.
		/// </summary>
		/// <param name="texture">Texture.</param>
		void OnPostprocessTexture(Texture2D texture)
		{
			if (!_isDitherAsset)
			{
				return;
			}

			Dither16bitModifier.Process16bitDither(texture, _importer);
		}

		private void _SetImportSettings(string packingTag)
		{
			_importer.spritePackingTag = packingTag;
			_importer.textureType = TextureImporterType.Advanced;
			_importer.alphaIsTransparency = true;
			_importer.generateCubemap = TextureImporterGenerateCubemap.None;
			_importer.isReadable = false;
			_importer.lightmap = false;
			_importer.normalmap = false;
			_importer.spriteImportMode = SpriteImportMode.Single;
			_importer.npotScale = TextureImporterNPOTScale.None;
			_importer.mipmapEnabled = false;
			_importer.wrapMode = TextureWrapMode.Clamp;
			_importer.filterMode = FilterMode.Bilinear;
			_importer.anisoLevel = ANISO_LEVEL;
			_importer.maxTextureSize = (int)MAX_TEXTURE_SIZE.Size2048;
		}

		private void _Set16bitFormat()
		{
			_importer.textureFormat = TextureImporterFormat.Automatic16bit;
		}

		private void _SetCompressedFormat()
		{
			_importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
		}
	}
}