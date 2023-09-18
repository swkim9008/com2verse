
import java.text.SimpleDateFormat

void initInfo(){
	echo "\n@ initInfo\n"
	initTime()
	initLogFileName()
}

todayTime = ""
void initTime(){
	echo "\n@ initTime\n"
	def date = new Date()
	def dateFormat = new SimpleDateFormat("yyMMddHHmm")
	todayTime = dateFormat.format(date)
}

void initLogFileName(){
	echo "\n@ initLogFileName\n"
	assetBundleBuildLogFile = "${WORKSPACE}/Logs/AssetBundle/${todayTime}_Build.log"
}

void printBuildInfo(){
	script{
		log = "\n@ Build Info\n"

		// path
		log += "\n@@ path\n"
		log += " + UNITY3D_EXECUTABLE : ${UNITY3D_EXECUTABLE}\n"
		log += " + WORKSPACE : ${WORKSPACE}\n"

		// log
		log += "\n@@ log\n"
		log += " + assetBundleBuildLogFile : ${assetBundleBuildLogFile}\n"

		echo "${log}"
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
				printBuildInfo()
			}
		}
		stage('Test API'){
			steps{
				script{
					echo "Test API"
					bat "\"${UNITY3D_EXECUTABLE}\" -quit -batchmode -projectPath \"${WORKSPACE}\" -logFile \"${assetBundleBuildLogFile}\" -executeMethod Com2VerseEditor.Build.BuildAssetBundle.UploadDev"
				}
			}
		}
	}
}