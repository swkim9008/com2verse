// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	CustomVectorConverter.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-13 오후 2:43
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Text;
using Com2Verse.PhysicsAssetSerialization;
using Newtonsoft.Json;

namespace Com2Verse.Builder
{
	public class CustomVector2Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			StringBuilder sb = new StringBuilder();

			Float2 floatValue = (Float2)value;
			sb.Append(ConvertUtil.FloatToHexString(floatValue.x));
			sb.Append(ConvertUtil.FloatToHexString(floatValue.y));
			writer.WriteValue(sb.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string value = reader.Value as String;

			return new Float2(ConvertUtil.HexStringToFloat(value.Substring(0, 8)), ConvertUtil.HexStringToFloat(value.Substring(8, 8)));
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Float2);
		}
	}

	public class CustomVector3Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			StringBuilder sb = new StringBuilder();

			Float3 floatValue = (Float3)value;
			sb.Append(ConvertUtil.FloatToHexString(floatValue.x));
			sb.Append(ConvertUtil.FloatToHexString(floatValue.y));
			sb.Append(ConvertUtil.FloatToHexString(floatValue.z));
			writer.WriteValue(sb.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string value = reader.Value as String;

			return new Float3(ConvertUtil.HexStringToFloat(value.Substring(0, 8)), ConvertUtil.HexStringToFloat(value.Substring(8, 8)), ConvertUtil.HexStringToFloat(value.Substring(16, 8)));
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Float3);
		}
	}

	public class CustomVector4Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			StringBuilder sb = new StringBuilder();

			Float4 floatValue = (Float4)value;
			sb.Append(ConvertUtil.FloatToHexString(floatValue.x));
			sb.Append(ConvertUtil.FloatToHexString(floatValue.y));
			sb.Append(ConvertUtil.FloatToHexString(floatValue.z));
			sb.Append(ConvertUtil.FloatToHexString(floatValue.w));
			writer.WriteValue(sb.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string value = reader.Value as String;

			return new Float4(ConvertUtil.HexStringToFloat(value.Substring(0, 8)), ConvertUtil.HexStringToFloat(value.Substring(8, 8)), ConvertUtil.HexStringToFloat(value.Substring(16, 8)), ConvertUtil.HexStringToFloat(value.Substring(24, 8)));
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Float4);
		}
	}
}
