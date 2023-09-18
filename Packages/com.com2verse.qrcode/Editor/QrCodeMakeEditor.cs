/*===============================================================
* Product:		Com2Verse
* File Name:	QrCodeMakeEditor.cs
* Developer:	klizzard
* Date:			2023-04-05 11:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Com2Verse.QrCode;
using Com2Verse.QrCode.Helpers;
using Com2Verse.QrCode.Schemas;

namespace Com2VerseEditor.QrCode
{
	public class QrCodeMakeEditor : EditorWindow
	{
		private enum SchemaType
		{
			VCARD,
		}
		private int _schemaType;

		private readonly VCardSchema _vCardSchema = new VCardSchema();

		private Texture2D _textureForPreview;
		private string _dataForPreview;

		[MenuItem("Com2Verse/QR Code Generator")]
		public static void OpenWindow()
		{
			var window = GetWindow<QrCodeMakeEditor>();
			if (window != null)
			{
				window.titleContent = new GUIContent("QR Code Generator");
				window.minSize = new Vector2(400, 800);
				window.Show();
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			// SCHEMA
			EditorGUI.BeginChangeCheck();
			{
				_schemaType = EditorGUILayout.Popup("Schema", _schemaType, Enum.GetNames(typeof(SchemaType)));
			}
			if (EditorGUI.EndChangeCheck()) _textureForPreview = null;

			EditorGUILayout.Separator();

			// SCHEMA
			EditorGUI.BeginChangeCheck();
			{
				switch ((SchemaType)_schemaType)
				{
					case SchemaType.VCARD:
					{
						_vCardSchema!.Organization = EditorGUILayout.TextField("Organization", _vCardSchema!.Organization);
						_vCardSchema!.FirstName = EditorGUILayout.TextField("First Name", _vCardSchema!.FirstName);
						_vCardSchema!.LastName = EditorGUILayout.TextField("Last Name", _vCardSchema!.LastName);
						_vCardSchema!.MiddleName = EditorGUILayout.TextField("Middle Name", _vCardSchema!.MiddleName);
						_vCardSchema!.PhoneNumber = EditorGUILayout.TextField("Phone Number", _vCardSchema!.PhoneNumber);
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			if (EditorGUI.EndChangeCheck()) _textureForPreview = null;

			EditorGUILayout.Separator();

			// GENERATE
			{
				if (GUILayout.Button("Generate"))
				{
					_dataForPreview = DataHelper.GetData(_vCardSchema);
					_textureForPreview = QrCodeMaker.Generate(_vCardSchema);
				}
			}

			EditorGUILayout.Separator();

			// PREVIEW
			{
				if (_textureForPreview != null)
				{
					EditorGUILayout.BeginHorizontal();

					var pixelRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
					EditorGUI.DrawPreviewTexture(pixelRect, _textureForPreview, null, ScaleMode.StretchToFill);

					if (GUILayout.Button("Save as.."))
					{
						var filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", "", "", "png");
						if (filePath is { Length: > 0 })
						{
							var textureBytes = _textureForPreview.EncodeToPNG();
							if (textureBytes != null)
								File.WriteAllBytes(filePath, textureBytes);
						}
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.TextArea(_dataForPreview, GUILayout.Height(120), GUILayout.ExpandWidth(true));
				}
			}

			EditorGUILayout.EndVertical();
		}
	}
}
