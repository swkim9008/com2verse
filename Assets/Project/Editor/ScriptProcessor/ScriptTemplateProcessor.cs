/*===============================================================
* Product:    ExtendingEditor
* File Name:  ScriptTemplateProcessor.cs
* Developer:  kkn
* Date:       2016-06-05 21:48
* Copyright ⓒ DefaultCompany. All rights reserved.
 ================================================================*/

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

public class ScriptTemplateProcessor : UnityEditor.AssetModificationProcessor
{
	private static readonly string DefaultNamespace = "Com2Verse";

	private static readonly string DefineNamespace = "#NAMESPACE#";
	private static readonly string DefineNoTrim = "#NOTRIM#";

	private static string _originalCS = 
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class #SCRIPTNAME# : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #NOTRIM#
    }

    // Update is called once per frame
    void Update()
    {
        #NOTRIM#
    }
}
";

	private static string _originalWithNamespaceCS =
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace #NAMESPACE#
{
    public class #SCRIPTNAME# : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        #NOTRIM#
        }

        // Update is called once per frame
        void Update()
        {
        #NOTRIM#
        }
    }
}
";
	private static string _customCS =
@"/*===============================================================
* Product:		#PROJECT_NAME#
* File Name:	#SCRIPT_NAME#.cs
* Developer:	#WRITER_NAME#
* Date:			#CREATION_DATE#
* History:		
* Documents:	
* Copyright ⓒ #COMPANY_NAME#. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace #NAMESPACE#
{
	public sealed class #SCRIPT_NAME# : MonoBehaviour
	{
#region Mono
		private void Awake()
		{
		}
#endregion	// Mono
	}
}
";

	public static byte[] bom = new byte[3] {
		0xEF,
		0xBB,
		0xBF
	};

	static public string OriginCSharp => _originalCS;

	static public string CSharp => _customCS;
	
	//---------------------------------------------------------------------------------------------

    public static void OnWillCreateAsset(string path)
    {
        string assetPath = path.Replace(".meta", "");
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        string extension = Path.GetExtension(assetPath);
        string templateScript = null;

        if(!File.Exists(assetPath))
        {
            return;
        }

        var p = File.ReadAllText(assetPath);
        switch (extension)
        {
	        case ".cs":
		        if (IsNewScript(fileName, "#SCRIPTNAME#", p, OriginCSharp, _originalWithNamespaceCS))
			        templateScript = CSharp;
		        break;
	        default:
		        return;
        }
        if (string.IsNullOrEmpty(templateScript))
            return;

        string strToday = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        templateScript = templateScript.Replace("#WRITER_NAME#", System.Environment.UserName);
        templateScript = templateScript.Replace("#CREATION_DATE#", strToday.Trim());
        templateScript = templateScript.Replace("#PROJECT_NAME#", PlayerSettings.productName.Trim());
        templateScript = templateScript.Replace("#COMPANY_NAME#", PlayerSettings.companyName);
        templateScript = templateScript.Replace("#SCRIPT_NAME#", fileName);
        templateScript = templateScript.Replace("#NAMESPACE#", TryGetNamespace(p, out var ns) ? ns : DefaultNamespace);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(templateScript);

        // 서명 있는
        if (IsExistsBom(ref bytes) == false)
        {
            byte[] newBytes = new byte[bytes.Length + bom.Length];
            System.Array.Copy(bom, newBytes, bom.Length);
            System.Array.Copy(bytes, 0, newBytes, bom.Length, bytes.Length);
            File.WriteAllBytes(assetPath, newBytes);
        }
        else
        {
            File.WriteAllText(assetPath, templateScript);
        }
        AssetDatabase.Refresh();
    }

	private static bool IsExistsBom(ref byte[] i_bytes)
	{
		if (i_bytes[0] == bom[0]
		    && i_bytes[1] == bom[1]
		    && i_bytes[2] == bom[2])
		{
			return true;
		}
		return false;
	}

	//-----------------------------------------------------------------------------------------------

	static public bool IsNewScript(string _fileName, string _scriptDefine, string _currentDesc, string _originTemplate, string _originTemplateWithNamespace)
	{
		// Without Namespace
        string compareScript = _originTemplate.Replace(_scriptDefine, _fileName);
        // 5.6 이후 부터 #NOTRIM# 추가됨
        compareScript = compareScript.Replace(DefineNoTrim, "");
        if (compareScript.Equals(_currentDesc))
	        return true;

        // With Namespace
        if (TryGetNamespace(_currentDesc, out var ns))
        {
	        compareScript = _originTemplateWithNamespace;
	        compareScript = compareScript.Replace(_scriptDefine, _fileName);
	        compareScript = compareScript.Replace(DefineNoTrim, string.Empty);
	        compareScript = compareScript.Replace(DefineNamespace, ns);
	        return compareScript.Equals(_currentDesc);
        }

        return false;
	}

	static bool TryGetNamespace(string script, out string result)
	{
		result = string.Empty;
		var match = Regex.Match(script, "namespace (.*)\r");
		if (!match.Success || match.Groups.Count <= 1) return false;
		result = match.Groups[1].Value;
		return true;
	}
}
