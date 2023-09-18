/*===============================================================
* Product:		Com2Verse
* File Name:	CodeGenerator.cs
* Developer:	jhkim
* Date:			2023-04-11 11:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using JetBrains.Annotations;

namespace Com2verseEditor.WebApi
{
	internal static partial class CodeGenerator
	{
#region Constant
		internal struct Types
		{
			public const string String = "string";
			public const string Int = "int";
			public const string Int32 = "int32";
			public const string Long = "long";
			public const string Int64 = "int64";
			public const string DateTime = "date-time";
			public const string Boolean = "boolean";
			public const string Bool = "bool";
			public const string Object = "object";
			public const string Double = "double";
		}

		private struct UtilMethods
		{
			public const string AccessToken = "AccessToken";
			public const string TrySetAuthToken = "TrySetAuthToken";
			// public const string IsTokenExist = "IsTokenExist";
		}

		public const string UsedImplicitly = "UsedImplicitly";
		private static readonly string RequestBodyPrefix = "requestBody";
#endregion // Constant

#region Variables
		private static readonly string Namespace = "Com2Verse.WebApi";
		private static readonly string NamespaceFormat = $"{Namespace}.{{0}}";
		private static readonly string ApiClassName = "Api";
		private static readonly string ComponentClassName = "Components";
		private static readonly string MemberApiUrl = "_apiUrl";
		private static readonly string PropertyApiUrl = "ApiUrl";
		private static readonly string MemberApiUrlFormat = "_apiUrlFormat";
		private static readonly string ApiUrlFormatStr = "{0}/{1}";
		private static readonly string RequestBodyParam = "requestBody";

		private static readonly string[] UsingNamespaces = new[]
		{
			"System",
			"Com2Verse.HttpHelper",
			"Cysharp.Threading.Tasks",
			"Newtonsoft.Json",
			"JetBrains.Annotations",
			// "Com2Verse.Cheat",
		};
		private static Swagger.SwaggerApi _currentApi;
#endregion // Variables

#region Generate
		internal static string GenerateSwaggerApi(Swagger.SwaggerApi api)
		{
			if (api == null) return string.Empty;

			C2VDebug.Log($"Generate Swagger Api = {api.Namespace}");
			_currentApi = api;

			var compileUnit = new CodeCompileUnit();

			var globalNamespace = CreateGlobalNamespace();
			var codeNamespace = CreateNamespace(api.Namespace);
			var apiClass = CreateClass(ApiClassName);
			var componentClass = CreateClass(ComponentClassName);

			ParseDefaultInfo(apiClass, api);
			ParseApiInfos(apiClass, api);
			ParseComponentInfos(componentClass, api.ComponentInfos);

			codeNamespace.Types.Add(apiClass);
			codeNamespace.Types.Add(componentClass);
			compileUnit.Namespaces.Add(globalNamespace);
			compileUnit.Namespaces.Add(codeNamespace);

			var provider = CodeDomProvider.CreateProvider("CSharp");
			var options = new CodeGeneratorOptions();
			options.BracingStyle = "C";

			var writer = new StringWriter();
			provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);

			var code = writer.ToString();
			code = Regex.Replace(code, "\n\n", "\n");

			// Display the generated code
			C2VDebug.Log(code);
			return code;
		}
#endregion // Generate

#region Swagger
		private static void ParseDefaultInfo(CodeTypeDeclaration apiClass, Swagger.SwaggerApi swaggerApi)
		{
			var apiUrl = new CodeMemberField(typeof(string), MemberApiUrl);
			apiUrl.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			apiUrl.InitExpression = new CodePrimitiveExpression(swaggerApi.ApiUrl);

			var apiUrlProp = CreateProperty(PropertyApiUrl);
			apiUrlProp.Type = new CodeTypeReference(typeof(string));
			apiUrlProp.Attributes = MemberAttributes.Public | MemberAttributes.Static;

			apiUrlProp.SetStatements.Add(new CodeFieldReferenceExpression {FieldName = $"{MemberApiUrl} = value"});
			apiUrlProp.GetStatements.Add(new CodeFieldReferenceExpression {FieldName = $"return {MemberApiUrl}"});

			var apiUrlFormat = new CodeMemberField(typeof(string), MemberApiUrlFormat);
			apiUrlFormat.Attributes = MemberAttributes.Private | MemberAttributes.Const;
			apiUrlFormat.InitExpression = new CodePrimitiveExpression(ApiUrlFormatStr);

			apiClass.Members.Add(apiUrl);
			apiClass.Members.Add(apiUrlProp);
			apiClass.Members.Add(apiUrlFormat);
			AddComment(apiClass, swaggerApi.Name);
		}

		private static void ParseApiInfos(CodeTypeDeclaration apiClass, Swagger.SwaggerApi api)
		{
			var apiInfos = api.ApiInfos;
			var apiMap = apiInfos.GroupBy(info => info.Tags[0]);
			foreach (var grouping in apiMap)
			{
				var groupCls = CreateClass(grouping.Key);
				foreach (var apiInfo in grouping)
				{
					var methodName = GetMethodName(apiInfo.ApiPath);
					if (string.IsNullOrWhiteSpace(methodName))
					{
						C2VDebug.LogWarning($"Invalid Method Name => {apiInfo.ApiPath}");
						continue;
					}

					var method = CreateApi(methodName, apiInfo, api.UseAuth);
					// var cheatMethod = CreateCheatMethod(methodName, apiInfo);
					groupCls.Members.Add(method);
					// groupCls.Members.Add(cheatMethod);
				}
				apiClass.Members.Add(groupCls);
			}

			CodeMemberMethod CreateApi(string methodName, Swagger.ApiInfo apiInfo, bool useAuth)
			{
				var requestTypeStr = apiInfo.RequestType.ToString();
				var clientName = nameof(Client);

				methodName = $"{ToPascalCase(requestTypeStr)}{methodName}";

				var method = CreateMethod(methodName, apiInfo);
				var comment = $"{requestTypeStr}\n {apiInfo.Summary}\n {apiInfo.ApiPath}";
				AddXmlComment(method, comment, apiInfo.Parameters);

				var urlStr = $@"$""{{{PropertyApiUrl}}}/{apiInfo.ApiPath}""";
				urlStr = urlStr.Replace("//", "/");

				if (useAuth)
					AddAuth(method, apiInfo);

				method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression {VariableName = "option"}, new CodeFieldReferenceExpression {FieldName = "option ?? RequestOption.Default"}));
				method.Statements.Add(new CodeVariableDeclarationStatement(typeof(string), "url", new CodeFieldReferenceExpression {FieldName = urlStr}));
				AddRequest(method, apiInfo);
				AddResponse(method, apiInfo);
				return method;

				void AddAuth(CodeMemberMethod m, Swagger.ApiInfo apiInfo)
				{
					var trySetAuthToken = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("!Util.Instance"), "TrySetAuthToken");
					CodeExpression returnType = new CodePrimitiveExpression(null);
					if (apiInfo.HasResponse)
					{
						var responseType = apiInfo.Response.Value.GetPropertyType();
						if (TryGetComponentInfo(responseType, out var componentInfo) && componentInfo.IsEnumComponent)
							returnType = new CodeSnippetExpression {Value = "default"};
					}
					var ifStatement = new CodeConditionStatement(trySetAuthToken, new CodeMethodReturnStatement(apiInfo.HasResponse ? returnType : null));
					m.Statements.Add(ifStatement);
				}

				void AddRequest(CodeMemberMethod m, Swagger.ApiInfo api)
				{
					var responseType = api.HasResponse ? ConvertTypeStr(api.Response.Value.GetPropertyType()) : string.Empty;
					if (api.HasRequestBody)
					{
						var messageName = nameof(Client.Message);
						AddRequestBody(m, api);

						var requestApi = GetRequestMessageApi(messageName);
						if (api.HasResponse)
						{
							m.Statements.Add(new CodeVariableDeclarationStatement("var", "response", new CodeFieldReferenceExpression {FieldName = $"{requestApi}(builder.Request, cts, option)"}));
							m.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression("response")));
						}
						else
						{
							m.Statements.Add(new CodeMethodInvokeExpression(null, requestApi, new CodeFieldReferenceExpression {FieldName = "builder.Request"}, new CodeFieldReferenceExpression {FieldName = "cts"}, new CodeFieldReferenceExpression {FieldName = "option"}));
						}
					}
					else
					{
						var requestApi = GetRequestApi();
						if (api.HasResponse)
						{
							m.Statements.Add(new CodeVariableDeclarationStatement("var", "response", new CodeFieldReferenceExpression {FieldName = $"{requestApi}(url, string.Empty, cts, option)"}));
							m.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression("response")));
						}
						else
						{
							m.Statements.Add(new CodeMethodInvokeExpression(null, requestApi, new CodeFieldReferenceExpression {FieldName = "url"}, new CodeSnippetExpression {Value = "string.Empty"}, new CodeFieldReferenceExpression {FieldName = "cts"}, new CodeFieldReferenceExpression {FieldName = "option"}));
						}
					}

					string GetRequestMessageApi(string messageName)
					{
						if (api.HasResponse)
						{
							var response = api.Response.Value.IsArray ? $"{responseType}[]" : responseType;
							return $"await {clientName}.{messageName}.Request<{response}>";
						}

						return $"await {clientName}.{messageName}.RequestStringAsync";
					}

					string GetRequestApi()
					{
						if (api.HasResponse)
						{
							var response = api.Response.Value.IsArray ? $"{responseType}[]" : responseType;
							return $"await {clientName}.{api.RequestType}.RequestAsync<{response}>";
						}
						return $"await {clientName}.{api.RequestType}.RequestStringAsync";
					}
				}

				void AddResponse(CodeMemberMethod m, Swagger.ApiInfo api)
				{
					if (api.HasResponse && !api.Response.Value.IsVoid)
					{
						var typeStr = ConvertTypeStr(api.Response.Value.GetPropertyType());
						if (api.Response.Value.IsArray)
							typeStr += "[]";

						m.ReturnType = new CodeTypeReference($"async UniTask<ResponseBase<{typeStr}>>");
					}
					else
						m.ReturnType = new CodeTypeReference("async UniTask<ResponseString>");
				}

				void AddRequestBody(CodeMemberMethod m, Swagger.ApiInfo api)
				{
					var typeParameters = new CodeExpression[]
					{
						new CodeFieldReferenceExpression {FieldName = $"{clientName}.{nameof(Client.eRequestType)}.{api.RequestType.ToString()}"},
						new CodeFieldReferenceExpression {FieldName = "url"},
					};
					var methodExpr = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("HttpRequestBuilder"), "CreateNew"), typeParameters);
					var builderVar = new CodeVariableDeclarationStatement("using var", "builder", methodExpr);
					var setContentType = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression {MethodName = "builder"}, "SetContentType", new CodeArgumentReferenceExpression($"{clientName}.{nameof(Client.Constant)}.{nameof(Client.Constant.ContentJson)}"));

					m.Statements.Add(builderVar);
					if (api.HasRequestBody)
					{
						var request = api.RequestBody.Value.RequestContents.First();
						var setContent = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression {MethodName = "builder"}, "SetContent");
						var isComponent = TryGetComponentName(request.GetRequestContentDataType(), out _);
						var requestBody = new CodeFieldReferenceExpression {FieldName = RequestBodyParam};
						if (isComponent)
						{
							// Component Type
							var serializeObject = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("JsonConvert"), "SerializeObject");
							serializeObject.Parameters.Add(requestBody);
							setContent.Parameters.Add(serializeObject);
						}
						else
						{
							// Primitive Type
							var requestBodyToString = new CodeMethodInvokeExpression(requestBody, "ToString");
							setContent.Parameters.Add(requestBodyToString);
						}
						m.Statements.Add(setContent);
						m.Statements.Add(setContentType);
					}
				}
			}
		}

		private static void ParseComponentInfos(CodeTypeDeclaration componentClass, Swagger.ComponentInfo[] componentInfos)
		{
			foreach (var componentInfo in componentInfos)
			{
				switch (componentInfo)
				{
					case Swagger.PropertyComponent propertyComponent:
					{
						var infoCls = CreateClass(componentInfo.Name);
						infoCls.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));

						foreach (var property in propertyComponent.Properties)
						{
							var memberField = CreateField(ToPascalCase(property.Name), property.GetPropertyType(), property.IsArray);
							memberField.CustomAttributes.Add(new CodeAttributeDeclaration {Name = "JsonProperty", Arguments = {new CodeAttributeArgument {Value = new CodePrimitiveExpression(property.Name)}}});
							memberField.Name += " { get; set; } //"; // HACK : 코드생성시 ; 가 추가되는 문제가 있어 주석추가

							infoCls.Members.Add(memberField);
						}

						componentClass.Members.Add(infoCls);
						break;
					}
					case Swagger.EnumComponent enumComponent:
					{
						var enumCls = CreateEnumClass(componentInfo.Name, enumComponent);
						componentClass.Members.Add(enumCls);
						break;
					}
					case Swagger.EmptyComponent:
					{
						var emptyCls = CreateClass(componentInfo.Name);
						emptyCls.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));
						componentClass.Members.Add(emptyCls);
						break;
					}
				}
			}
		}
#endregion // Swagger

#region CodeDom
		// Namespace
		private static CodeNamespace CreateGlobalNamespace()
		{
			var ns = new CodeNamespace();
			foreach (var usingNamespace in UsingNamespaces)
				ns.Imports.Add(new CodeNamespaceImport(usingNamespace));
			return ns;
		}
		private static CodeNamespace CreateNamespace(string name = null)
		{
			if (string.IsNullOrWhiteSpace(name))
				return new CodeNamespace(Namespace);

			return new CodeNamespace(string.Format(NamespaceFormat, name));
		}

		// Class
		private static CodeTypeDeclaration CreateClass(string name)
		{
			var classDeclaration = new CodeTypeDeclaration(name);
			classDeclaration.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			return classDeclaration;
		}

		private static CodeTypeDeclaration CreateEnumClass(string name, Swagger.EnumComponent enumComponent)
		{
			var classDeclaration = new CodeTypeDeclaration(name);
			classDeclaration.Attributes = MemberAttributes.Public;
			classDeclaration.IsEnum = true;

			var memberFields = enumComponent.Items.Select(item =>
			{
				var enumField = new CodeMemberField()
				{
					Name = item.Value,
					InitExpression = new CodePrimitiveExpression(item.Key),
				};
				return enumField;
			}).ToArray();

			classDeclaration.Members.AddRange(memberFields);
			return classDeclaration;
		}

		// Method
		private static CodeMemberMethod CreateMethod(string name, Swagger.ApiInfo apiInfo)
		{
			var method = new CodeMemberMethod();
			method.Name = name;
			method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			method.Parameters.AddRange(apiInfo.Parameters.Select(param => CreateParameter(param.GetSchemaDataType(), param.Name)).ToArray());

			AddRequestBodyParam(method, apiInfo.RequestBody);
			method.Parameters.Add(CreateParameter("System.Threading.CancellationTokenSource", "cts = null"));
			method.Parameters.Add(CreateParameter("RequestOption", "option = null"));
			return method;

			void AddRequestBodyParam(CodeMemberMethod method, Swagger.RequestBody? requestBodyObj)
			{
				if (!requestBodyObj.HasValue) return;

				var requestBody = requestBodyObj.Value;
				if (requestBody.RequestContents.Length == 0) return;

				var request = requestBody.RequestContents.First();
				if (TryGetComponentName(request.GetRequestContentDataType(), out var type))
					method.Parameters.Add(CreateParameter(type, RequestBodyParam));
				else
					method.Parameters.Add(CreateParameter(ConvertTypeStr(request.GetRequestContentDataType()), RequestBodyParam));
			}
		}

		private static CodeMemberMethod CreateMethod(string name, params Swagger.Parameter[] parameters)
		{
			var method = new CodeMemberMethod();
			method.Name = name;
			method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			if (parameters.Length > 0)
				method.Parameters.AddRange(parameters.Select(param => CreateParameter(param.GetSchemaDataType(), param.Name)).ToArray());
			return method;
		}

		// TODO: Cheat 미사용시 제거
		private static CodeMemberMethod CreateCheatMethod(string methodName, Swagger.ApiInfo apiInfo)
		{
			var requestTypeStr = apiInfo.RequestType.ToString();
			var clientName = nameof(Client);
			var cheatMethodName = $"Cheat{ToPascalCase(requestTypeStr)}{methodName}";

			var method = CreateMethod(cheatMethodName, apiInfo);
			AddCheatAttribute(method, apiInfo);

			method.Parameters.Clear();
			AddParameters(method, apiInfo);
			method.ReturnType = new CodeTypeReference("async UniTask");
			return method;

			void AddParameters(CodeMemberMethod m, Swagger.ApiInfo api)
			{
				m.Parameters.AddRange(api.Parameters.Select(param => CreateParameter(Types.String, param.Name)).ToArray());

				var parameters = api.Parameters.Select(param =>
				{
					var paramName = GetParamName(param.Name, param.GetDataType());
					return new CodeFieldReferenceExpression {FieldName = paramName};
				}).ToList();

				var requestBodyParameters = GetRequestBodyParameters(api.RequestBody);
				if (requestBodyParameters.Length > 0)
				{
					parameters.AddRange(requestBodyParameters.Select(request =>
					{
						var dataType = eDataTypeExtension.Get(request.Item1);
						var paramName = GetParamName(request.Item2, dataType);
						return new CodeFieldReferenceExpression {FieldName = paramName};
					}));
					m.Parameters.AddRange(requestBodyParameters.Select(request => CreateParameter(Types.String, request.Item2)).ToArray());
				}

				var apiMethodName = $"await {ToPascalCase(requestTypeStr)}{methodName}";
				m.Statements.Add(new CodeMethodInvokeExpression(null, apiMethodName, parameters.ToArray()));

				string GetParamName(string name, Swagger.eDataType type)
				{
					var paramName = name;

					switch (type)
					{
						case Swagger.eDataType.INT32:
							paramName = $"int.Parse({name})";
							break;
						case Swagger.eDataType.INT64:
							paramName = $"long.Parse({name})";
							break;
						case Swagger.eDataType.BOOLEAN:
							paramName = $"bool.Parse({name})";
							break;
						case Swagger.eDataType.STRING:
						case Swagger.eDataType.NONE:
						default:
							break;
					}

					return paramName;
				}
			}

			void AddCheatAttribute(CodeMemberMethod method, Swagger.ApiInfo apiInfo)
			{
				var attribute = new CodeAttributeDeclaration("MetaverseCheat");
				var cheatPath = $"Cheat/WebApi/{apiInfo.Tags[0]}/{GetMethodName(apiInfo.ApiPath)}";
				var attributeArgument = new CodeAttributeArgument(new CodePrimitiveExpression(cheatPath));
				attribute.Arguments.Add(attributeArgument);

				method.CustomAttributes.Add(attribute);
			}

			// return (type, name)
			(string, string)[] GetRequestBodyParameters(Swagger.RequestBody? requestBodyObj)
			{
				if (!requestBodyObj.HasValue) return Array.Empty<(string, string)>();

				var requestBody = requestBodyObj.Value;
				if (requestBody.RequestContents.Length == 0) return Array.Empty<(string, string)>();

				var result = new List<(string, string)>();
				var content = requestBody.RequestContents.First();
				switch (content.GetDataType())
				{
					case Swagger.eDataType.INT32:
						result.Add((Types.Int32, GetPrimitiveRequestBodyParam(Types.Int)));
						break;
					case Swagger.eDataType.INT64:
						result.Add((Types.Int64, GetPrimitiveRequestBodyParam(Types.Long)));
						break;
					case Swagger.eDataType.BOOLEAN:
						result.Add((Types.Boolean, GetPrimitiveRequestBodyParam(Types.Bool)));
						break;
					case Swagger.eDataType.STRING:
						result.Add((Types.String, GetPrimitiveRequestBodyParam(Types.String)));
						break;
					case Swagger.eDataType.NONE:
					default:
					{
						if (TryGetComponentInfo(content.GetRequestContentDataType(), out var info))
						{
							switch (info)
							{
								case Swagger.PropertyComponent propertyComponent:
								{
									foreach (var prop in propertyComponent.Properties)
										result.Add((prop.GetPropertyType(), prop.Name));
								}
									break;
								case Swagger.EnumComponent enumComponent:
									result.Add((enumComponent.Type, enumComponent.Name));
									break;
							}
						}
					}
						break;
				}

				return result.ToArray();
			}
		}


		static string GetPrimitiveRequestBodyParam(string typeStr) => $"{RequestBodyPrefix}{ToPascalCase(typeStr)}";
		static bool TryGetComponentInfo(string name, out Swagger.ComponentInfo info)
		{
			info = null;
			if (string.IsNullOrWhiteSpace(name)) return false;

			if (name.Contains("/"))
				name = name.Split("/")[^1];
			return _currentApi?.TryGetComponent(name, out info) ?? false;
		}

		// Parameter
		[NotNull]
		private static CodeParameterDeclarationExpression CreateParameter(string typeStr, string name) => new(ConvertTypeStr(typeStr), name);

		// Field
		private static CodeMemberField CreateField(string name, string typeStr, bool isArray = false)
		{
			var memberType = new CodeTypeReference(ConvertTypeStr(typeStr)) {ArrayRank = isArray ? 1 : 0};
			var memberField = new CodeMemberField(memberType, name);
			memberField.Attributes = MemberAttributes.Public;
			return memberField;
		}

		// Property
		private static CodeMemberProperty CreateProperty(string name)
		{
			var memberProperty = new CodeMemberProperty()
			{
				Name = name,
				Attributes = MemberAttributes.Public,
			};
			return memberProperty;
		}
		// Comment
		private static CodeCommentStatement CreateComment(string comment, bool docComment = false) => new CodeCommentStatement(comment, docComment);
		private static void AddComment(CodeTypeMember member, string comment, bool docComment = false) => member.Comments.Add(CreateComment(comment, docComment));
		private static void AddXmlComment(CodeTypeMember member, string summary, (string, string)[] parameters, string returnComment = "")
		{
			AddComment(member, "<summary>", true);
			AddComment(member, summary, true);
			AddComment(member, "</summary>", true);

			if (parameters != null)
			{
				foreach (var (name, comment) in parameters)
					AddComment(member, $"<param name=\"{name}\">{comment}</param>", true);
			}

			if (!string.IsNullOrWhiteSpace(returnComment))
				AddComment(member, $"<returns>{returnComment}</returns>");
		}
		private static void AddXmlComment(CodeTypeMember member, string summary, Swagger.Parameter[] parameters, string returnComment = "")
		{
			var xmlParameters = parameters.Select(p => (p.Name, ConvertDisplayTypeStr(p.GetSchemaDataType()))).ToArray();
			AddXmlComment(member, summary, xmlParameters, returnComment);

			string ConvertDisplayTypeStr(string typeStr)
			{
				switch (typeStr)
				{
					case Types.Int32:
						return Types.Int;
					case Types.Int64:
						return Types.Long;
					case Types.Boolean:
						return Types.Bool;
					default:
						return typeStr;
				}
			}
		}
#endregion // CodeDom

#region Util
		private static string ConvertTypeStr(string typeStr)
		{
			switch (typeStr)
			{
				case Types.String:
					return "System.String";
				case Types.Int:
				case Types.Int32:
					return "System.Int32";
				case Types.Long:
				case Types.Int64:
					return "System.Int64";
				case Types.DateTime:
					return "System.DateTime";
				case Types.Boolean:
					return "System.Boolean";
				case Types.Object:
					return "System.Object";
				case Types.Double:
					return "System.Double";
				case "array": // HACK : Primitive Array 형식에 대한 처리
					return "Array";
				default:
				{
					if (typeStr.StartsWith("#") && typeStr.Contains("/"))
					{
						var refName = typeStr.Split("/")[^1];
						return $"{ComponentClassName}.{refName}";
					}
					return typeStr;
				}
			}
		}

		private static bool TryGetComponentName(string componentPath, out string result)
		{
			result = string.Empty;
			if (componentPath.StartsWith("#") && componentPath.Contains("/"))
			{
				var refName = componentPath.Split("/")[^1];
				result = $"{ComponentClassName}.{refName}";
				return true;
			}

			return false;
		}

		private static string GetMethodName(string apiPath)
		{
			if (string.IsNullOrWhiteSpace(apiPath))
			{
				C2VDebug.LogWarning($"GetMethodName apiPath is empty");
				return string.Empty;
			}

			apiPath = Regex.Replace(apiPath, "[ -]", string.Empty);

			var tokenApi = "/api/";
			var tokenApiIdx = apiPath.IndexOf(tokenApi);
			if (tokenApiIdx != -1)
				apiPath = apiPath.Substring(tokenApiIdx + tokenApi.Length);

			var tokens = apiPath.Split("/");
			if (tokens.Length == 0) return ToPascalCase(apiPath);

			StringBuilder sb = new StringBuilder();
			for (int i = tokens.Length - 1; i >= 0; --i)
			{
				if (string.IsNullOrWhiteSpace(tokens[i]) || IsParameter(tokens[i]))
					continue;

				if (IsVersionToken(tokens[i])) break;
				sb.Insert(0, ToPascalCase(tokens[i]));
			}

			return sb.ToString();

			bool IsVersionToken(string token) => Regex.IsMatch(token, "v[0-9]+");
			bool IsParameter(string token) => token.Length > 2 && token[0] == '{' && token[^1] == '}';
		}

		// https://stackoverflow.com/questions/23345348/topascalcase-c-sharp-for-all-caps-abbreviations
		private static string ToPascalCase(string s)
		{
			var result = new StringBuilder();
			var nonWordChars = new Regex(@"[^a-zA-Z0-9]+");
			var tokens = nonWordChars.Split(s);
			foreach (var token in tokens)
				result.Append(PascalCaseSingleWord(token));

			return result.ToString();

			// https://stackoverflow.com/questions/23345348/topascalcase-c-sharp-for-all-caps-abbreviations
			string PascalCaseSingleWord(string s)
			{
				var match = Regex.Match(s, @"^(?<word>\d+|^[a-z]+|[A-Z]+|[A-Z][a-z]+|\d[a-z]+)+$");
				var groups = match.Groups["word"];

				var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
				var result = new StringBuilder();
				foreach (var capture in groups.Captures.Cast<Capture>())
				{
					result.Append(textInfo.ToTitleCase(capture.Value.ToLower()));
				}

				return result.ToString();
			}
		}
#endregion // Util
	}
}
