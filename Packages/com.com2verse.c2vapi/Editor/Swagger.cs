/*===============================================================
* Product:		Com2Verse
* File Name:	Swagger.cs
* Developer:	jhkim
* Date:			2023-04-07 09:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Com2verseEditor.WebApi
{
	internal static class Swagger
	{
		public enum eDataType
		{
			NONE,
			INT32,
			INT64,
			BOOLEAN,
			STRING,
		}

		private static readonly string ResponseSuccess = "200";
		private static readonly string ContentJson = "application/json";

		public static class Type
		{
			public const string Array = "array";
			public const string Integer = "integer";
			public const string String = "string";
		}
#region Parse
		internal static async UniTask<SwaggerApi> ParseSwaggerInfo(string url)
		{
			try
			{
				var request = UnityWebRequest.Get(url);
				await request.SendWebRequest();
				if (request.isDone && request.result == UnityWebRequest.Result.Success)
				{
					var swaggerApi = new SwaggerApi();
					var apiInfos = new List<ApiInfo>();
					var json = request.downloadHandler.text;
					var jObject = JObject.Parse(json);
					var jPaths = jObject["paths"];
					if (jPaths != null)
					{
						foreach (var jName in jPaths)
						{
							var apiName = (jName as JProperty).Name;
							var jPath = jPaths[apiName];

							foreach (var jRequestTypeObj in jPaths[apiName])
							{
								var apiInfo = new ApiInfo();
								apiInfo.ApiPath = apiName;

								var requestType = (jRequestTypeObj as JProperty).Name;
								var jRequestType = jPath[requestType];
								var tags = (jRequestType["tags"] as JArray).Select(token => token.Value<string>()).ToArray();

								TryGetValue<string>(jRequestType, "summary", out var summary);

								apiInfo.RequestType = Client.GetRequestType(requestType);
								apiInfo.Tags = tags;
								apiInfo.Summary = summary;

								apiInfo = ParseRequest(jRequestType, apiInfo);
								apiInfo = ParseResponse(jRequestType, apiInfo);

								apiInfos.Add(apiInfo);
							}
						}
					}
					swaggerApi.ApiInfos = apiInfos.ToArray();

					var componentInfos = ParseComponent(jObject);
					swaggerApi.ComponentInfos = componentInfos;
					return swaggerApi;
				}
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning($"Parse Swagger failed = {url}\n{e}");
			}

			return null;

#region Parse - Request
			ApiInfo ParseRequest(JToken jRequestType, ApiInfo apiInfo)
			{
				var jRequestBody = jRequestType["requestBody"];
				var requestBody = ParseRequestBody(jRequestBody);
				if (requestBody.HasValue)
					apiInfo.RequestBody = requestBody;

				var jParameters = jRequestType["parameters"];
				var parameters = ParseParameters(jParameters);
				apiInfo.Parameters = parameters;
				return apiInfo;
			}

			RequestBody? ParseRequestBody(JToken jRequestBody)
			{
				if (jRequestBody == null) return null;

				var jContents = jRequestBody["content"];
				var requestBody = new RequestBody();
				var requestContents = new List<RequestContent>();
				foreach (var jContent in jContents)
				{
					var requestContent = new RequestContent();
					var contentType = (jContent as JProperty).Name;
					var jContentType = jContents[contentType];
					var jSchema = jContentType["schema"];
					var jSchemaType = jSchema["type"];
					requestContent.RequestType = contentType;
					if (jSchemaType != null)
					{
						var schemaType = jSchemaType.Value<string>();
						requestContent.Type = schemaType;
						switch (schemaType)
						{
							case Type.Array:
							{
								if (jSchema.Contains("items"))
								{
									var jItems = jSchema["items"];
									if (jItems.Contains("$ref"))
									{
										var schemaRef = jItems["$ref"]?.Value<string>();
										requestContent.Ref = schemaRef;
									}

									if (jItems.Contains("type"))
									{
										var schemaRef = jItems["type"]?.Value<string>();
										requestContent.Ref = schemaRef;
									}
								}
							}
								break;
							case Type.Integer:
							{
								var format = jSchema["format"].Value<string>();
								requestContent.Format = format;
							}
								break;
							case Type.String:
								requestContent.Format = schemaType;
								break;
							default:
								C2VDebug.LogWarning($"unhandled schemaType = {schemaType}\n{jSchema}");
								break;
						}
					}
					else
					{
						if (TryGetValue<string>(jSchema, "$ref", out var refName))
							requestContent.Ref = refName;
					}

					requestContents.Add(requestContent);
				}

				requestContents.Sort((l, r) =>
				{
					if (l.RequestType == Client.Constant.ContentJson) return -1;

					return r.RequestType == Client.Constant.ContentJson ? 1 : 0;
				});

				if (!IsValid(requestContents)) return null;

				requestBody.RequestContents = requestContents.ToArray();
				return requestBody;

				bool IsValid(List<RequestContent> requests)
				{
					if (requests?.Count == 0) return false;

					var request = requests[0];
					return request.Format != null || request.Type != null || request.Ref != null;
				}
			}

			Parameter[] ParseParameters(JToken jParameters)
			{
				if (jParameters == null) return Array.Empty<Parameter>();

				var parameters = new List<Parameter>();
				foreach (var jParameter in jParameters)
				{
					var parameter = new Parameter();
					var name = jParameter["name"].Value<string>();
					var jSchema = jParameter["schema"];

					parameter.Name = name;
					if (jSchema != null)
						FillParameters(ref parameter, jSchema);
					else
						FillParameters(ref parameter, jParameter);

					parameters.Add(parameter);
				}
				return parameters.ToArray();

				void FillParameters(ref Parameter parameter, JToken jParameter)
				{
					if (TryGetValue<string>(jParameter, "type", out var type))
						parameter.SchemaType = type;
					if (TryGetValue<string>(jParameter, "format", out var format))
						parameter.SchemaFormat = format;
					if (TryGetValue<string>(jParameter, "$ref", out var refName))
						parameter.Ref = refName;
				}
			}
#endregion // Parse - Request

#region Parse - Response
			static ApiInfo ParseResponse(JToken jRequestType, ApiInfo apiInfo)
			{
				var jResponses = jRequestType["responses"];
				if (jResponses != null)
				{
					foreach (var jResponseObj in jResponses)
					{
						var retCode = (jResponseObj as JProperty).Name;
						if (retCode == ResponseSuccess)
						{
							var jResponse = jResponses[retCode];
							TryGetValue<string>(jResponse, "description", out var description);
							var jContents = jResponse["content"];
							if (jContents != null)
							{
								foreach (var jContextObj in jContents)
								{
									var contentType = (jContextObj as JProperty).Name;
									if (contentType == ContentJson)
									{
										var jContent = jContents[contentType];
										var jSchema = jContent["schema"];
										var response = ParseResponseInternal(jSchema);
										// response.ContentType = contentType;
										apiInfo.Response = response;
										break;
									}
								}
							}
							else
							{
								var response = ParseResponseInternal(jResponse["schema"]);
								apiInfo.Response = response;
							}
						}
					}
				}

				return apiInfo;

				Response? ParseResponseInternal(JToken jSchema)
				{
					if (jSchema == null) return null;

					var response = new Response();
					TryGetValue<string>(jSchema, "type", out var type);
					TryGetValue<string>(jSchema, "$ref", out var refName);

					response.Type = type;
					response.Ref = refName;

					if (type == Type.Array)
					{
						var jItems = jSchema["items"];
						if (jItems != null)
						{
							TryGetValue<string>(jItems, "$ref", out refName);
							response.Ref = refName;
						}
					}
					return response;
				}
			}
#endregion // Parse - Response

#region Parse - Components
			static ComponentInfo[] ParseComponent(JToken jObject)
			{
				var jComponents = jObject["components"];
				if (jComponents != null)
				{
					var jSchemas = jComponents["schemas"];
					var componentInfos = ParseComponentsInternal(jSchemas);
					return componentInfos.ToArray();
				}
				else
				{
					var jDefinitions = jObject["definitions"];
					if (jDefinitions != null)
					{
						var componentInfos = ParseComponentsInternal(jDefinitions);
						return componentInfos.ToArray();
					}
				}

				return Array.Empty<ComponentInfo>();
			}

			static List<ComponentInfo> ParseComponentsInternal(JToken jComponents)
			{
				var componentInfos = new List<ComponentInfo>();
				foreach (var jSchemaName in jComponents)
				{
					var schemaName = (jSchemaName as JProperty).Name;
					var jSchema = jComponents[schemaName];
					var schemaType = jSchema["type"].Value<string>();

					ComponentInfo componentInfo = null;

					var jProperties = jSchema["properties"];
					if (jProperties != null)
					{
						var propertyInfos = new List<PropertyInfo>();
						foreach (var jPropertyName in jProperties)
						{
							var propertyInfo = new PropertyInfo();
							var propertyName = (jPropertyName as JProperty).Name;

							propertyInfo.Name = propertyName;
							var jProperty = jProperties[propertyName];
							if (TryGetValue<string>(jProperty, "type", out var type))
								propertyInfo.Type = type;

							FillProperty(ref propertyInfo, jProperty);
							propertyInfos.Add(propertyInfo);
						}

						componentInfo = new PropertyComponent();
						(componentInfo as PropertyComponent).Properties = propertyInfos.ToArray();
					}

					var jEnums = jSchema["enum"];
					if (jEnums != null)
					{
						componentInfo = new EnumComponent();

						var enumComponent = componentInfo as EnumComponent;
						foreach (var jEnum in jEnums)
						{
							// Enum Value는 int로 고정. 문제시 확인
							var value = jEnum.Value<int>();
							value = value < 0 ? -value : value; // HACK : 음수 키가 있는 경우 양수로 변환 (Swagger 수정 필요)
							if (!enumComponent.Items.ContainsKey(value))
								enumComponent.Items.Add(value, string.Empty);
						}

						var jEnumDescription = jSchema["description"];
						if (jEnumDescription != null)
						{
							var jsonDescription = jEnumDescription.Value<string>();
							jsonDescription = jsonDescription.Replace("-", string.Empty); // HACK : 음수 키가 있는 경우 양수로 변환 (Swagger 수정 필요)
							var jDescription = JObject.Parse(jsonDescription);
							if (jDescription != null)
							{
								foreach (var (value, name) in jDescription)
								{
									if (int.TryParse(value, out var intValue))
									{
										if (enumComponent.Items.ContainsKey(intValue))
											enumComponent.Items[intValue] = name.Value<string>();
									}
								}
							}
						}
					}

					componentInfo ??= new EmptyComponent();

					componentInfo.Name = schemaName;
					componentInfo.Type = schemaType;
					componentInfos.Add(componentInfo);
				}

				return componentInfos;
			}

			static void FillProperty(ref PropertyInfo propertyInfo, JToken jProperties)
			{
				if (TryGetValue<string>(jProperties, "format", out var format))
					propertyInfo.Format = format;
				if (TryGetValue<string>(jProperties, "type", out var type))
					propertyInfo.Type = type;
				if (TryGetValue<string>(jProperties, "$ref", out var refName))
					propertyInfo.Ref = refName;
				if (TryGetValue<int>(jProperties, "minLength", out var min))
					propertyInfo.Min = min;
				if (TryGetValue<int>(jProperties, "maxLength", out var max))
					propertyInfo.Min = max;

				if (!string.IsNullOrWhiteSpace(type) && type == Type.Array)
				{
					propertyInfo.IsArray = true;

					var jItems = jProperties["items"];

					if (TryGetValue(jItems, "format", out format))
						propertyInfo.Format = format;
					if (TryGetValue(jItems, "type", out type))
						propertyInfo.Type = type;
					if (TryGetValue(jItems, "$ref", out refName))
						propertyInfo.Ref = refName;
				}
			}
#endregion // Parse - Components
		}
#endregion // Parse

#region Json
		static bool TryGetValue<T>(JToken token, string key, out T result)
		{
			result = default;

			if (token?[key] != null)
			{
				result = token[key].Value<T>();
				return true;
			}

			return false;
		}
#endregion // Json

#region Data
		internal struct SwaggerInfo
		{
			public string Name;
			public string Namespace;
			public string ApiUrl;
			public string SwaggerUrl;
			public string JsonUrl;
			public bool UseAuth;
		}

		public class SwaggerApi
		{
			public string Name;
			public string Namespace;
			public string ApiUrl;
			public ApiInfo[] ApiInfos;
			public ComponentInfo[] ComponentInfos;
			public bool UseAuth;
			public bool TryGetComponent(string name, out ComponentInfo info)
			{
				info = null;
				if (ComponentInfos == null) return false;

				var idx = Array.FindIndex(ComponentInfos, info => info?.Name?.ToLower() == name.ToLower());
				if (idx != -1)
					info = ComponentInfos[idx];

				return idx != -1;
			}
		}
		internal struct ApiInfo
		{
			public string ApiPath;
			public Client.eRequestType RequestType;
			public string[] Tags;
			public string Summary;
			public RequestBody? RequestBody;
			public Parameter[] Parameters;
			public Response? Response;

			public readonly bool HasParameter => Parameters?.Length > 0;
			public readonly bool HasRequestBody => RequestBody != null;
			public readonly bool HasResponse => Response != null;
		}

		internal struct Parameter
		{
			public string Name;
			public string SchemaType;
			[CanBeNull] public string SchemaFormat;
			[CanBeNull] public string Ref;

			public bool IsRef => Ref != null;
			public string GetSchemaDataType()
			{
				if (IsRef) return Ref;

				return SchemaFormat ?? SchemaType;
			}

			public eDataType GetDataType() => IsRef ? eDataType.NONE : SwaggerUtil.GetDataType(GetSchemaDataType());
		}

		internal struct RequestBody
		{
			public RequestContent[] RequestContents;
		}

		internal struct RequestContent
		{
			public string RequestType;
			[CanBeNull] public string Ref;
			[CanBeNull] public string Type;
			[CanBeNull] public string Format;

			public bool IsRef => Ref != null;
			public string GetRequestContentDataType()
			{
				if (IsRef) return Ref;

				return Format ?? Type;
			}

			public eDataType GetDataType() => IsRef ? eDataType.NONE : SwaggerUtil.GetDataType(GetRequestContentDataType());
		}

		internal abstract record ComponentInfo
		{
			public string Name;
			public string Type;
			public bool IsArray => Type == Swagger.Type.Array;
			public bool IsPropertyComponent => this is PropertyComponent;
			public bool IsEnumComponent => this is EnumComponent;
		}
		internal record PropertyComponent : ComponentInfo
		{
			public PropertyInfo[] Properties;
		}
		internal record EnumComponent : ComponentInfo
		{
			public Dictionary<int, string> Items;

			public EnumComponent()
			{
				Items = new Dictionary<int, string>();
			}
		}

		internal record EmptyComponent : ComponentInfo
		{
		}
		internal struct PropertyInfo
		{
			public string Name;
			public string Type;
			[CanBeNull] public string Format;
			[CanBeNull] public string Ref;
			public int? Min;
			public int? Max;
			public bool? Nullable;
			public bool IsArray;
			public bool IsRef => Ref != null;
			public string GetPropertyType()
			{
				if (IsRef) return Ref;

				return Format ?? Type;
			}
			public eDataType GetDataType() => IsRef ? eDataType.NONE : SwaggerUtil.GetDataType(GetPropertyType());
		}

		internal struct Response
		{
			public string Type;
			[CanBeNull] public string Ref;
			public bool IsRef => Ref != null;
			public bool IsVoid => string.IsNullOrWhiteSpace(Type) && !IsRef;
			public bool IsArray => Type == Swagger.Type.Array;
			public string GetPropertyType() => IsRef ? Ref : Type;
		}

		private static class SwaggerUtil
		{
			public static eDataType GetDataType(string typeStr) => typeStr.ToLower() switch
			{
				"int32" => eDataType.INT32,
				"int64" => eDataType.INT64,
				"string" => eDataType.STRING,
				"boolean" => eDataType.BOOLEAN,
				_ => eDataType.NONE,
			};
		}
#endregion // Data
	}

	static class eDataTypeExtension
	{
		public static Swagger.eDataType Get(string dataTypeStr)
		{
			switch (dataTypeStr)
			{
				case CodeGenerator.Types.String:
					return Swagger.eDataType.STRING;
				case CodeGenerator.Types.Int:
				case CodeGenerator.Types.Int32:
					return Swagger.eDataType.INT32;
				case CodeGenerator.Types.Int64:
				case CodeGenerator.Types.Long:
					return Swagger.eDataType.INT64;
				case CodeGenerator.Types.Bool:
				case CodeGenerator.Types.Boolean:
					return Swagger.eDataType.BOOLEAN;
				default:
					return Swagger.eDataType.NONE;
			}
		}
	}
}
