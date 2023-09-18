import java.io.File
import java.io.FileWriter
import java.text.SimpleDateFormat
import org.apache.commons.io.FileUtils
import org.codehaus.groovy.runtime.DefaultGroovyMethods

import groovy.util.Node
import groovy.util.XmlParser
import groovy.util.XmlSlurper
import groovy.xml.XmlUtil
import groovy.xml.StreamingMarkupBuilder
import groovy.json.JsonSlurper

void initInfo(){
	echo "\n@ initInfo\n"
	initPath()
	initTime()
	initLogFileName()
	initPackageType()
	initAppVersionTmp()
}

void initPath(){
buildPath = ""
	echo "\n@ initPath\n"
	executablePath = "${WORKSPACE}\\Build\\Executable"
	buildReportPath = "${WORKSPACE}\\Build\\Report\\BuildReport.json"
	buildPath = "${executablePath}\\${getBuildTargetOSStr()}\\${DistributeType}"
	buildPathBase = "Build\\Executable\\${getBuildTargetOSStr()}\\${DistributeType}"
	buildExeFilePath = "${buildPath}\\${AppName}.exe"
	buildGameAssemblyDllFilePath = "${buildPath}\\GameAssembly.dll"
	isccExePath = "C:\\Program Files (x86)\\Inno Setup 6\\iscc.exe"
	msiWrapperExePath = "C:\\Program Files (x86)\\MSI Wrapper\\MsiWrapperBatch.exe"
	jenkinsScriptsPath = "${WORKSPACE}\\JenkinsScripts"
	issFilePath = "${jenkinsScriptsPath}\\inno_setup_script.iss"
	msiWrapperXmlFilePath = "${jenkinsScriptsPath}\\msiwrapper.xml"
	codeSignFilePath = "${jenkinsScriptsPath}\\CodeSign_Windows.bat"
	launcherPatchMakerFilePath = "${jenkinsScriptsPath}\\CrossplayLauncher\\cui\\patch_maker\\cpflPtc.exe"
	launcherUploaderFilePath = "${jenkinsScriptsPath}\\CrossplayLauncher\\cui\\uploader\\cpflupl.exe"
	outputPath = "${WORKSPACE}\\Build\\OutPut"
	exeFilePath = "${outputPath}\\${AppName}.exe"
	msiFilePath = "${outputPath}\\${AppName}.msi"
	zipFilePath = "${outputPath}\\${AppName}.zip"
	zipFilePathBase = "\\Build\\OutPut\\${AppName}.zip"
	
	// Themida
	themidaExePath = "D:\\Themida\\Themida_3.1.4.0\\Themida64.exe"
	themidaSettingsPath = "D:\\Themida\\Themida_3.1.4.0\\ThemidaSettings.tmd"
	buildInputGameAssemblyDllPath = "${buildPath}\\GameAssembly.dll"
	buildOutputGameAssemblyDllPath = "${buildPath}\\GameAssembly_protected.dll"
	
	// NaverWorks Token
	naverWorksTokenFolderPath = "D:\\NaverWorksToken"
	naverWorksTokenDateFilePath = "${naverWorksTokenFolderPath}\\TokenDateFile.txt"
	naverWorksTokenFilePath = "${naverWorksTokenFolderPath}\\TokenFile.txt" 

	// Launcher
	launcherWorkPath = "${WORKSPACE}\\Launcher"
	launcherPreWorkPath = "${WORKSPACE}\\Launcher\\Pre_Version_Folder"
	launcherCurWorkPath = "${WORKSPACE}\\Launcher\\Cur_Version_Folder"
	launcherPatchWorkPath = "${WORKSPACE}\\Launcher\\Patch_Files_Folder"
	launcherMetadatasWorkPath = "${WORKSPACE}\\Launcher\\metadatas"
	configPath = "${WORKSPACE}\\Config"
}

todayTime = ""
appCenterCommentTime = ""
void initTime(){
	echo "\n@ initTime\n"
	def date = new Date()
	def dateFormat = new SimpleDateFormat("yyMMddHHmm")
	todayTime = dateFormat.format(date)
	def dateFormatAppcenter = new SimpleDateFormat("yy/MM/dd HH:mm:ss")
	appCenterCommentTime = dateFormatAppcenter.format(date)
}

def getCurrentTime(){
	def date = new Date()
	return date.getTime()
}

void initLogFileName(){
	echo "\n@ initLogFileName\n"
	buildBeforeLogFile = "${WORKSPACE}/Logs/${todayTime}_buildBefore.log"
	buildLogFile = "${WORKSPACE}/Logs/${todayTime}_build.log"
	assetBundleUploadLogFile = "${WORKSPACE}/Logs/${todayTime}_assetBundleUpload.log"
}

void initPackageType(){
	echo "\n@ initPackageType\n"
	echo "PackageType - ${PackageType}\n"
	switch("${PackageType}".toLowerCase()){
		case "zip":
			isBuildBefore = true
			isBuild = true
			isProtect = true
			isExeInstaller = false
			isMsi = false
			isCodeSign = true
			isZip = true
			isAppCenter = true
			isLauncher = false
			isAssetBundleUpload = true
			codeSignTargetFilePath = "${buildExeFilePath}"
			appcenterUploadTargetFilePath = "${zipFilePath}"
			break

		case "msi":
			isBuildBefore = true
			isBuild = true
			isProtect = false
			isExeInstaller = true
			isMsi = true
			isCodeSign = true
			isZip = false
			isAppCenter = true
			isLauncher = false
			isAssetBundleUpload = true
			codeSignTargetFilePath = "${msiFilePath}"
			appcenterUploadTargetFilePath = "${msiFilePath}"
			break

		case "launcher":
			isBuildBefore = true
			isBuild = true
			isProtect = true
			isExeInstaller = false
			isMsi = false
			isCodeSign = true
			isZip = false
			isAppCenter = false
			isLauncher = true
			isAssetBundleUpload = true
			codeSignTargetFilePath = "${buildExeFilePath}"
			appcenterUploadTargetFilePath = ""
			break

		case "asset":
			isBuildBefore = true
			isBuild = false
			isProtect = false
			isExeInstaller = false
			isMsi = false
			isCodeSign = false
			isZip = false
			isAppCenter = false
			isLauncher = false
			isAssetBundleUpload = true
			codeSignTargetFilePath = "${buildExeFilePath}"
			appcenterUploadTargetFilePath = ""
			break

		case "not":
			isBuildBefore = true
			isBuild = true
			isProtect = false
			isExeInstaller = false
			isMsi = false
			isCodeSign = false
			isZip = false
			isAppCenter = false
			isLauncher = false
			isAssetBundleUpload = false
			codeSignTargetFilePath = ""
			appcenterUploadTargetFilePath = ""
			break
	}
}

void initAppVersionTmp(){
	echo "\n@ initAppVersionTmp\n"
	appVersionTmp = getVersionFromProjectSettings()
	if(!appVersionTmp){
		error("Get Unity BundleVersion Fail")
		return
	}
	echo "appVersionTmp : ${appVersionTmp}"
}

def getVersionFromProjectSettings() {
	projectSettingsPath = "${WORKSPACE}\\ProjectSettings\\ProjectSettings.asset"
	File projectSettingsFile = new File(projectSettingsPath)
	if(projectSettingsFile.exists()){
		def lines = projectSettingsFile.readLines()
		def versionPattern = ~/bundleVersion:\s+(\S+)/
		for (line in lines) {
			def matcher = versionPattern.matcher(line)
			if (matcher.find()) {
				return matcher.group(1)
			}
		}
	}
	return null
}

void gitUpdate(){
	echo "\n@ gitUpdate\n"
	// clean
	// bat(script: "git remote prune origin")
	// bat(script: "git clean -fd")
	// bat(script: "git checkout .")

	// git
	// git branch: '${GitBranch}', credentialsId: 'jbmw_01', url: 'https://meta-bitbucket.com2us.com/scm/c2verse/c2vclient.git'

	// submodule update
	// bat(script: "git -c diff.mnemonicprefix=false -c core.quotepath=false --no-optional-locks submodule update --init Assets/Project/Bundles/100001/TableData")
	// bat(script: "git -c diff.mnemonicprefix=false -c core.quotepath=false --no-optional-locks submodule update --init Assets/Project/Protocols")
	// bat(script: "git submodule update --recursive")
	// bat(script: "git submodule update --remote --recursive")
}

void gitInfo(){
	log = "\n@ gitInfo\n"
	gitCommitHash = bat(script: "@git rev-parse --short HEAD", returnStdout: true).trim()
	gitRevisionCount = bat(script: "@git rev-list --count HEAD", returnStdout: true).trim()

	// pipeline scm case - head.
	// gitBranchName = bat(script: "@git branch --show-current", returnStdout: true).trim()
	gitBranchName = "${GitBranch}"
}

def getCommitHashLink(){
	return "[${gitCommitHash}](https://meta-bitbucket.com2us.com/projects/C2VERSE/repos/c2vclient/commits/${gitCommitHash})";
}

void initAfter(){
	echo "\n@ initAfter\n"
	envTypeFull = "${EnvType}".toLowerCase()
	envTypeShort = envTypeFull[0]
    if("${envTypeFull}" == "dev_integration")
    {
        envTypeShort = "di"
    }
	String[] version_token = appVersionTmp.tokenize('.')
	appVersion = "${version_token[0]}.${version_token[1]}.${gitRevisionCount}_${envTypeShort}"
	appVersionMsi = "${version_token[0]}.${version_token[1]}.${gitRevisionCount}.0"
	IsForceSingleInstance = "${DistributeType}".toLowerCase() == "debug" ? false : true

	// launcher
	hiveEnvStr = "${HiveEnv}".toLowerCase()
	hiveEnvParam = "${hiveEnvStr}" == "sandbox" ? "-e=SANDBOX" : ""
	isHiveEnvSandbox = "${hiveEnvStr}" == "sandbox"
	hiveReleaseParam = IsHiveRelease ? "--release" : ""
	isLauncherError = false
	isLauncherErrorMsg = ""
	launcherBasePath = "D:\\Builds\\${getBuildTargetStr()}\\"
	if("${AppID}" == "QA.windows.hivepc.global.normal"){
		launcherBasePath += "test"
	}
	else{
		launcherBasePath += "${envTypeFull}"
		if(isHiveEnvSandbox){
			launcherBasePath += "_${hiveEnvStr}"
		}
	}
	preVersionFilePath = "${launcherBasePath}\\pre_version.txt"
	File preVersionFile = new File(preVersionFilePath)
	appVersionPre = "default"
	if(preVersionFile.exists()){
		appVersionPre = preVersionFile.text
		if(appVersion == appVersionPre){
			isLauncherError = true
			isLauncherErrorMsg = "already uploaded version - ${appVersion}"
		}
	}
	else{
		isLauncherError = true
		isLauncherErrorMsg = "pre version file not exists"
	}

	launcherPreBuildPath = "${launcherBasePath}\\${appVersionPre}"
	launcherCurBuildPath = "${launcherBasePath}\\${appVersion}"
	launcherCurMetadataPath = "${launcherBasePath}\\${appVersion}_metadatas"
}

void sendEmails(boolean isSuccess = true){
    // send email
    echo "emails : ${getEmails()}"
    def emailMessage = getBuildResultCommentString(isSuccess)
    emailext body : "${emailMessage}",
       subject : "[배포-${EnvType}(${DistributeType})-v${appVersion}] 컴투버스 클라이언트 배포",
       to : getEmails()
}

void sendNaverWorks(boolean isSuccess = true){
    // naver Works
    makeFolder("${naverWorksTokenFolderPath}")
    
    def accessToken = getNaverWorksAccessToken() //"kr1AAABJyOjstr8avCdNjp8JtAn9o2/n2tNjiTv/C/iQ+IlAWG461Vz2QHPSOP7U9W7Bc1NV77n5RNuF2SXPAPP4snrj/sE71SUiVUujmuF2rn1Mn5RsGGny5GEWFgy9VurCs6arVK5KZLpz208vOzk35ItUv/LlmYlunaddYPPcTDUFEQNSnQzoGbL6GTGr5WBu/f0PvQ+J4bFDhCoXRmD+si2AV/KwVOSg1dOUy5hacCG7jS3xrEvUia5NDRvOcIIgLaYKkb56BDIFBXYpbq+/L8pPbYEhmUBbt3O4/VJvUpqbMhHSNVKC7ZqMPAbc9QpruZdB/GLyV3KU5sbBHq8VcEjppZzQweF8QvvfLXjKm6lFibf5t2tnZGUjZWTm2ZvUTdXhGL1T5/z0LIL3j6Zja/j6oM=.kwiu9yNovfcs8Rumz2QSOg"
    def message =  makeJenkinsMessage(isSuccess)
    def url = "https://www.worksapis.com/v1.0/bots/5502051/channels/1acb48a0-ca9f-0ad1-e425-5edd714c5060/messages";
    
    bat "curl -v -H \"Content-Type: application/json\" -H \"Authorization: Bearer $accessToken\" --data $message $url"
}

def getAppCenterDistributeGroup(){
	// Collaborators (전체) , Client , QA
	return "${AppCenterDistributeGroup}"
}

def getAppCenterComment(){
	def comment = "${AppCenterComment}"
	if(!comment.trim() || comment == "daily_build"){
		// ex) Windows il2cpp (release)
		comment = "${getBuildTargetStr()} ${ScriptingBackend} (${DistributeType})"
		if(comment == "daily_build"){
			// ex) Windows il2cpp (release) - daily build
			comment += " - daily build"
		}
	}
	return comment
}

def getAppCenterAppName(){
	// Com2Verse/Windows_Dev
	// Com2Verse/Windows_Dev_Debug
	// Com2Verse/Windows_QA
	// Com2Verse/Windows_QA_Debug
	// Com2Verse/Windows_Staging
	// Com2Verse/Windows_Staging_Debug
	// Com2Verse/Windows_Production
	// Com2Verse/Windows_Production_Debug
	// Com2Verse/Windows_IL2CPP
	def appName = "${AppCenterAppName}"
	if(!appName.trim()){
		appName = "Com2Verse/${getBuildTargetStr()}"
		switch(envTypeFull) {
			case "dev":
				appName += "_Dev"
				break
			case "qa":
				appName += "_QA"
				break
            case "dev_integration":
                appName += "_Dev_Integration"
				break
			case "staging":
				appName += "_Staging"
				break
			case "production":
				appName += "_Production"
				break
		}
		if("${DistributeType}" == "debug"){
			appName += "_Debug"
		}
	}
	return appName
}

def getLauncherPlatform(){
	//TODO: Mr.Song - platform 별 코드 정리.
	return "w"
}

def getBuildTargetStr(){
	//TODO: Mr.Song - platform 별 코드 정리.
	// Android
	// iOS
	// MacOSX
	return "Windows"
}

def getBuildTargetOSStr(){
	//TODO: Mr.Song - platform 별 코드 정리.
	return "Windows64"
}

def getAppCenterCommentFilePath(){
	return "${outputPath}\\.AppCenterComment.md"
}

def getOX(boolean check){
	return check ? "O" : "X"
}

def getScriptDefineSymbols(){
    def jsonFile = new File("${buildReportPath}")
    def jsonSlurper = new JsonSlurper()
    def json = jsonSlurper.parseText(jsonFile.text);
    def symbols = json.scriptDefineSymbols;
    return symbols
}

def getEmails(){
     def clientEmails = "tlghks1009@com2us.com, jehyun@com2us.com, mikeyid77@com2us.com, swkim@com2us.com, jhkim@com2us.com, sun7302@com2us.com, pjhara@com2us.com, slew61@com2us.com, yangsehoon@com2us.com, qswwsq78@com2us.com, mdpuff@com2us.com, soulcookie@com2us.com, ralf1204@com2us.com, haminjeong@com2us.com, eugene9721@com2us.com"
     def allEmails = "tlghks1009@com2us.com, CV-DD@com2us.com"
     def emails = allEmails
     echo "${envTypeFull}"
     
     if("${envTypeFull}" == "dev"){
         emails = clientEmails
     }
     return emails
}

MAX_COMMENT_LENGTH = 5000
def getAppCenterCommentString(boolean limit = false){
	def comment = ""
	comment += "# ${AppName} v${appVersion} (${DistributeType})\n"
	comment += "## ${getAppCenterComment()}\n"
	comment += "### ${appCenterCommentTime}\n"
	comment += "### App Info\n"
	// comment += "- Build Target : {info.BuildTarget.ToString()}\n"		// ex) StandaloneWindows64
	// comment += "- Build Group : {info.BuildTargetGroup.ToString()}\n"	// ex) Standalone
	comment += "- Environment : ${EnvType}\n"
	comment += "- Scripting Backend : ${ScriptingBackend}\n"
	comment += "- Asset Build Type : ${AssetBuildType}\n"
	comment += "- IsProtected : ${getOX(isProtect)}\n"
	comment += "- IsForceSingleInstance : ${getOX(IsForceSingleInstance)}\n"
	comment += "- IsForceEnableAppInfo : ${getOX(ForceEnableAppInfo.toBoolean())}\n"
	comment += "- IsForceEnableSentry : ${getOX(ForceEnableSentry.toBoolean())}\n"
	comment += "- EnableLogging : ${getOX(EnableLogging.toBoolean())}\n"
	comment += "### Git Info\n"
	comment += "- Branch : ${gitBranchName}\n"
	// comment += "- HASH : ${gitCommitHash}\n"
	comment += "- HASH : ${getCommitHashLink()}\n"
	comment += "- Revision : ${gitRevisionCount}\n"
	comment += "### Installer Info\n"
	comment += "- PackageType : ${PackageType}\n"
	comment += "- Exe Installer : ${getOX(isExeInstaller)}\n"
	comment += "- Exe to Msi : ${getOX(isMsi)}\n"
	comment += "- CodeSign : ${getOX(isCodeSign)}\n"
	comment += "- Zip : ${getOX(isZip)}\n"
	comment += "- AppCenter : ${getOX(isAppCenter)}\n"
	comment += "- AssetBundleUpload : ${getOX(isAssetBundleUpload)}\n"
	// comment += "- Launcher : ${getOX(isLauncher)}\n"
	comment += "${getChangeLogs(limit,comment.length())}"
	return comment;
}

def getBuildResultCommentString(boolean isSuccess, boolean limit = false){
	def comment = ""
	def buildResultString = "FAILURE"
	if (isSuccess)
	   buildResultString = "SUCCESS"
	   
	comment += "1. 배포 일시 : ${appCenterCommentTime}\n"
	comment += "2. 배포 정보 : ${PackageType}\n"
	comment += "3. 빌드 결과 : ${env.JOB_NAME} - Build # ${env.BUILD_NUMBER} - ${buildResultString}\n"
	comment += "4. 빌드 버전 : ${AppName} v${appVersion} (${DistributeType})\n"
	comment += "5. 빌드 정보\n"
	if("${PackageType}".toLowerCase() == "asset")
	{
        comment += "- Environment : ${EnvType}\n"
        comment += "- Asset Build Type : ${AssetBuildType}\n"
    }
    else
    {
        comment += "- CodeSign : ${getOX(isCodeSign)}\n"
        comment += "- Environment : ${EnvType}\n"
        comment += "- Scripting Backend : ${ScriptingBackend}\n"
        comment += "- Asset Build Type : ${AssetBuildType}\n"
        comment += "- IsForceSingleInstance : ${getOX(IsForceSingleInstance)}\n"
        comment += "- IsProtected : ${getOX(isProtect)}\n"
        comment += "- IsForceEnableAppInfo : ${getOX(ForceEnableAppInfo.toBoolean())}\n"
        comment += "- IsForceEnableSentry : ${getOX(ForceEnableSentry.toBoolean())}\n"
        comment += "- EnableLogging : ${getOX(EnableLogging.toBoolean())}\n"
        comment += "- Scripting Define Symbols : ${getScriptDefineSymbols()}\n"
    }
    comment += "6. 저장소 정보\n"
    comment += "- Branch : ${gitBranchName}\n"
    comment += "- HASH : ${getCommitHashLink()}\n"
    comment += "- Revision : ${gitRevisionCount}\n\n"
    comment += "${getChangeLogs(limit,comment.length())}"
    return comment
}

def getChangeLogs(boolean limit = false, int preTextLength = 0){
	def result = "### Change Logs\n"
	def tempText = ""
	def textLength = preTextLength + result.length()
	def changeLogSets = currentBuild.changeSets
	// def changeLogSets = currentBuild.rawBuild.changeSets

	def index = 1
	// 옛날 우선 순서.
	// for (int i = 0; i < changeLogSets.size(); i++){
	// 	def entries = changeLogSets[i].items
	// 	for (int j = 0; j < entries.length; j++){
	// 		def entry = entries[j]
	// 		tempText = "${index++}. ${entry.msg} by ${entry.author}\n"
	// 		textLength += tempText.length()
	// 		if(MAX_APPCENTER_COMMENT_LENGTH <= textLength){
	// 			return result
	// 		}
	// 		result += tempText
	// 	}
	// }

	// 최신 우선 순서.
	for (int i = changeLogSets.size()-1; 0 <= i ; i--){
		def entries = changeLogSets[i].items
		for (int j = entries.length-1; 0 <= j ; j--){
			def entry = entries[j]
			tempText = "${index++}. ${entry.msg} by ${entry.author}\n"
			textLength += tempText.length()
			if(MAX_COMMENT_LENGTH <= textLength){
				return result
			}
			result += tempText
		}
	}
	return result
}


def shouldMakeTokenRequest() {
    def date = new Date()
    def dateFormat = new SimpleDateFormat("yyMMdd")
    currentDateTime = dateFormat.format(date)
    
    File naverWorksTokenDateFile = new File(naverWorksTokenDateFilePath)
    if(naverWorksTokenDateFile.exists())
    {
        if(currentDateTime == naverWorksTokenDateFile.text)
        {
            // 발급 X
            return false                                        
        }
        else
        {
            return true
        }
    }
    return true                
}


def makeJenkinsMessage(boolean isSuccess) {
    def buildResultString = "FAILURE"
	if (isSuccess)
	   buildResultString = "SUCCESS"

	def jsonStr = "\"{\\\"content\\\":{\\\"type\\\":\\\"flex\\\",\\\"altText\\\":\\\"빌드완료\\\",\\\"contents\\\":{\\\"type\\\":\\\"bubble\\\",\\\"body\\\":{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"vertical\\\",\\\"contents\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"${AppName} v${appVersion}(${DistributeType})\\\",\\\"wrap\\\":true,\\\"weight\\\":\\\"bold\\\",\\\"size\\\":\\\"xl\\\",\\\"color\\\":\\\"#222222\\\",\\\"margin\\\":\\\"none\\\"},{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"horizontal\\\",\\\"contents\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"배포일시\\\",\\\"wrap\\\":true,\\\"flex\\\":3,\\\"size\\\":\\\"xs\\\",\\\"color\\\":\\\"#222222\\\"},{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"${appCenterCommentTime}\\\",\\\"wrap\\\":true,\\\"size\\\":\\\"xs\\\",\\\"margin\\\":\\\"md\\\",\\\"flex\\\":7,\\\"color\\\":\\\"#222222\\\",\\\"align\\\":\\\"start\\\"}],\\\"margin\\\":\\\"xxl\\\"},{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"baseline\\\",\\\"contents\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"배포정보\\\",\\\"wrap\\\":true,\\\"flex\\\":3,\\\"size\\\":\\\"xs\\\",\\\"color\\\":\\\"#222222\\\"},{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"${EnvType}\\\",\\\"wrap\\\":true,\\\"size\\\":\\\"xs\\\",\\\"margin\\\":\\\"md\\\",\\\"color\\\":\\\"#0E71EB\\\",\\\"flex\\\":7}],\\\"margin\\\":\\\"md\\\"},{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"baseline\\\",\\\"contents\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"빌드결과\\\",\\\"wrap\\\":true,\\\"flex\\\":3,\\\"size\\\":\\\"xs\\\",\\\"color\\\":\\\"#222222\\\"},{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"${buildResultString}\\\",\\\"wrap\\\":true,\\\"size\\\":\\\"xs\\\",\\\"margin\\\":\\\"md\\\",\\\"flex\\\":7,\\\"color\\\":\\\"#222222\\\"}],\\\"margin\\\":\\\"md\\\"},{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"baseline\\\",\\\"contents\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"빌드버전\\\",\\\"wrap\\\":true,\\\"flex\\\":3,\\\"size\\\":\\\"xs\\\",\\\"color\\\":\\\"#222222\\\"},{\\\"type\\\":\\\"text\\\",\\\"wrap\\\":true,\\\"size\\\":\\\"xs\\\",\\\"margin\\\":\\\"md\\\",\\\"flex\\\":7,\\\"text\\\":\\\"v${appVersion}\\\",\\\"color\\\":\\\"#222222\\\"}],\\\"margin\\\":\\\"md\\\"},{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"baseline\\\",\\\"contents\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"빌드브랜치\\\",\\\"wrap\\\":true,\\\"flex\\\":3,\\\"size\\\":\\\"xs\\\",\\\"color\\\":\\\"#222222\\\"},{\\\"type\\\":\\\"text\\\",\\\"wrap\\\":true,\\\"size\\\":\\\"xs\\\",\\\"margin\\\":\\\"md\\\",\\\"flex\\\":7,\\\"text\\\":\\\"${gitBranchName}\\\",\\\"color\\\":\\\"#222222\\\"}],\\\"margin\\\":\\\"md\\\"}],\\\"spacing\\\":\\\"sm\\\"},\\\"footer\\\":{\\\"type\\\":\\\"box\\\",\\\"layout\\\":\\\"vertical\\\",\\\"contents\\\":[{\\\"type\\\":\\\"button\\\",\\\"style\\\":\\\"primary\\\",\\\"action\\\":{\\\"type\\\":\\\"uri\\\",\\\"label\\\":\\\"다운로드\\\",\\\"uri\\\":\\\"http://10.27.0.65:3000/\\\"},\\\"color\\\":\\\"#0E71EB\\\",\\\"height\\\":\\\"sm\\\"}]}}}}\""
	return jsonStr;
}


def getNaverWorksAccessToken() {
    if(!shouldMakeTokenRequest()){
        File naverWorksTokenFile = new File(naverWorksTokenFilePath)
        if(naverWorksTokenFile.exists())
            return naverWorksTokenFile.text
    }

	def response = bat(
		label: '',
		returnStdout:true,
		script: "curl --location \"https://email.com2us.com/common/naverssoif.nsf/commonjwt\" --header \"ConsumerKey: 4vlk8ywsmjauruj9hoqt\""
	)
	response = response.substring(response.indexOf('{'))
	echo "RESPONSE = $response"

	def jsonSlurper = new JsonSlurper();
	def result = jsonSlurper.parseText(response);
	def accessToken = ""
	if (result && !result.error) {
		accessToken = result.access_token;
		echo "NAVER WORKS ACCESS TOKEN = $accessToken"
	}
	
    File naverWorksTokenDateFile = new File(naverWorksTokenDateFilePath)
    naverWorksTokenDateFile.write("${currentDateTime}")
    
    File naverWorksTokenFile = new File(naverWorksTokenFilePath)
    naverWorksTokenFile.write(accessToken)
            
	return accessToken
}


LAUNCHER_API_BASE = "https://sandbox-hivecrossplay.qpyou.cn"
def getLauncherReleaseAPI(){
	return "${LAUNCHER_API_BASE}/app/deploy/status/change"
}

void printBuildInfo(){
	script{
		log = "\n@ Build Info\n"

		// path
		log += "\n@@ path\n"
		log += " + UNITY3D_EXECUTABLE : ${UNITY3D_EXECUTABLE}\n"
		log += " + WORKSPACE : ${WORKSPACE}\n"
		log += " + executablePath : ${executablePath}\n"		// ex) "~\C2VClient_Main\Build\Executable\"
		log += " + buildPath : ${buildPath}\n"					// ex) "~\C2VClient_Main\Build\Executable\Windows64\debug"
		log += " + buildPathBase : ${buildPathBase}\n"			// ex) "Build\Executable\Windows64\debug"
		log += " + buildExeFilePath : ${buildExeFilePath}\n"	// ex) "~\C2VClient_Main\Build\Executable\Windows64\debug\Com2Verse.exe"
		log += " + isccExePath : ${isccExePath}\n"				// ex) "C:\Program Files (x86)\Inno Setup 5\iscc.exe"
		log += " + msiWrapperExePath : ${msiWrapperExePath}\n"	// ex) "C:\Program Files (x86)\MSI Wrapper\MsiWrapperBatch.exe"
		log += " + issFilePath : ${issFilePath}\n"				// ex) "~\JenkinsScripts\inno_setup_script.iss"
		log += " + codeSignFilePath : ${codeSignFilePath}\n"	// ex) "~\JenkinsScripts\CodeSign_Windows.bat"
		log += " + launcherPatchMakerFilePath : ${launcherPatchMakerFilePath}\n"
		log += " + launcherUploaderFilePath : ${launcherUploaderFilePath}\n"
		log += " + outputPath : ${outputPath}\n"				// ex) "~\C2VClient_Main\Build\OutPut\"
		log += " + exeFilePath : ${exeFilePath}\n"				// ex) "~\C2VClient_Main\Build\OutPut\Com2Verse.exe"
		log += " + msiFilePath : ${msiFilePath}\n"				// ex) "~\C2VClient_Main\Build\OutPut\Com2Verse.msi"
		log += " + zipFilePath : ${zipFilePath}\n"				// ex) "~\C2VClient_Main\Build\OutPut\Com2Verse.zip"
		log += " + codeSignTargetFilePath : ${codeSignTargetFilePath}\n"
		log += " + appcenterUploadTargetFilePath : ${appcenterUploadTargetFilePath}\n"
		log += " + buildPathBase : ${buildPathBase}\n"
		log += " + zipFilePathBase : ${zipFilePathBase}\n"		// ex) "Build\OutPut\Com2Verse.zip"

		// params
		log += "\n@@ params\n"
		log += " + AppName : ${AppName}\n"						// "Com2Verse" , "MetaversePlatform"
		log += " + AppID : ${AppID}\n"							// "QA.windows.hivepc.global.normal"
																// "com2verse.env.windows.hivepc.global.normal.dev"
																// "com2verse.env.windows.hivepc.global.normal.qa"
		log += " + HiveEnv : ${HiveEnv}\n"						// "sandbox", "live"
		// log += " + UploadType : ${UploadType}\n"				// "release", "patch"
		log += " + IsHiveRelease : ${IsHiveRelease}\n"			// true : release, false : patch
		log += " + DistributeType : ${DistributeType}\n"		// "debug", "release"
		log += " + ScriptingBackend : ${ScriptingBackend}\n"	// "il2cpp", "mono"
		log += " + EnvType : ${EnvType}\n"						// dev, qa, staging, production
		log += " + AssetBuildType : ${AssetBuildType}\n"		// local, remote, remote test, editor hosted
		log += " + GitBranch : ${GitBranch}\n"
		log += " + PackageType : ${PackageType}\n"
		log += " + Exe Installer : ${getOX(isExeInstaller)}\n"
		log += " + Msi : ${getOX(isMsi)}\n"
		log += " + CodeSign : ${getOX(isCodeSign)}\n"
		log += " + Zip : ${getOX(isZip)}\n"
		log += " + AppCenter : ${getOX(isAppCenter)}\n"
		log += " + Launcher : ${getOX(isLauncher)}\n"
		log += " + AssetBundleUpload : ${getOX(isAssetBundleUpload)}\n"
		log += " + AppCenterDistributeGroup : ${AppCenterDistributeGroup}\n"
		log += " + AppCenterComment : ${AppCenterComment}\n"
		log += " + AppCenterAppName : ${AppCenterAppName}\n"
		log += " + getAppCenterDistributeGroup() : ${getAppCenterDistributeGroup()}\n"
		log += " + getAppCenterComment() : ${getAppCenterComment()}\n"
		log += " + getAppCenterAppName() : ${getAppCenterAppName()}\n"
		log += " + getAppCenterCommentFilePath() : ${getAppCenterCommentFilePath()}\n"
		log += " + CleanBuild : ${CleanBuild}\n"
		log += " + IsForceSingleInstance : ${IsForceSingleInstance}\n"
		log += " + IsForceEnableAppInfo : ${ForceEnableAppInfo}\n"
		log += " + IsForceEnableSentry : ${ForceEnableSentry}\n"

		// git info
		log += "\n@@ git info\n"
		log += " + gitCommitHash : ${gitCommitHash}\n"
		log += " + getCommitHashLink() : ${getCommitHashLink()}\n"
		log += " + gitRevisionCount : ${gitRevisionCount}\n"
		log += " + gitBranchName : ${gitBranchName}\n"

		// log
		log += "\n@@ log\n"
		log += " + buildBeforeLogFile : ${buildBeforeLogFile}\n"
		log += " + buildLogFile : ${buildLogFile}\n"
		log += " + assetBundleUploadLogFile : ${assetBundleUploadLogFile}\n"

		// make info
		log += "\n@@ make info\n"
		log += " + envTypeFull : ${envTypeFull}\n"			// dev, qa, staging , dev_integration, production
		log += " + envTypeShort : ${envTypeShort}\n"		// d, s, p
		log += " + appVersion : ${appVersion}\n"			// 0.1.12234_d
		log += " + appVersionMsi : ${appVersionMsi}\n"		// 0.1.12234.0

		// launcher
		log += "\n@@ launcher\n"
		log += " + appVersionPre : ${appVersionPre}\n"							// ex) "0.1.23786_d"
		log += " + hiveEnvParam : ${hiveEnvParam}\n"							// ex) -e=SANDBOX
		log += " + hiveReleaseParam : ${hiveReleaseParam}\n"					// ex) --release
		log += " + preVersionFilePath : ${preVersionFilePath}\n"				// ex) "D:\Builds\Windows\dev\pre_vesion.txt"
		log += " + launcherPreBuildPath : ${launcherPreBuildPath}\n"			// ex) "D:\Builds\Windows\dev\0.1.23786_d"
		log += " + launcherCurBuildPath : ${launcherCurBuildPath}\n"			// ex) "D:\Builds\Windows\dev\0.1.23787_d"
		log += " + launcherCurMetadataPath : ${launcherCurMetadataPath}\n"		// ex) "D:\Builds\Windows\dev\0.1.23787_d_metadatas"
		log += " + launcherWorkPath : ${launcherWorkPath}\n"					// ex) "~\C2VClient_Main\Launcher\"
		log += " + launcherPreWorkPath : ${launcherPreWorkPath}\n"				// ex) "~\C2VClient_Main\Launcher\Pre_Version_Folder"
		log += " + launcherCurWorkPath : ${launcherCurWorkPath}\n"				// ex) "~\C2VClient_Main\Launcher\Cur_Version_Folder"
		log += " + launcherPatchWorkPath : ${launcherPatchWorkPath}\n"			// ex) "~\C2VClient_Main\Launcher\Patch_Files_Folder"
		log += " + launcherMetadatasWorkPath : ${launcherMetadatasWorkPath}\n"	// ex) "~\C2VClient_Main\Launcher\metadatas"
		log += " + configPath : ${configPath}\n"								// ex) "~\C2VClient_Main\Config"

		// time
		log += "\n@@ time\n"
		log += " + todayTime : ${todayTime}\n"							// 2211291504
		log += " + appCenterCommentTime : ${appCenterCommentTime}\n"	// 22/11/29 15:04:46

		echo "${log}"
	}
}


void makeFolder(String path, boolean recursive = true){
	script{
		echo "makeFolder : ${path} , recursive(${recursive})"
		File folder = new File(path)
		if(!folder.exists()){
			if(recursive){
				folder.mkdirs()
			}
			else{
				folder.mkdir()
			}
		}
	}
}

void makeFolderWithClean(String path){
	script{
		echo "makeFolderWithClean : ${path}"
		File folder = new File(path)
		if(folder.exists()){
			FileUtils.cleanDirectory(folder)
			folder.deleteDir()
		}
		folder.mkdir()
		// folder.mkdirs()
	}
}

void deleteFolder(String path){
	script{
		echo "deleteFolder : ${path}"
		File folder = new File(path)
		if(folder.exists()){
			FileUtils.cleanDirectory(folder)
			folder.deleteDir()
		}
	}
}

void copyFolder(String src, String dst){
	script{
		echo "copyFolder\n + src : ${src}\n + dst : ${dst}"
		File srcFolder = new File(src)
		File dstFolder = new File(dst)
		if(srcFolder.exists() && dstFolder.exists()){
			FileUtils.copyDirectory(srcFolder, dstFolder)
		}
		else{
		    echo "copy Folder Failed. Src Exists : ${srcFolder.exists()} Dst Exists : ${dstFolder.exists()}"
		}
	}
}

void copyFile(String src, String dst){
	script{
		echo "copyFile\n + src : ${src}\n + dst : ${dst}"
		File srcFile = new File(src)
		File dstFile = new File(dst)
		if(srcFile.exists() && !dstFile.exists()){
			FileUtils.copyFile(srcFile, dstFile)
		}
	}
}

void deleteFile(String src){
    script{
        echo "deleteFile \n + src : ${src}"
        
        File srcFile = new File(src)
        if(srcFile.exists()){
            FileUtils.delete(srcFile)
        }
            
    }
}

pipeline{ 
	agent{
		label 'Windows'
	}

	environment{
		// unity tool installation
		UNITY3D_EXECUTABLE = "C:\\Program Files\\Unity\\Hub\\Editor\\2021.3.17f1\\Editor\\Unity.exe"
	}

	stages{
		stage('Init'){
			steps{
				echo "Init (git repogitory)"
				initInfo()
				// gitUpdate()
				gitInfo()
				initAfter()
				printBuildInfo()

				// delete work folder
				script{
					// logs
					echo "getChangeLogs() : \n\n${getChangeLogs()}"
					echo "getAppCenterCommentString() : \n\n${getAppCenterCommentString()}"

					echo "delete work folder"
					deleteFolder("${executablePath}")
					deleteFolder("${outputPath}")
				}
			}
		}
		stage('Build Before'){
			steps{
				script{
					if(isBuildBefore){
						echo "Build Before (Unity3D)"

						// build before
						bat "\"${UNITY3D_EXECUTABLE}\" -quit -batchmode -projectPath \"${WORKSPACE}\" -logFile \"${buildBeforeLogFile}\" -executeMethod Com2VerseEditor.Build.BuildScript.BuildWindowsBeforeForJenkins -appName \"${AppName}\" -distributeType \"${DistributeType}\" -env \"${EnvType}\" -scriptingBackend \"${ScriptingBackend}\" -assetBuildType \"${AssetBuildType}\" -gitBuildBranch \"${GitBranch}\" -enableLogging \"${EnableLogging}\" -cleanBuild \"${CleanBuild}\" -forceEnableAppInfo \"${ForceEnableAppInfo}\" -appId \"${AppID}\" -forceEnableSentry \"${ForceEnableSentry}\" -packageType \"${PackageType}\" -hiveEnv \"${HiveEnv}\""
					}
				}
			}
		}
		stage('Build'){
			steps{
				script{
					if(isBuild){
						echo "Build (Unity3D)"

						// build
						bat "\"${UNITY3D_EXECUTABLE}\" -quit -batchmode -projectPath \"${WORKSPACE}\" -logFile \"${buildLogFile}\" -executeMethod Com2VerseEditor.Build.BuildScript.BuildWindowsForJenkins -appName \"${AppName}\" -distributeType \"${DistributeType}\" -env \"${EnvType}\" -scriptingBackend \"${ScriptingBackend}\" -assetBuildType \"${AssetBuildType}\" -gitBuildBranch \"${GitBranch}\" -enableLogging \"${EnableLogging}\" -cleanBuild \"${CleanBuild}\" -forceEnableAppInfo \"${ForceEnableAppInfo}\" -appId \"${AppID}\" -forceEnableSentry \"${ForceEnableSentry}\" -packageType \"${PackageType}\" -hiveEnv \"${HiveEnv}\""
						
						// pdb files
                        def pdbBackupFolderPath = "D:\\PDBFolder\\${getBuildTargetOSStr()}\\${EnvType}\\${DistributeType}"
                        def buildPdbFolderPath = "${buildPath}\\${appName}_BackUpThisFolder_ButDontShipItWithYourGame"

                        deleteFolder(pdbBackupFolderPath)
						makeFolder(pdbBackupFolderPath)
						
						// live
						copyFile("${buildPath}\\baselib_Win64_Master_il2cpp_x64.pdb", "${pdbBackupFolderPath}\\baselib_Win64_Master_il2cpp_x64.pdb")
                        copyFile("${buildPath}\\baselib_Win64_Master_il2cpp_x64_s.pdb", "${pdbBackupFolderPath}\\baselib_Win64_Master_il2cpp_x64_s.pdb")
                        copyFile("${buildPath}\\UnityPlayer_Win64_player_il2cpp_x64.pdb", "${pdbBackupFolderPath}\\UnityPlayer_Win64_player_il2cpp_x64.pdb")
                        copyFile("${buildPath}\\UnityPlayer_Win64_player_il2cpp_x64_s.pdb", "${pdbBackupFolderPath}\\UnityPlayer_Win64_player_il2cpp_x64_s.pdb")
                        copyFile("${buildPath}\\WindowsPlayer_player_Master_il2cpp_x64.pdb", "${pdbBackupFolderPath}\\WindowsPlayer_player_Master_il2cpp_x64.pdb")
                        copyFile("${buildPath}\\WindowsPlayer_player_Master_il2cpp_x64_s.pdb", "${pdbBackupFolderPath}\\WindowsPlayer_player_Master_il2cpp_x64_s.pdb")
						
						// develop 
						copyFile("${buildPath}\\baselib_Win64_Release_il2cpp_x64.pdb", "${pdbBackupFolderPath}\\baselib_Win64_Release_il2cpp_x64.pdb")
						copyFile("${buildPath}\\baselib_Win64_Release_il2cpp_x64_s.pdb", "${pdbBackupFolderPath}\\baselib_Win64_Release_il2cpp_x64_s.pdb")
						copyFile("${buildPath}\\GameAssembly.pdb", "${pdbBackupFolderPath}\\GameAssembly.pdb")
						copyFile("${buildPath}\\UnityPlayer_Win64_player_development_il2cpp_x64.pdb", "${pdbBackupFolderPath}\\UnityPlayer_Win64_player_development_il2cpp_x64.pdb")
						copyFile("${buildPath}\\UnityPlayer_Win64_player_development_il2cpp_x64_s.pdb", "${pdbBackupFolderPath}\\UnityPlayer_Win64_player_development_il2cpp_x64_s.pdb")
						copyFile("${buildPath}\\WindowsPlayer_player_Release_il2cpp_x64.pdb", "${pdbBackupFolderPath}\\WindowsPlayer_player_Release_il2cpp_x64.pdb")
						copyFile("${buildPath}\\WindowsPlayer_player_Release_il2cpp_x64_s.pdb", "${pdbBackupFolderPath}\\WindowsPlayer_player_Release_il2cpp_x64_s.pdb")
						copyFile("${buildPath}\\UnityCrashHandler64.pdb", "${pdbBackupFolderPath}\\UnityCrashHandler64.pdb")
						copyFile("${buildPath}\\UnityCrashHandler64_s.pdb", "${pdbBackupFolderPath}\\UnityCrashHandler64_s.pdb")
						
						copyFolder(buildPdbFolderPath, pdbBackupFolderPath)
                        // delete folder - do not ship folder
                        deleteFolder("${buildPath}\\${appName}_BackUpThisFolder_ButDontShipItWithYourGame")
                        deleteFolder("${buildPath}\\${appName}_BurstDebugInformation_DoNotShip")
                        deleteFile("${buildPath}\\baselib_Win64_Release_il2cpp_x64.pdb")
                        deleteFile("${buildPath}\\baselib_Win64_Release_il2cpp_x64_s.pdb")
                        deleteFile("${buildPath}\\GameAssembly.pdb")
                        deleteFile("${buildPath}\\UnityPlayer_Win64_player_development_il2cpp_x64.pdb")
                        deleteFile("${buildPath}\\UnityPlayer_Win64_player_development_il2cpp_x64_s.pdb")
                        deleteFile("${buildPath}\\WindowsPlayer_player_Release_il2cpp_x64.pdb")
                        deleteFile("${buildPath}\\WindowsPlayer_player_Release_il2cpp_x64_s.pdb")
                        deleteFile("${buildPath}\\UnityCrashHandler64.pdb")
                        deleteFile("${buildPath}\\UnityCrashHandler64_s.pdb")
                        
                        // live
                        deleteFile("${buildPath}\\baselib_Win64_Master_il2cpp_x64.pdb")
                        deleteFile("${buildPath}\\baselib_Win64_Master_il2cpp_x64_s.pdb")
                        deleteFile("${buildPath}\\UnityPlayer_Win64_player_il2cpp_x64.pdb")
                        deleteFile("${buildPath}\\UnityPlayer_Win64_player_il2cpp_x64_s.pdb")
                        deleteFile("${buildPath}\\WindowsPlayer_player_Master_il2cpp_x64.pdb")
                        deleteFile("${buildPath}\\WindowsPlayer_player_Master_il2cpp_x64_s.pdb")

					}
				}
			}
		}
        stage('Protect'){
            steps{
                script{
                    if(isProtect && "${envTypeFull}" == "production"){
                        echo "Protect (Themida)"
                        
                        def gameAssemblyDllBackupFolderPath = "D:\\Com2VerseBackup\\GameAssembly\\${getBuildTargetOSStr()}\\${EnvType}\\${DistributeType}\\${appVersion}"
                        def themidaOutputTempFolder = "${buildPath}\\ThemidaTemp"
                        def themidaOutputPath = "${themidaOutputTempFolder}\\GameAssembly.dll"
                        
						deleteFolder(gameAssemblyDllBackupFolderPath)
                        makeFolder(themidaOutputTempFolder)
						makeFolder(gameAssemblyDllBackupFolderPath)

                        cmdProtect = "\"${themidaExePath}\" /protect \"${themidaSettingsPath}\" /inputfile \"${buildInputGameAssemblyDllPath}\" /outputfile \"${themidaOutputPath}\""
                        echo "${cmdProtect}"
                        bat "${cmdProtect}"
                        
                        // 원본 파일 제거 및 백업
                        copyFile("${buildInputGameAssemblyDllPath}", "${gameAssemblyDllBackupFolderPath}\\GameAssembly_Backup.dll")
                        deleteFile("${buildInputGameAssemblyDllPath}")
                        // themida 적용된 assembly 적용
                        copyFile(themidaOutputPath, "${buildPath}\\GameAssembly.dll")
                        // 제거
                        deleteFolder("${themidaOutputTempFolder}")
                    }
                }
            }
        }
		stage('Installer'){
			steps{
				script{
					if(isExeInstaller){
						echo "Make Installer (Innosetup)"

						// installer
						// "iscc.exe" /Qp /O".\Build\OutPut" /DMyAppVersion="0.1.12234_d" /DMySrcDir=".\BuildExecutable\Windows64\debug" ".\JenkinsScripts\inno_setup_script.iss"
						// ex) "C:\Program Files (x86)\Inno Setup 6\iscc.exe" /Qp /O"D:\Jenkins\workspace\Win_Pipeline_Test_02\Build\OutPut" /DMyAppVersion="0.1.12234_d" /DMySrcDir="D:\Jenkins\workspace\Win_Pipeline_Test_02\Build\Executable\Windows64\debug" "D:\Jenkins\workspace\Win_Pipeline_Test_02\JenkinsScripts\inno_setup_script.iss"
						bat "\"${isccExePath}\" /Qp /O\"${outputPath}\" /DMyAppVersion=\"${appVersion}\" /DMySrcDir=\"${buildPath}\" \"${issFilePath}\""
					}
				}
			}
		}
		stage('Msi config'){
			steps{
				script{
					if(isMsi){
						echo "Msi Config"

						// read
						def xmlFile = new XmlSlurper().parse("${msiWrapperXmlFilePath}")

						// ex) ~\Build\OutPut\Com2Verse.exe
						xmlFile.WrappedInstaller.Executable['@FileName'] = "${exeFilePath}".toString()
						echo "exeFilePath : ${exeFilePath}"
						
						// ex) ~\Build\OutPut\Com2Verse.msi
						xmlFile.Installer.Output['@FileName'] = "${msiFilePath}".toString()
						echo "msiFilePath : ${msiFilePath}"

						// ex) 0.1.12267.0
						xmlFile.Installer.ProductVersion['@Value'] = "${appVersionMsi}".toString()
						echo "appVersionMsi : ${appVersionMsi}"

						// write
						def saveFileWriter = new FileWriter(new File("${msiWrapperXmlFilePath}"))
						XmlUtil xmlUtil = new XmlUtil()
						xmlUtil.serialize(xmlFile, saveFileWriter)
						saveFileWriter.close()
					}
				}
			}
		}
		stage('Exe to Msi'){
			steps{
				script{
					if(isMsi){
						echo "Exe to Msi"
						echo "msiWrapperExePath : \"${msiWrapperExePath}\""
						echo "msiWrapperXmlFilePath : \"${msiWrapperXmlFilePath}\""
						echo "exeFilePath : \"${exeFilePath}\""
						echo "msiFilePath : \"${msiFilePath}\""

						// exe to msi
						// ex) "C:\Program Files (x86)\MSI Wrapper\MsiWrapperBatch.exe" config="msiwrapper.xml"
						bat "\"${msiWrapperExePath}\" config=\"${msiWrapperXmlFilePath}\""
					}
				}
			}
		}
		stage('Code Signing'){
			steps{
				script{
					if(isCodeSign){
						echo "Code Signing (signtool)"
						echo "codeSignFilePath : \"${codeSignFilePath}\""
						echo "codeSignTargetFilePath : \"${codeSignTargetFilePath}\""

						// CodeSign
						bat "\"${codeSignFilePath}\" \"${codeSignTargetFilePath}\""
						bat "\"${codeSignFilePath}\" \"${buildGameAssemblyDllFilePath}\""
					}
				}
			}
		}
		stage('Zip'){
			steps{
				script{
					if(isZip){
						echo "Zip"
						echo "zipFilePathBase : \"${zipFilePathBase}\""
						echo "buildPathBase : \"${buildPathBase}\""

						// zip
						zip zipFile: "${zipFilePathBase}", dir: "${buildPathBase}"
					}
				}
			}
		}
		stage('Appcenter Upload'){
			steps{
				script{
					if(isAppCenter){
						echo "Appcenter Upload"
						echo "appcenterUploadTargetFilePath : \"${appcenterUploadTargetFilePath}\""
						
						// make comment file
						writeFile(file: getAppCenterCommentFilePath(), text: getAppCenterCommentString(true))

						// upload
						bat "appcenter distribute release -a \"${getAppCenterAppName()}\" -f \"${appcenterUploadTargetFilePath}\" --group \"${getAppCenterDistributeGroup()}\" --build-version ${appVersion} -R \"${getAppCenterCommentFilePath()}\" --token ${env.APPCENTER_ACCESS_TOKEN}"
					}
				}
			}
		}
		stage('Patch Maker'){
			steps{
				script{
					if(isLauncher && !isLauncherError){
						echo "Patch Maker"
						echo "launcherPatchMakerFilePath : \"${launcherPatchMakerFilePath}\""
						echo "pre version path : \"${launcherPreWorkPath}\""
						echo "cur version path : \"${launcherCurWorkPath}\""
						echo "patch file path : \"${launcherPatchWorkPath}\""

						// delete work folder
						deleteFolder("${launcherWorkPath}")

						// mkdir
						makeFolder("${launcherPreWorkPath}")
						makeFolder("${launcherCurWorkPath}")
						makeFolder("${launcherPatchWorkPath}")
						makeFolder("${launcherMetadatasWorkPath}")

						// prePath
						copyFolder("${launcherPreBuildPath}" , "${launcherPreWorkPath}")
						
						// nextPath
						copyFolder("${buildPath}" , "${launcherCurWorkPath}")
						
						// copy config file
						copyFile("${configPath}\\meta.json", "${launcherCurWorkPath}\\meta.json")
						copyFile("${configPath}\\${envTypeFull}_icon.ico", "${launcherCurWorkPath}\\icon.ico")

						// ~/cpflPtc.exe
						// -e=SANDBOX
						// -prePath=${launcherPreWorkPath}
						// -nextPath=${launcherCurWorkPath}
						// -patchPath=${launcherPatchWorkPath}
						// -appid=${AppID}
						// -version=${appVersionPre}
						// -runbatch

						// run
						cmdPatchMaker = "\"${launcherPatchMakerFilePath}\" ${hiveEnvParam} -prePath=\"${launcherPreWorkPath}\" -nextPath=\"${launcherCurWorkPath}\" -patchPath=\"${launcherPatchWorkPath}\" -appid=${AppID} -version=${appVersionPre} -runbatch"
						echo "${cmdPatchMaker}"
						bat "${cmdPatchMaker}"
					}
				}
			}
		}
		stage('Uploader'){
			steps{
				script{
					if(isLauncher && !isLauncherError){
						echo "Uploader"
						echo "launcherUploaderFilePath : \"${launcherUploaderFilePath}\""
						echo "upload path : \"${launcherCurWorkPath}\""
						echo "patch file path : \"${launcherPatchWorkPath}\""

						// ~/cpflupl.exe
						// -e=SANDBOX
						// --release
						// -appid=${AppID}
						// -version=${appVersion}
						// --notfirstupload
						// -platform=${getLauncherPlatform()}
						// --filepath=${launcherCurWorkPath}
						// --filepatchpath=${launcherPatchWorkPath}
						// --metadatapath=${launcherMetadatasWorkPath}
						// -runbatch

						// run
						cmdUploader = "\"${launcherUploaderFilePath}\" ${hiveEnvParam} ${hiveReleaseParam} -appid=${AppID} -version=${appVersion} --notfirstupload -platform=${getLauncherPlatform()} --filepath=\"${launcherCurWorkPath}\" --filepatchpath=\"${launcherPatchWorkPath}\" --metadatapath=\"${launcherMetadatasWorkPath}\" -runbatch"
						echo "${cmdUploader}"
						bat "${cmdUploader}"

						// copy
						// - build folder
						makeFolder("${launcherCurBuildPath}")
						copyFolder("${launcherCurWorkPath}" , "${launcherCurBuildPath}")
						// - meta file
						makeFolder("${launcherCurMetadataPath}")
						copyFolder("${launcherMetadatasWorkPath}" , "${launcherCurMetadataPath}")

						// version file update
						File preVersionFile = new File(preVersionFilePath)
						preVersionFile.write("${appVersion}")
					}
				}
			}
		}
		stage('Sandbox Release'){
			steps{
				script{
					if(isLauncher && isHiveEnvSandbox && !isLauncherError){
						echo "Release Hive Sandbox"
						def payload = """{\\"project\\":\\"${AppID}\\",\\"version\\":\\"${appVersion}\\",\\"platform\\":\\"${getLauncherPlatform()}\\",\\"deployTime\\":${getCurrentTime()}}"""
						releaseError = bat(script: """@echo off
							curl -d "${payload}" -H "Content-Type: application/json" -X POST ${getLauncherReleaseAPI()}""", returnStdout: true)
						if(releaseError){
							// ex) {"error":{"reason":"Failed to processing. (The requested version is already in distribution.)","code":495}}
							echo "releaseError : ${releaseError}"
							def jsonSlurper = new JsonSlurper()
							def object = jsonSlurper.parseText(releaseError)
							if(object && object.error){
								error("Launcher Release Fail : \n${releaseError}")
							}
						}
					}

					if(isLauncher && isLauncherError){
						error("Launcher Fail : \n${isLauncherErrorMsg}")
					}
				}
			}
		}
		stage('AssetBundle Upload'){
			steps{
				script{
					if(isAssetBundleUpload && "${AssetBuildType}".toLowerCase() == "remote"){
						echo "AssetBundle Upload"
						bat "\"${UNITY3D_EXECUTABLE}\" -quit -batchmode -projectPath \"${WORKSPACE}\" -logFile \"${assetBundleUploadLogFile}\" -executeMethod Com2VerseEditor.Build.BuildScript.UploadAssetBundleForJenkins -appName \"${AppName}\" -distributeType \"${DistributeType}\" -env \"${EnvType}\" -scriptingBackend \"${ScriptingBackend}\" -assetBuildType \"${AssetBuildType}\" -gitBuildBranch \"${GitBranch}\" -enableLogging \"${EnableLogging}\" -cleanBuild \"${CleanBuild}\" -forceEnableAppInfo \"${ForceEnableAppInfo}\" -forceEnableSentry \"${ForceEnableSentry}\" -packageType \"${PackageType}\" -hiveEnv \"${HiveEnv}\""
					}
				}
			}
		}
	}
	post{
        always{
            echo "post : always"
            script{
                echo "post : always - script"
            }
        }
        success{
            echo "post : success"
            script{
                echo "post : success - script"

                sendEmails(true)
                sendNaverWorks(true) 
            }
        }
        failure{
            echo "post : failure"
            script{
                echo "post : failure - script"

                sendEmails(false)
                sendNaverWorks(false)
            }
        }
        aborted{
            echo "post : aborted"
            script{
                echo "post : aborted - script"
            }
        }
    }
}