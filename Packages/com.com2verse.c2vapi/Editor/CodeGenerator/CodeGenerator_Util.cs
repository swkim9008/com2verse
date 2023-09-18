/*===============================================================
* Product:		Com2Verse
* File Name:	CodeGenerator_Util.cs
* Developer:	jhkim
* Date:			2023-05-16 11:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

namespace Com2verseEditor.WebApi
{
	// Util
	internal static partial class CodeGenerator
	{
		internal static string GenerateUtilClass()
		{
			var compileUnit = new CodeCompileUnit();

			var globalNamespace = CreateGlobalNamespace();
			var codeNamespace = CreateNamespace();
			var utilClass = CreateClass("Util");
			var constructor = new CodeConstructor();
			constructor.CustomAttributes.Add(new CodeAttributeDeclaration {Name = UsedImplicitly});
			constructor.Attributes = MemberAttributes.Private;

			WriteUtilClass(utilClass);

			codeNamespace.Types.Add(utilClass);
			compileUnit.Namespaces.Add(globalNamespace);
			compileUnit.Namespaces.Add(codeNamespace);

			var provider = CodeDomProvider.CreateProvider("CSharp");
			var options = new CodeGeneratorOptions();
			options.BracingStyle = "C";

			var writer = new StringWriter();
			provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);

			var code = writer.ToString();
			return code;

			void WriteUtilClass(CodeTypeDeclaration utilClass)
			{
				utilClass.BaseTypes.Add("Singleton<Util>");
				utilClass.TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public;

				var accessTokenFieldName = "_accessToken";
				var accessTokenField = CreateField(accessTokenFieldName, Types.String);
				accessTokenField.Attributes = MemberAttributes.Private;

				var accessTokenProperty = CreateProperty(UtilMethods.AccessToken);
				accessTokenProperty.Type = new CodeTypeReference(typeof(string));
				accessTokenProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;

				accessTokenProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression {FieldName = accessTokenFieldName}));
				accessTokenProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression {FieldName = accessTokenFieldName}, new CodePropertySetValueReferenceExpression()));

				var setAuthTokenMethod = CreateMethod(UtilMethods.TrySetAuthToken);
				setAuthTokenMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;

				var isAccessTokenExist = new CodeMethodInvokeExpression
				{
					Method = new CodeMethodReferenceExpression
					{
						TargetObject = new CodeFieldReferenceExpression {FieldName = "!string"},
						MethodName = "IsNullOrWhiteSpace",
					},
					Parameters = {new CodeFieldReferenceExpression {FieldName = accessTokenFieldName}},
				};

				var ifTokenExistExpr = new CodeConditionStatement(isAccessTokenExist);
				var makeTokenAuthInfo = new CodeMethodInvokeExpression
				{
					Method = new CodeMethodReferenceExpression
					{
						TargetObject = new CodeFieldReferenceExpression {FieldName = "HttpHelper.Util"},
						MethodName = "MakeTokenAuthInfo",
					},
					Parameters = {new CodeFieldReferenceExpression {FieldName = UtilMethods.AccessToken}},
				};
				var setTokenAuthentication = new CodeMethodInvokeExpression
				{
					Method = new CodeMethodReferenceExpression
					{
						TargetObject = new CodeFieldReferenceExpression {FieldName = "Client.Auth"},
						MethodName = "SetTokenAuthentication",
					},
					Parameters = {makeTokenAuthInfo},
				};
				ifTokenExistExpr.TrueStatements.Add(setTokenAuthentication);

				ifTokenExistExpr.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
				ifTokenExistExpr.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

				setAuthTokenMethod.Statements.Add(ifTokenExistExpr);
				setAuthTokenMethod.ReturnType = new CodeTypeReference(typeof(bool));

				utilClass.Members.Add(constructor);
				utilClass.Members.Add(accessTokenField);
				utilClass.Members.Add(accessTokenProperty);
				utilClass.Members.Add(setAuthTokenMethod);
			}
		}
	}
}
