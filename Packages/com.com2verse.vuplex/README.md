# Vuplex 3D WebView (com.com2verse.vuplex)

## 개요
[클라이언트] Vuplex 3D WebView Asset 관련 모듈입니다.

## 설치
패키지 매니저에서 아래의 Git 주소로 패키지 추가
```
https://meta-bitbucket.com2us.com/scm/c2verse/com.com2verse.vuplex.git?path=Packages/com.com2verse.vuplex/
```

## Dependencies
com.unity.inputsystem
```
1.4.3
```

## 수정사항
Singleton Instance 초기화를 위해 _resetInstance() 메서드 추가
```
WindowsWebPlugins.cs
MacWebPlugins.cs
MockWebPlugin.cs
StandaloneCookieManager.cs
MockCookieManager.cs
KeyboardManager.cs
XritPointerEventHelper.cs
XRSettingsWrapper.cs
```

패키지화를 위해 Application.dataPath가 사용된 코드 수정
```
StandaloneVideoCodecsWindow.cs
WindowsBuildScript.cs
```

창 스크롤 및 커서 변경 방지를 위한 Prefab 수정
```
CanvasWebViewPrefab.prefab
WebViewPrefab.prefab
```

사용중인 WebView의 수를 세기 위해 코드 수정
```
BaseWebViewPrefab.cs
```

한글 입력 처리를 위해 코드 수정
```
NativeKeyboardListener.cs
```